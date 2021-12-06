<div style="display: flex;justify-content: center;align-items: center;">
  <!-- 在此处设置您的项目图标 -->
  <img src="//app.cngal.org/_content/NewCngalWebSite.Shared/images/logo.png" height="50" style="margin-right: 20px;"/>
  <span style="font-size: 35px;">CnGal资料站</span>
</div>
<p align="center"  style="margin-top: 10px;">
<!-- 在这里填写您的项目口标语，最好是一个简短的句子。 -->
    愿每一个CnGal创作者的作品都能不被忘记
</p>
<span id="nav-1"></span>
<p align="center">
  <a href="https://github.com/misitebao/standard-repository/blob/main/LICENSE">
    <img alt="GitHub" src="https://img.shields.io/github/license/misitebao/standard-repository?style=flat-square"/>
  </a>
  <a href="https://github.com/misitebao/standard-repository">
    <img alt="GitHub" src="https://cdn.jsdelivr.net/gh/misitebao/standard-repository@main/assets/badge_flat-square.svg"/>
  </a>
  <a href="https://github.com/misitebao/standard-repository">
    <img alt="GitHub Repo stars" src="https://img.shields.io/github/stars/misitebao/standard-repository?style=flat-square"/>
  </a>
</p>

## Internationalization

<!-- 这是多语言列表 -->

[English](README.en-US.md) | [简体中文](README.md)

<span id="nav-2"></span>

## Table of Contents

<details open="open">
  <summary>Click me to Open/Close the directory listing</summary>

