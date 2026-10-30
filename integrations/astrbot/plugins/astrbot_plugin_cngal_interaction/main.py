import sys
import json
from datetime import date, datetime
from zoneinfo import ZoneInfo

from astrbot.api import AstrBotConfig
from astrbot.api.event import AstrMessageEvent, filter
from astrbot.api.provider import ProviderRequest
from astrbot.api.star import Context, Star, StarTools, register
from astrbot.api.message_components import At, Plain
from .core.binding import BindingClient, BindingError, parse_binding
from .core.assets import catalog, asset_chain
from .core.native import NativePolicy, ai_enabled
from .core.rate_limit import InteractionLimiter


def current_text(event):
    return "".join(part.text for part in event.get_messages() if isinstance(part, Plain)).lstrip()


def explicit_ai_trigger(event):
    """仅当前正文和 @ 可触发唤醒，引用消息不参与判断。"""
    parts = event.get_messages()
    text = current_text(event)
    content = text[3:].lstrip(" ，,：:") if text.startswith("看板娘") else text
    if content.startswith("/"):
        return False
    return event.is_private_chat() or "看板娘" in text or any(
        isinstance(part, At) and str(part.qq) == str(event.get_self_id()) for part in parts)


class EarlyQQPolicy(filter.CustomFilter):
    """在原生唤醒阶段检查权限，早于可能发送提示的内容检查。"""
    policy = None

    def filter(self, event, cfg):
        if event.get_platform_name() != "aiocqhttp":
            return True
        # 原生唤醒流程可能移除 message_str 中的前缀。
        # 解析原始 Plain 正文，避免旧的斜杠前缀把消息误判为绑定请求。
        binding = parse_binding(current_text(event), explicit_ai_trigger(event))
        event.set_extra("cngal_binding", binding)
        scope = (self.policy.binding_scope(event) if binding is not None else self.policy.interaction_scope(event)) if self.policy else False
        if not scope:
            event.stop_event()
            # 阻止原生命令过滤器在同一阶段继续回复。
            event.is_at_or_wake_command = False
            return False
        if binding is None and explicit_ai_trigger(event):
            event.is_at_or_wake_command = True
        return True


