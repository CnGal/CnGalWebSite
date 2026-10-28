"""读取 AstrBot 原生路由和权限，不维护第二份群配置。

只接受精确会话路由，不使用通配路由或默认配置兜底；本模块只读原生配置。
"""
from astrbot.core.star.session_llm_manager import SessionServiceManager
from astrbot.core.star.session_plugin_manager import SessionPluginManager

INTERACTION = "astrbot_plugin_cngal_interaction"


def selected(config, plugin):
    plugins = config.get("plugin_set", [])
    return isinstance(plugins, list) and plugin in plugins


class NativePolicy:
    def __init__(self, context):
        self.context = context

    def chat_profile(self, umo):
        """返回当前明确选中的配置档；无法确认时拒绝处理。"""
        parts = umo.split(":")
        if (len(parts) != 3 or parts[1] not in ("GroupMessage", "FriendMessage") or not parts[0]
                or any(c in parts[0] for c in "*?[]")
                or not parts[2].isascii() or not parts[2].isdecimal() or int(parts[2]) <= 0):
            return None
        platform = next((p for p in self.context.platform_manager.get_insts()
                         if p.meta().id == parts[0]), None)
        if platform is None or platform.meta().name != "aiocqhttp":
            return None
        manager = self.context.astrbot_config_mgr
        router = manager.ucr
        # 原生路由采用首个匹配项，并非优先采用最精确的路由。
        match = next(((pattern, conf_id) for pattern, conf_id in
                      tuple(router.umop_to_conf_id.items()) if router._is_umo_match(pattern, umo)), None)
        if match is None or match[0] != umo or match[1] == "default":
            return None
        profile = manager.confs.get(match[1])
        if profile is None or manager.get_conf_info(umo)["id"] != match[1]:
            return None
        if self.context.get_config(umo) is not profile or not selected(profile, INTERACTION):
            return None
        return profile

    def interaction_scope(self, event):
        if event.get_platform_name() != "aiocqhttp":
            return False
        return self.chat_profile(event.unified_msg_origin) is not None

    def binding_scope(self, event):
        if event.get_platform_name() != "aiocqhttp":
            return False
        if event.is_private_chat():
            return selected(self.context.get_config(event.unified_msg_origin), INTERACTION)
        return self.interaction_scope(event)

    async def binding_allowed(self, event):
        umo = event.unified_msg_origin
        return (self.binding_scope(event)
                and await SessionServiceManager.is_session_enabled(umo)
                and await SessionPluginManager.is_plugin_enabled_for_session(umo, INTERACTION)
                and self.binding_scope(event))

    async def interaction_allowed(self, event):
        umo = event.unified_msg_origin
        return (self.interaction_scope(event)
                and await SessionServiceManager.is_session_enabled(umo)
                and await SessionPluginManager.is_plugin_enabled_for_session(umo, INTERACTION)
                and self.interaction_scope(event))


async def ai_enabled(context, umo):
    return (context.get_config(umo).get("provider_settings", {}).get("enable", True)
            and await SessionServiceManager.is_session_enabled(umo)
            and await SessionServiceManager.is_llm_enabled_for_session(umo))
