"""由插件确定性处理账号绑定，不经模型提交绑定请求。"""
import re
from urllib.parse import urljoin, urlparse

import aiohttp

GUID = re.compile(r"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}")


class BindingError(Exception):
    def __init__(self, message="呜呜呜~ 请稍后再试喵"):
        super().__init__(message)


def parse_binding(text, woken=False):
    """返回绑定码；参数无效时返回空字符串，咨询类消息返回 None。

    仅提交完整 GUID。夹带 GUID 的说明文字和独立非法 ASCII 参数返回用法提示，
    其余自然语言交给 AI。普通消息不会被扫描或提取绑定码。
    """
    text = text.strip()
    if text.startswith("看板娘"):
        text, woken = text[3:].lstrip(" ，,：:"), True
    elif woken:
        text = text.lstrip(" ，,：:")
    if not woken or not text.startswith("绑定"):
        return None
    code = text[2:].strip()
    if not code:
        return None
    if GUID.fullmatch(code):
        return code.lower()
    if GUID.search(code) or (code.isascii() and not any(c.isspace() for c in code)):
        return ""
    return None


class BindingClient:
    def __init__(self, base_url, binding_token=""):
        if urlparse(base_url).scheme not in ("http", "https"):
            raise ValueError("api_base_url must use http or https")
        self.endpoint = urljoin(base_url.rstrip("/") + "/", "api/robot/v2/bind-qq")
        self.binding_token = binding_token
        self.session = None

    async def close(self):
        if self.session:
            await self.session.close()

    async def bind(self, code, sender_id):
        code = code.strip()
        if not GUID.fullmatch(code) or not sender_id.isascii() or not sender_id.isdecimal() or int(sender_id) <= 0:
            raise BindingError("请发送“看板娘绑定 绑定码”来绑定账号喵~")
        if not self.binding_token:
            raise BindingError("看板娘好像忘记了什么，请联系管理员喵~")
        if self.session is None:
            self.session = aiohttp.ClientSession(timeout=aiohttp.ClientTimeout(total=15))
        try:
            async with self.session.post(self.endpoint, json={"code": code.lower(), "senderQQ": int(sender_id)},
                                         headers={"Authorization": "Bearer " + self.binding_token},
                                         allow_redirects=False) as response:
                if response.status != 200:
                    raise BindingError()
                result = await response.json()
                if not isinstance(result, dict):
                    raise BindingError()
                successful = next((value for key, value in result.items() if key.lower() == "successful"), None)
                if type(successful) is not bool:
                    raise BindingError()
                return "o(〃＾▽＾〃)o 成功绑定账号" if successful else "＞﹏＜ 绑定失败了喵~ 请在主站重新获取绑定码后再试。"
        except (aiohttp.ClientError, TimeoutError, ValueError):
            # 只请求一次；错误提示不包含服务端正文、绑定码或凭据。
            raise BindingError() from None