@register("astrbot_plugin_cngal_interaction", "CnGal", "CnGal AI 交互与绑定", "1.0.0")
class CnGalInteraction(Star):
    def __init__(self, context: Context, config: AstrBotConfig):
        super().__init__(context, config)
        rate_limit = config.get("rate_limit", {})
        if not isinstance(rate_limit, dict):
            raise ValueError("rate_limit 必须为配置对象")
        self.limiter = InteractionLimiter(**rate_limit)
        self.assets = catalog(StarTools.get_data_dir("astrbot_plugin_cngal_interaction") / "assets.json")
        self.policy = NativePolicy(context)
        EarlyQQPolicy.policy = self.policy
        self.client = BindingClient(config["api_base_url"])

    def ai_trigger(self, event):
        return event.is_at_or_wake_command and explicit_ai_trigger(event)

    async def ai_allowed(self, event):
        """在 AI 处理的各阶段重新检查原生权限，不缓存检查结果。"""
        return (await self.policy.allowed(event)
                and await ai_enabled(self.context, event.unified_msg_origin))

    @filter.custom_filter(EarlyQQPolicy, priority=sys.maxsize + 1)
    async def gate(self, event: AstrMessageEvent):
        if event.get_platform_name() != "aiocqhttp":
            return
        binding = event.get_extra("cngal_binding")
        if binding is not None:
            await self.execute_binding(event, binding)
            return
        if not await self.policy.allowed(event):
            event.stop_event()
            return
        if not self.ai_trigger(event) or not await ai_enabled(self.context, event.unified_msg_origin):
            return
        await self.admit(event)

    async def admit(self, event):
        if event.get_extra("cngal_admitted"):
            return True
        allowed, warn = await self.limiter.admit(str(event.get_sender_id()), event.unified_msg_origin)
        if not allowed:
            if warn:
                await event.send(event.plain_result("核心温度过高，正在冷却......"))
            event.stop_event()
            return False
        event.set_extra("cngal_admitted", True)
        return True

    async def execute_binding(self, event, code):
        if event.get_platform_name() != "aiocqhttp" or event.is_stopped():
            return
        if not await self.policy.allowed(event, binding=True):
            event.stop_event()
            return
        if not await self.admit(event):
            return
        try:
            try:
                text = await self.client.bind(code, str(event.get_sender_id()))
            except BindingError as exc:
                text = str(exc)
            # 绑定不依赖 AI。HTTP 请求结束后重新检查绑定权限，再决定是否发送回执。
            if event.is_stopped() or not await self.policy.allowed(event, binding=True):
                return
            await event.send(event.plain_result(text))
        finally:
            # 在原生群聊历史、上下文处理和模型请求之前结束本事件。
            event.set_extra("cngal_binding", None)
            event.set_extra("cngal_binding_handled", True)
            event.stop_event()

    @filter.on_llm_request(priority=sys.maxsize + 1)
    async def before_ai(self, event: AstrMessageEvent, req: ProviderRequest):
        if event.get_platform_name() != "aiocqhttp":
            return
        event.set_extra("cngal_asset_agent_done", None)
        if event.get_extra("cngal_binding") is not None or event.get_extra("cngal_binding_handled") or not self.ai_trigger(event) or not await self.ai_allowed(event):
            event.stop_event()
            return
        if not await self.admit(event):
            return
        if req.conversation is None:
            event.stop_event()
            return
        event.set_extra("cngal_reply_event", event)
        try:
            _, persona, _, _ = await self.context.persona_manager.resolve_selected_persona(
                umo=event.unified_msg_origin,
                conversation_persona_id=req.conversation.persona_id,
                platform_name=event.get_platform_name(),
                provider_settings=self.context.get_config(event.unified_msg_origin).get("provider_settings", {}))
            if persona and persona.get("tools") is not None and req.func_tool is not None:
                # 原生流程会在人设注入后追加平台工具；最终请求仍使用同一份人设工具选择。
                for tool in list(req.func_tool.tools):
                    if tool.name not in persona["tools"]:
                        req.func_tool.remove_tool(tool.name)
        except BaseException as exc:
            # 原生回调调度器记录异常后可能继续执行，取消异常也一样。
            # 先停止事件，避免人设解析失败后仍把未筛选的工具交给模型。
            event.stop_event()
            if isinstance(exc, Exception):
                raise RuntimeError("无法读取 CnGal 人设，已停止 AI 请求。") from None
            raise
        today = datetime.now(ZoneInfo("Asia/Shanghai")).date()
        birthday = date(2015, 6, 30)
        age = today.year - birthday.year - ((today.month, today.day) < (6, 30))
        req.system_prompt += f"\n看板娘当前年龄：{age}岁（按上海日期计算）。"

    @filter.llm_tool(name="cngal_asset")
    async def asset(self, event: AstrMessageEvent, asset_id: str = "") -> str:
        """为当前聊天回复选择一个已登记素材，不跨会话发送。

        当前上下文已确认素材名称时直接选择该名称，不重复查询目录；名称或用途不确定时，
        以空 asset_id 获取已配置素材的名称、类型、用途，再选择返回的名称。
        需要实质资料、解释或链接时用纯文字，不选择表情。只需表情回应时选择
        usage=expression，最终只发图片。仅在用户明确要求语音或插图时选择
        usage=voice 或 illustration。固定语音不是TTS，不声称能朗读任意文本。
        选中任何素材后以非空文字“完成”结束本轮，不再调用工具，不输出过程文字。
        这句内部确认由插件替换成所选素材，用户只收到一张图片或一条语音，不附带文字。
        每个用户事件最多选择一个素材。不再调用发送工具重复投递，不生成URL或使用列表外文件。
        没有合适素材时仅回复文字，不假称已送达；原生定时任务不得使用此聊天工具。

        Args:
            asset_id(string): 固定素材名，留空查询名称、类型、用途。
        """
        if event.get_platform_name() != "aiocqhttp" or event.is_stopped() or not event.get_extra("cngal_admitted") or not await self.ai_allowed(event):
            return "当前会话不允许使用此工具。"
        assets = self.assets
        if not asset_id:
            return json.dumps([{"asset_id": name, "kind": value["kind"], "usage": value["usage"]}
                               for name, value in assets.items()], ensure_ascii=False)
        if not assets:
            return "当前未配置可用素材，请使用文字回复。"
        if asset_id not in assets:
            return "未知素材，请从素材列表选择。"
        if event.get_extra("cngal_asset_selected"):
            return "本次回复已经选择过素材，不再重复选择。"
        event.set_extra("cngal_asset_selected", True)
        event.set_extra("cngal_asset_pending", (event, asset_id))
        label = {"expression": "表情", "voice": "语音", "illustration": "插图"}[assets[asset_id]["usage"]]
        return f"已选择{label}。请以非空文字“完成”结束本轮，不再调用工具；这句内部确认会被替换成所选素材，用户只收到{label}。"

    async def guard_reply(self, event):
        """发送本插件处理的 AI 回复前重查权限；检查异常时也停止回复。"""
        allowed = False
        try:
            allowed = (event.get_extra("cngal_reply_event") is event
                       and event.get_platform_name() == "aiocqhttp" and not event.is_stopped()
                       and not event.get_extra("agent_stop_requested")
                       and not event.get_extra("agent_user_aborted")
                       and bool(event.get_extra("cngal_admitted"))
                       and await self.ai_allowed(event))
            return allowed
        finally:
            if not allowed:
                if (result := event.get_result()) is not None:
                    result.chain = []
                event.set_extra("cngal_asset_pending", None)
                event.set_extra("cngal_asset_seed", None)
                event.set_extra("cngal_asset_agent_done", None)
                event.is_at_or_wake_command = False
                event.stop_event()

    @filter.on_agent_begin()
    async def agent_begin(self, event: AstrMessageEvent, run_context):
        event.set_extra("cngal_asset_agent_done", None)
        event.set_extra("cngal_asset_pending", None)
        # 保留旧临时组件的引用，交给结果装饰阶段移除，避免媒体残留在缓冲结果中。

    @filter.on_agent_done(priority=-sys.maxsize - 1)
    async def agent_done(self, event: AstrMessageEvent, run_context, response):
        # 工具调用前的过程文字也标记为 LLM_RESULT。
        # 只有已完成且不含工具调用的助手回复，才能进入最终素材替换。
        event.set_extra("cngal_asset_agent_done", None)
        if event.get_extra("cngal_reply_event") is None:
            return
        if not await self.guard_reply(event):
            if response is not None:
                response.completion_text = ""
                response.result_chain = None
            return
        if (event.get_platform_name() != "aiocqhttp" or event.is_stopped()
                or event.get_extra("agent_stop_requested") or event.get_extra("agent_user_aborted")
                or getattr(getattr(run_context, "context", None), "event", None) is not event
                or response is None or response.role != "assistant"
                or response.tools_call_name or response.tools_call_args or response.tools_call_ids):
            event.set_extra("cngal_asset_pending", None)
            return
        event.set_extra("cngal_asset_agent_done", event)
        pending = event.get_extra("cngal_asset_pending")
        if (isinstance(pending, tuple) and len(pending) == 2 and pending[0] is event
                and pending[1] in self.assets
                and not response.completion_text
                and (response.result_chain is None or not response.result_chain.chain)):
            # 空回复需要临时消息链才能进入原生结果流程。
            # 非空文字保留到原生回复内容检查完成后再替换。
            response.result_chain = asset_chain(pending[1], self.assets)
            event.set_extra("cngal_asset_seed", (event, response.result_chain.chain[0]))

    @filter.on_decorating_result()
    async def decorate(self, event: AstrMessageEvent):
        pending = event.get_extra("cngal_asset_pending")
        result = event.get_result()
        seed = event.get_extra("cngal_asset_seed")
        if isinstance(seed, tuple) and len(seed) == 2:
            # 先移除临时组件，再处理权限或异常分支。
            # 原生 run_agent 合并消息链时复用组件对象，可按对象引用移除。
            if result:
                result.chain = [part for part in result.chain if part is not seed[1]]
            event.set_extra("cngal_asset_seed", None)
        # 检查本插件处理的文字、媒体和原生错误回复；独立定时任务及绑定回执保持原流程。
        # 待发素材必须属于当前事件，复制的状态不能获得发送权限。
        if event.get_extra("cngal_reply_event") is None and not pending and not seed:
            return
        if not await self.guard_reply(event):
            return
        if (not pending or not result or not result.is_llm_result()
                or event.get_extra("cngal_asset_agent_done") is not event):
            return
        event.set_extra("cngal_asset_pending", None)
        if not isinstance(pending, tuple) or len(pending) != 2 or pending[0] is not event:
            result.chain = []
            event.stop_event()
            return
        asset = self.assets.get(pending[1])
        if asset:
            # 固定素材替换全部缓冲过程文字和最终正文。
            result.chain = asset_chain(pending[1], self.assets).chain

    async def terminate(self):
        if EarlyQQPolicy.policy is self.policy:
            EarlyQQPolicy.policy = None
        await self.client.close()
