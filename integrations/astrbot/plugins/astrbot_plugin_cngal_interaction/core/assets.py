"""读取素材目录，并构造单个素材的消息链。"""
import json
import logging
from pathlib import Path
from types import MappingProxyType

logger = logging.getLogger(__name__)
USAGE_KINDS = {"expression": "image", "illustration": "image", "voice": "record"}


def catalog(path):
    """插件加载时读取一次；配置缺失或无效时仅停用素材功能。"""
    path = Path(path)
    try:
        assets = json.loads(path.read_text(encoding="utf-8-sig"))
        if not isinstance(assets, dict):
            raise ValueError("素材目录必须是 JSON 对象")
        for name, asset in assets.items():
            if not name.strip() or not isinstance(asset, dict):
                raise ValueError("素材名称不能为空，每项素材必须是对象")
            usage = asset.get("usage")
            if not isinstance(usage, str) or usage not in USAGE_KINDS or asset.get("kind") != USAGE_KINDS[usage]:
                raise ValueError("素材的 kind 与 usage 必须匹配")
            if not isinstance(asset.get("url"), str) or not asset["url"].strip():
                raise ValueError("素材的 url 必须是非空字符串")
        return MappingProxyType({name: MappingProxyType(asset) for name, asset in assets.items()})
    except FileNotFoundError:
        return MappingProxyType({})
    except (OSError, UnicodeError, ValueError) as exc:
        logger.error("素材配置无效，已停用素材功能：%s（%s）", path, exc)
        return MappingProxyType({})


def asset_chain(asset_id, assets):
    """每次构造新的消息组件，避免原生发送流程修改缓存中的组件。"""
    from astrbot.api.event import MessageChain
    from astrbot.api.message_components import Image, Record
    asset = assets[asset_id]
    component = Image.fromURL(asset["url"]) if asset["kind"] == "image" else Record(file=asset["url"])
    return MessageChain([component])