- [Internationalization](#internationalization)
- [Table of Contents](#table-of-contents)
- [Introductions](#introductions)
  - [Official Website](#official-website)
  - [Background](#background)
- [Graphic Demo](#graphic-demo)
- [Features](#features)
- [Architecture](#architecture)
- [Getting Started](#getting-started)
- [Maintainer](#maintainer)
- [Contributors](#contributors)
- [Community Exchange](#community-exchange)
- [Part Of Users](#part-of-users)
- [Donators](#donators)
- [Sponsors](#sponsors)
- [Special Thanks](#special-thanks)
- [License](#license)

</details>

<span id="nav-3"></span>

## Introductions

<!-- 在这里填写关于您的项目的详细介绍 -->
这里是CnGal资料站第三次改版后的完整网站项目

CnGal是一个非营利性的，立志于收集整理国内制作组创作的中文Galgame/AVG的介绍、攻略、评测、感想等内容的资料性质的网站。 此外，CnGal官方还会与圈内中文AVG制作组进行友好合作，如免费提供Banner广告位，网站服务器资源等。

<span id="nav-3-1"></span>

### Official Website

主站：https://www.cngal.org/

PWA应用：https://app.cngal.org/
<!-- 在此填写您项目的官网地址，包括主页、文档等。 -->

<span id="nav-3-2"></span>

### Background

**Time Line**

2020年6月6日 - [《CnGal制作组大全V0.1》](https://docs.qq.com/sheet/DRFBsb1pEbUVpUXlj)发布，收录了我们在微博上关注的制作组

2020年11月13日 - 开启资料站补全计划，针对角色、游戏周边、相关文章、STAFF情报制作专门表格，为主站改版做准备

2020年12月23日 - 已对130+款作品情报进行重新整理，占已知CnGal游戏总数过1/3

2021年1月29日 - 公开[《CnGal资料表》](https://docs.qq.com/sheet/DREhYYVdGVmhCa1V2)原始表格，收录210+款作品

2021年8月19日 - CnGal资料站 v3.0 开始内测

2021年9月20日 - 公测开始

**初心**

『为了让每一个创作者的作品都能不被忘记』

我们开始了为期一年的资料补全<br>
升级站点，让游戏更便于录入<br>
与互联网档案馆对接，让数据永存<br>

『为了让每一个普通的玩家都能参与』

我们采用了类Wiki的形式，让每一个玩家都参与编辑<br>
对外开放API，采用知识共享署名，让资料本身不成为专属某一方的垄断<br>

<!-- 这里填写项目创作背景 -->

<span id="nav-4"></span>

## Graphic Demo

![主页](https://pic.cngal.top/images/2021/12/06/QQ20211206195920.png)
<!-- 把你项目的demo放在这里，可以是具体的访问地址、图片截图、Gif或者视频等。 -->

<span id="nav-5"></span>

## Features

- 词条
  - 包括二级分类：游戏、制作组、角色、STAFF
  - 拥有主页，相册，标签等模块
  - 游戏包括STAFF、制作组、发行商
  - 游戏会展示Steam贩售价格
  - 角色可以录入身高，性格，瞳色等详细数据
  - 以卡片形式展示关联信息
  - 任何人都可编辑，查看历史编辑数据
  - 允许用户留言
- 文章
  - 包括二级分类：攻略、 访谈、感想......
  - 允许用户点赞、评论
  - 任何人都能发布文章，但只有自己能够编辑
- 周边
  - 包括详尽的信息字段：价格，分类，尺寸......
  - 允许关联其他周边，以套装形式展示
  - 会以合集方式展示在相关词条下方
  - 可以记录用户的收集进度
  - 允许用户评论
- 标签
  - 拥有层级关系
- 消歧义页
  - 可以对相似文章、词条消歧义
- 用户
  - 完整的账户管理
  - 允许创建收藏夹，并收藏词条、文章、周边
  - 查看编辑历史
  - 记录积分和贡献值
  - 接收系统消息
  - 允许其他用户在空间留言
  - 绑定第三方账户
- 后台
  - 完善的各模块数据管理页面
  - 批量导入数据
  - 拥有临时脚本执行入口
  - 允许执行定时任务
  - 自动备份页面到互联网档案馆
  - 用户权限管理
- 其他
  - 数据汇总页面
  - 编辑指引与词条完善度检查
  - 动态汇总页面
<!-- 在此处填写您的项目的功能，通常是一个列表。 -->

<span id="nav-6"></span>

## Architecture

后端：ASP .Net Core Web API

前端：ASP .Net Core Blazor

UI库：BlazorBootstrap

数据库：Mysql 8.0

ORM：Entity Framework Core 6.0

SDK：.Net 6.0

<!-- 在这里填写你的项目架构图或描述，你可以放置项目目录描述 -->

```
|—— .git                                         Git 配置文件
|—— CnGalInformationWebsite                      项目代码
| |—— CnGalInformationWebsite.APIServer          API项目
| | |—— Application                                公共方法
| | |—— Controllers                                控制器
| | |—— CustomMiddlewares                          中间件
| | |—— DataReositories                            数据库基础设施
| | |—— Infrastructure                             接口
| | |—— Migrations                                 数据库迁移文件
| |—— CnGalInformationWebsite.Server             服务端渲染项目
| |—— CnGalInformationWebsite.WebAssembly        客户端渲染项目
| |—— CnGalInformationWebsite.DataModel          数据模型类库
| |—— CnGalInformationWebsite.Shared             Blazor页面组件类库
|—— CHANGELOG.md                    发布日志
|—— LICENSE                         许可证
|—— README.md                       中文 README
|—— README.en-US.md                 英语 README

```
<span id="nav-7"></span>

## Getting Started

<!-- 在这里写下项目的详细说明，告诉用户如何使用你的项目。 -->
如果你想要开发第三方客户端，请参阅 [API使用流程](/Docs/AboutCode/APIInstructions/Summary.md)

[点我](/Docs/AboutCode/BasicTutorial/HowToRun.md) 查看如何搭建运行环境并运行项目

在参与项目之前，可以查看 [代码文档](/Docs/AboutCode/Summary.md) 了解代码结构，并阅读我们的 [代码规范]()

<span id="nav-8"></span>

## Maintainer
Thanks to the maintainers of these projects:
<!-- 这里填写项目作者的相关信息 -->
<a href="https://github.com/LittleFish-233"><img src="https://pic.cngal.top/images/2021/11/05/35e3e59e0876.jpg" width="40" height="40" alt="沙雕の方块" title="沙雕の方块"/></a>

<details>
  <summary>Click me to Open/Close the maintainer listing</summary>

- [沙雕の方块](https://github.com/LittleFish-233) - 项目开发者，大二，努力成为全栈工程师中

</details>

<span id="nav-9"></span>

## Contributors
感谢所有参与 CnGal资料站 开发的贡献者。[贡献者列表](https://github.com/LittleFish-233/standard-repository/graphs/contributors)

<!-- 这里填写项目贡献者列表，通常是列表，当然也可以用图片代替。 -->

<span id="nav-10"></span>

## Community Exchange

CnGal玩家交流群：[128446539](https://jq.qq.com/?_wv=1027&k=mG6qNvyg)

CnGal资料站编辑者交流&Bug反馈群：[761794704](https://jq.qq.com/?_wv=1027&k=JzuI1IkF)

新浪微博：[CnGal](https://weibo.com/cngalorg)
<!-- 此处填写项目的线上线下交流地址，可以是即时通讯群、社区、讨论群等。 -->

<span id="nav-11"></span>

## Part Of Users

CnGal资料站：https://www.cngal.org/
<!-- 在此处填写项目的用户列表，并告诉访问者哪些用户正在使用您的项目。 -->

<span id="nav-12"></span>

## Donators

<!-- 在这里填写捐赠者名单 -->

<span id="nav-13"></span>

## Sponsors

<!-- 在这里填写赞助商名单 -->

<span id="nav-14"></span>

## Special Thanks

<!-- 在这里填写特别感谢名单，可以是任何人或事物。 -->

<span id="nav-15"></span>

## License

[License MIT](LICENSE)
