# CnGal 交互插件

为 QQ 群聊与私聊提供 CnGal AI 查询、固定图片或语音回复，以及独立的账号绑定流程。查询通过 CnGal MCP 提供，绑定直接调用主站接口。

首次公开版本为 **v1.0.0**，兼容范围为 **AstrBot >=4.28.2,<5**，平台为 **QQ／OneBot v11（aiocqhttp）**。AI 聊天需要已配置的模型和 CnGal MCP；插件使用 AstrBot 自带的 aiohttp，无须单独安装依赖。

## 安装与更新

在 AstrBot 原生插件管理中选择通过仓库链接安装，填写：

```text
https://github.com/CnGal/CnGalWebSite/tree/astrbot-plugin
```

该地址在维护者首次成功运行发布工作流后可用。后续在插件管理中点击更新，即从同一发布分支获取版本；发布分支仅在维护者发布时更新。不要填写主仓库根地址或主分支内插件子目录的网页地址。

离线安装时，可按[部署说明](https://github.com/CnGal/CnGalWebSite/blob/master/integrations/astrbot/deployment/README.md)构建 ZIP，再通过原生插件管理上传安装或更新。更新前备份 AstrBot 运行数据，保留原生插件数据目录和人设。

## 配置与使用

按[完整部署步骤](https://github.com/CnGal/CnGalWebSite/blob/master/integrations/astrbot/deployment/README.md)连接 MCP、配置模型、合并基础与聊天配置补丁，并将获准群聊或私聊精确绑定到非默认配置档。默认配置关闭 AI，获准配置显式选择本插件并开启 AI。

- 群聊通过“看板娘”或 @机器人唤醒；获准私聊可直接交流。
- 发送“看板娘绑定 绑定码”绑定账号。绑定需要管理员单独配置主站凭据，不调用 AI；只发送“绑定”等缺参咨询时按 AI 聊天权限处理。
- 人设正文由管理员自行编写并通过 AstrBot 原生人设管理导入，仓库不提供人设正文。
- 正式素材放在 `data/plugin_data/astrbot_plugin_cngal_interaction/assets.json`，格式与实际路径说明见部署文档。公开素材清单为空；没有素材时仍可文字聊天和绑定。每次素材回复只发送一张图片或一条语音。
- 在插件设置中调整 `rate_limit`，保存并重载后生效。

## 源码与许可证

源码在 [CnGalWebSite 主分支](https://github.com/CnGal/CnGalWebSite/tree/master/integrations/astrbot/plugins/astrbot_plugin_cngal_interaction)维护；本分支由发布工作流生成。代码采用 [MIT License](LICENSE)，正式人设和素材由部署方另行提供。
