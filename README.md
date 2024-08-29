<div align="center">
  <!-- 在此处设置您的项目图标 -->
  <img src="Docs/Images/logo.png" height="70"/>

# CnGal 资料站

</div>

<span id="nav-1"></span>

<p align="center"  style="margin-top: 10px;">
<!-- 在这里填写您的项目口标语，最好是一个简短的句子。 -->
    愿每一个CnGal创作者的作品都能不被忘记
</p>
<span id="nav-1"></span>
<div align="center">
 
[![LICENSE](https://img.shields.io/github/license/CnGal/CnGalWebSite)](https://github.com/CnGal/CnGalWebSite/LICENSE)
[![standard-repository](https://cdn.jsdelivr.net/gh/misitebao/standard-repository@main/assets/badge_flat.svg)](https://github.com/misitebao/standard-repository)
[![stars](https://img.shields.io/github/stars/CnGal/CnGalWebSite)](https://github.com/CnGal/CnGalWebSite)
[![爱发电](https://afdian.moeci.com/3/badge.svg)](https://afdian.com/@cngal)

</div>

## 国际化

<!-- 这是多语言列表 -->

[简体中文](README.md) | [English](README.en-US.md)

<span id="nav-2"></span>

## 内容目录

<details open="open">
  <summary>点我 打开/关闭 目录列表</summary>

- [CnGal 资料站](#cngal资料站)
  - [国际化](#国际化)
  - [内容目录](#内容目录)
  - [项目介绍](#项目介绍)
    - [官方网站](#官方网站)
    - [背景](#背景)
  - [图形演示](#图形演示)
  - [功能特色](#功能特色)
  - [架构](#架构)
  - [新手入门](#新手入门)
  - [维护者](#维护者)
  - [贡献者](#贡献者)
  - [社区交流](#社区交流)
  - [部分用户](#部分用户)
  - [捐赠者](#捐赠者)
  - [特别感谢](#特别感谢)
  - [版权许可](#版权许可)

</details>

<span id="nav-3"></span>

## 项目介绍

<!-- 在这里填写关于您的项目的详细介绍 -->

这里是 CnGal 资料站第二次改版后的完整网站项目

CnGal 是一个非营利性的，立志于收集整理国内制作组创作及中文化的中文 Galgame/AVG 的介绍、攻略、评测、感想等内容的资料性质的网站。 此外，CnGal 官方还会与圈内中文 AVG 制作组进行友好合作，如免费提供 Banner 广告位，网站服务器资源等。

<span id="nav-3-1"></span>

### 官方网站

主站：https://www.cngal.org/

PWA 应用：https://app.cngal.org/

<!-- 在此填写您项目的官网地址，包括主页、文档等。 -->

<span id="nav-3-2"></span>

### 背景

**时间轴**

2020 年 6 月 6 日 - [《CnGal 制作组大全 V0.1》](https://docs.qq.com/sheet/DRFBsb1pEbUVpUXlj)发布，收录了我们在微博上关注的制作组

2020 年 11 月 13 日 - 开启资料站补全计划，针对角色、游戏周边、相关文章、STAFF 情报制作专门表格，为主站改版做准备

2020 年 12 月 23 日 - 已对 130+款作品情报进行重新整理，占已知 CnGal 游戏总数过 1/3

2021 年 1 月 29 日 - 公开[《CnGal 资料表》](https://docs.qq.com/sheet/DREhYYVdGVmhCa1V2)原始表格，收录 210+款作品

2021 年 8 月 19 日 - CnGal 资料站 v3.0 开始内测

2021 年 9 月 21 日 - 公测开始

2021 年 12 月 16 日 - 正式上线，前端后端所有代码以MIT协议开源

**初心**

『为了让每一个创作者的作品都能不被忘记』

我们开始了为期一年的资料补全<br>
升级站点，让游戏更便于录入<br>
与互联网档案馆对接，让数据永存<br>

『为了让每一个普通的玩家都能参与』

我们采用了类 Wiki 的形式，让每一个玩家都参与编辑<br>
对外开放 API，采用知识共享署名<br>

<!-- 这里填写项目创作背景 -->

<span id="nav-4"></span>

## 图形演示

![主页](Docs/Images/主页v3.3.png)

![开放平台](Docs/Images/开放平台.png)

![鉴权中心](Docs/Images/鉴权中心.png)

<!-- 把你项目的demo放在这里，可以是具体的访问地址、图片截图、Gif或者视频等。 -->

<span id="nav-5"></span>

## 功能特色

- 词条
  - 包括二级分类：游戏、制作组、角色、STAFF
  - 拥有主页，相册，标签等模块
  - 游戏包括 STAFF、制作组、发行商
  - 游戏会展示 Steam 贩售价格
  - 角色可以录入身高，性格，瞳色等详细数据
  - 以卡片形式展示关联信息
  - 任何人都可编辑，查看历史编辑数据
  - 允许用户留言
  - 音频预览
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
- 视频
  - 收录简介、预览图、链接
  - 允许关联其他词条、文章、视频
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
- 看板娘
  - 问答
  - 定时任务、随机任务
  - 查询数据
- 投稿工具
  - 合并词条
  - 转载文章
  - 转载视频
- 开放平台
  - 注册应用
  - 提供聚合登入
- 鉴权中心
  - 登入授权
  - 修改账号资料
  - 绑定第三方账号
  - 身份验证
- 其他
  - 数据汇总页面
  - 编辑指引与词条完善度检查
  - 动态汇总页面
  - CV专题页
  <!-- 在此处填写您的项目的功能，通常是一个列表。 -->

<span id="nav-6"></span>

## 架构

### 网站

后端：ASP .Net Core Web API

前端：ASP .Net Core Blazor

UI 库：Masa Blazor, BlazorBootstrap

数据库：Mysql 8.0

ORM：Entity Framework Core 7.0

SDK：.Net 7.0

### 鉴权中心

架构：ASP.NET Core MVC

基于 IdentityServer4 二次开发

### 看板娘

QQ机器人框架使用 [Mirai](https://github.com/mamoe/mirai) 和 [Mirai-API-HTTP](https://github.com/project-mirai/mirai-api-http) 插件
并在 [.Net](https://github.com/microsoft/dotnet) 平台上通过 [MeowMiraiLib](https://github.com/DavidSciMeow/MeowMiraiLib) 第三方库开发

QQ频道使用官方API，在 [.Net](https://github.com/microsoft/dotnet) 平台上通过 [Masuda.Net](https://github.com/ssccinng/Masuda.Net) 第三方库开发

### 文件结构

<!-- 在这里填写你的项目架构图或描述，你可以放置项目目录描述 -->

```
|—— .git                                       Git 配置文件
|—— CnGalWebSite                               项目代码
| |—— CnGalWebSite.APIServer                       主站 - API
| |—— CnGalWebSite.DataModel                       主站 - 数据模型
| |—— CnGalWebSite.DrawingBed                      图床 - API
| |—— CnGalWebSite.Extensions                      公共 - 扩展类库
| |—— CnGalWebSite.HealthCheck                     公共 - 健康检查
| |—— CnGalWebSite.Helper                          公共 - 工具类库
| |—— CnGalWebSite.HistoryData                     资料表处理脚本（已弃用）
| |—— CnGalWebSite.IdentityServer                  鉴权中心
| |—— CnGalWebSite.IdentityServer.Admin.Shared     开放平台 - 共享组件
| |—— CnGalWebSite.IdentityServer.Admin.SSR        开放平台 - SSR
| |—— CnGalWebSite.IdentityServer.Admin.WASM       开放平台 - WASM
| |—— CnGalWebSite.IdentityServer.Models           鉴权中心 - 数据模型
| |—— CnGalWebSite.Maui                            MAUI
| |—— CnGalWebSite.PostTools                       投稿工具（已弃用）
| |—— CnGalWebSite.PublicToolbox                   投稿工具
| |—— CnGalWebSite.RobotClient                     看板娘
| |—— CnGalWebSite.Server                          主站 - SSR
| |—— CnGalWebSite.Shared                          主站 - 共享组件
| |—— CnGalWebSite.WebAssembly                     主站 - WASM
|—— CHANGELOG.md                              发布日志
|—— LICENSE                                   许可证
|—— README.md                                 中文 README
|—— README.en-US.md                           英语 README

```

<span id="nav-7"></span>

## 新手入门

<!-- 在这里写下项目的详细说明，告诉用户如何使用你的项目。 -->

如果你想要开发第三方客户端，请参阅 [API 使用流程](/Docs/AboutCode/APIInstructions/Summary.md)

[点我](/Docs/AboutCode/BasicTutorial/HowToRun.md) 查看如何搭建运行环境并运行项目

或者查看 [看板娘的自我介绍](/Docs/Kanban/Summary.md)

在参与项目之前，可以查看 [代码文档](/Docs/AboutCode/Summary.md) 了解代码结构，并阅读我们的 [代码规范]()

<span id="nav-8"></span>

## 维护者

感谢这些项目的维护者：

<!-- 这里填写项目作者的相关信息 -->

<a href="https://github.com/LittleFish-233"><img src="Docs/Images/沙雕方块.jpg" width="40" height="40" alt="沙雕の方块" title="沙雕の方块"/></a>

<details>
  <summary>点我 打开/关闭 维护者列表</summary>

- [沙雕の方块](https://github.com/LittleFish-233) - 项目开发者，大三，努力成为全栈工程师中

</details>

<span id="nav-9"></span>

## 贡献者

感谢所有参与 CnGal 资料站 开发的贡献者。[贡献者列表](https://github.com/CnGal/CnGalWebSite/graphs/contributors)

<!-- 这里填写项目贡献者列表，通常是列表，当然也可以用图片代替。 -->

<span id="nav-10"></span>

## 社区交流

CnGal 玩家交流群：[128446539](https://jq.qq.com/?_wv=1027&k=mG6qNvyg)

CnGal 资料站编辑者交流&Bug 反馈群：[761794704](https://jq.qq.com/?_wv=1027&k=JzuI1IkF)

新浪微博：[CnGal](https://weibo.com/cngalorg)

<!-- 此处填写项目的线上线下交流地址，可以是即时通讯群、社区、讨论群等。 -->

<span id="nav-11"></span>

## 部分用户

CnGal 资料站：https://www.cngal.org/

<!-- 在此处填写项目的用户列表，并告诉访问者哪些用户正在使用您的项目。 -->

<span id="nav-12"></span>

## 捐赠者

感谢这些项目的捐赠者：

<a href="https://afdian.com/u/613de57082bb11e982ed52540025c377"><img src="https://pic1.afdiancdn.com/user/613de57082bb11e982ed52540025c377/avatar/2307c01d2a74aaf44583e5ebfe745c14_w2560_h1440_s1797.jpg?imageView2/1/w/120/h/120" width="40" height="40" alt="NTR天下第一" title="NTR天下第一"/></a>
<a href="https://afdian.com/u/ad2d3d66a3f011eba1e052540025c377"><img src="https://pic1.afdiancdn.com/user/ad2d3d66a3f011eba1e052540025c377/avatar/b55149a56f8a80d4ed510f8efd1ea915_w562_h562_s50.jpg?imageView2/1/w/120/h/120" width="40" height="40" alt="雷之" title="雷之"/></a>
<a href="https://afdian.com/u/2c1c2e3eb4f011ea8d4452540025c377"><img src="https://pic1.afdiancdn.com/user/2c1c2e3eb4f011ea8d4452540025c377/avatar/f3ae53ff06d599d4215ddcb677a7bd40_w958_h958_s71.jpg?imageView2/1/w/120/h/120" width="40" height="40" alt="Zero就是零啊" title="Zero就是零啊"/></a>
<a href="https://afdian.com/u/f3d043f4831311e8b53152540025c377"><img src="https://pic1.afdiancdn.com/user/f3d043f4831311e8b53152540025c377/avatar/a2853ceb0f33559a8fe9be8077357cbd_w640_h640_s63.jpeg?imageView2/1/w/120/h/120" width="40" height="40" alt="mzy069" title="mzy069"/></a>
<a href="https://afdian.com/u/c8299728395d11e89de552540025c377"><img src="https://pic1.afdiancdn.com/user/c8299728395d11e89de552540025c377/avatar/0ad90bd1e5c7ed5728378404bd593132_w465_h553_s281.jpg?imageView2/1/w/120/h/120" width="40" height="40" alt="Emiya" title="Emiya"/></a>
<a href="https://afdian.com/u/b35076f03a0711e8b8f152540025c377"><img src="https://pic1.afdiancdn.com/user/b35076f03a0711e8b8f152540025c377/avatar/634450a83053184f4cfc6e7850e165e0_w791_h793_s310.jpg?imageView2/1/w/120/h/120" width="40" height="40" alt="被炒的炒饭" title="被炒的炒饭"/></a>
<a href="https://afdian.com/@wumingjianxia"><img src="https://pic1.afdiancdn.com/user/2309207c14fb11e9a0a552540025c377/avatar/2efcac67405bad1b031894c005159633_w204_h256_s11.jpg?imageView2/1/w/120/h/120" width="40" height="40" alt="无名剑侠" title="无名剑侠"/></a>
<a href="https://afdian.com/u/172f8dfc2a4011eb8f5a52540025c377"><img src="https://pic1.afdiancdn.com/user/172f8dfc2a4011eb8f5a52540025c377/avatar/8fe4335592e6e7c41c8ef9653d7f6436_w1024_h576_s751.jpg?imageView2/1/w/120/h/120" width="40" height="40" alt="mem.wey" title="mem.wey"/></a>
<a href="https://afdian.com/u/d666631cb54711ea9e5952540025c377"><img src="https://pic1.afdiancdn.com/user/d666631cb54711ea9e5952540025c377/avatar/01d1dfb98aa597b17820e53daac7a29c_w938_h938_s440.jpg?imageView2/1/w/120/h/120" width="40" height="40" alt="声控灯" title="声控灯"/></a>
<a href="https://afdian.com/u/793e510cfadd11e88b6c52540025c377"><img src="https://pic1.afdiancdn.com/user/793e510cfadd11e88b6c52540025c377/avatar/3dcb4490deb7cd8711eda7f3c1617120_w640_h637_s43.jpg?imageView2/1/w/120/h/120" width="40" height="40" alt="隐_hermity" title="隐_hermity"/></a>
<a href="https://afdian.com/u/94996d86268311eca29652540025c377"><img src="https://pic1.afdiancdn.com/user/94996d86268311eca29652540025c377/avatar/3b3cb21c816d7b0e39acfb3d5e6944e0_w1080_h1080_s89.jpg?imageView2/1/w/120/h/120" width="40" height="40" alt="陈炎西" title="陈炎西"/></a>
<a href="https://afdian.com/u/a424d630273611eab69c52540025c377"><img src="https://pic1.afdiancdn.com/user/a424d630273611eab69c52540025c377/avatar/dba861b5911b5a7c43d0309ae6d26287_w2292_h1667_s315.jpg?imageView2/1/w/120/h/120" width="40" height="40" alt="小恸恸" title="小恸恸"/></a>
<a href="https://afdian.com/@skt_studio"><img src="https://pic1.afdiancdn.com/user/0e803d96b55611eaa0d152540025c377/avatar/9d55484978aa2eff4bebe6c1c640570e_w587_h587_s127.jpg?imageView2/1/w/120/h/120" width="40" height="40" alt="毕业后咖啡时间 SKT STUDIO" title="毕业后咖啡时间 SKT STUDIO"/></a>
<a href="https://afdian.com/u/a0d140fe493e11e8906652540025c377"><img src="https://pic1.afdiancdn.com/user/a0d140fe493e11e8906652540025c377/avatar/88ffc6a9234986cc34ef55f4b64ad796_w551_h583_s93.jpg?imageView2/1/w/120/h/120" width="40" height="40" alt="巴格拉季昂亲王" title="巴格拉季昂亲王"/></a>
<a href="https://afdian.com/@mycngal"><img src="https://pic1.afdiancdn.com/user/175f8396b50011eab95a52540025c377/avatar/63a0e5f753914e4afbce3d8a99c9c64f_w1080_h1080_s197.jpg?imageView2/1/w/120/h/120" width="40" height="40" alt="莫言国G" title="莫言国G"/></a>
<a href="https://afdian.com/u/699e8f367b4d11eaa21d52540025c377"><img src="https://pic1.afdiancdn.com/user/699e8f367b4d11eaa21d52540025c377/avatar/be9628b3769d4acc0312fbae4adfafbe_w361_h392_s12.jpg?imageView2/1/w/120/h/120" width="40" height="40" alt="慕寒幻夜" title="慕寒幻夜"/></a>
<a href="https://afdian.com/@xiaopu2049"><img src="https://pic1.afdiancdn.com/user/1feecc3cbaf611eab2c652540025c377/avatar/ead4082f1cd23194249a76002f3b9b98_w798_h615_s752.jpg?imageView2/1/w/120/h/120" width="40" height="40" alt="SP-time制作组" title="SP-time制作组"/></a>
<a href="https://afdian.com/u/7604edbcbd2c11eab24f52540025c377"><img src="https://pic1.afdiancdn.com/user/7604edbcbd2c11eab24f52540025c377/avatar/7265e65e21f308ffea37938d2823916b_w1250_h1600_s1494.jpg?imageView2/1/w/120/h/120" width="40" height="40" alt="星辰" title="星辰"/></a>
<a href="https://afdian.com/u/4320fc62df0d11eab52d52540025c377"><img src="https://pic1.afdiancdn.com/user/4320fc62df0d11eab52d52540025c377/avatar/8bbdd37bb86edfa434cd16b7c4cf049b_w1920_h1200_s340.jpg?imageView2/1/w/120/h/120" width="40" height="40" alt="CnG天下第一" title="CnG天下第一"/></a>
<a href="https://afdian.com/u/8b3a03ccb51911ea82b652540025c377"><img src="https://pic1.afdiancdn.com/user/8b3a03ccb51911ea82b652540025c377/avatar/8ef64a1bf04d2d3150a574942b0cf08c_w617_h656_s618.jpg?imageView2/1/w/120/h/120" width="40" height="40" alt="墨小菊天下第一" title="墨小菊天下第一"/></a>
<a href="https://afdian.com/u/3d760d0cfd6211eaa74f52540025c377"><img src="https://pic1.afdiancdn.com/user/3d760d0cfd6211eaa74f52540025c377/avatar/a36ec7f7d47da20e2f08b6528406972b_w376_h377_s40.jpeg?imageView2/1/w/120/h/120" width="40" height="40" alt="十六夜" title="十六夜"/></a>

<details>
  <summary>点我 打开/关闭 捐赠者列表</summary>

- [NTR 天下第一](https://afdian.com/u/613de57082bb11e982ed52540025c377)
- [kyatana](https://afdian.com/u/e9d4ccf6b4b911ea907652540025c377)
- [雷之](https://afdian.com/u/ad2d3d66a3f011eba1e052540025c377)
- [Zero 就是零啊](https://afdian.com/u/2c1c2e3eb4f011ea8d4452540025c377)
- [mzy069](https://afdian.com/u/f3d043f4831311e8b53152540025c377)
- [Emiya](https://afdian.com/u/c8299728395d11e89de552540025c377)
- [被炒的炒饭](https://afdian.com/u/b35076f03a0711e8b8f152540025c377)
- [无名剑侠](https://afdian.com/@wumingjianxia)
- [mem.wey](https://afdian.com/u/172f8dfc2a4011eb8f5a52540025c377)
- [声控灯](https://afdian.com/u/d666631cb54711ea9e5952540025c377)
- [隐\_hermity](https://afdian.com/u/793e510cfadd11e88b6c52540025c377)
- [真恋寄语枫秋](https://afdian.com/u/660987cef20a11eb95c052540025c377)
- [陈炎西](https://afdian.com/u/94996d86268311eca29652540025c377)
- [小恸恸](https://afdian.com/u/a424d630273611eab69c52540025c377)
- [毕业后咖啡时间 SKT STUDIO](https://afdian.com/@skt_studio)
- [击落夫飞艇](https://afdian.com/u/f88ff312b69c11eab5bb52540025c377)
- [巴格拉季昂亲王](https://afdian.com/u/a0d140fe493e11e8906652540025c377)
- [莫言国 G](https://afdian.com/@mycngal)
- [慕寒幻夜](https://afdian.com/u/699e8f367b4d11eaa21d52540025c377)
- [SP-time 制作组](https://afdian.com/@xiaopu2049)
- [星辰](https://afdian.com/u/7604edbcbd2c11eab24f52540025c377)
- [音速灰行的起子](https://afdian.com/u/a355a938becf11eaa23652540025c377)
- [蹦蹦的笨](https://afdian.com/u/e273d0b0c44a11ea9df252540025c377)
- [CnG 天下第一](https://afdian.com/u/4320fc62df0d11eab52d52540025c377)
- [綦](https://afdian.com/u/e5e533aad92611eba30152540025c377)
- [墨小菊天下第一](https://afdian.com/u/8b3a03ccb51911ea82b652540025c377)
- [爱发电用户\_C8K6](https://afdian.com/u/ffd8a6d8b50611ea87fc52540025c377)
- [老张](https://afdian.com/u/d1d17100c6b511eaa13252540025c377)
- [十六夜](https://afdian.com/u/3d760d0cfd6211eaa74f52540025c377)
- [大 K](https://afdian.com/u/3c41a268c79a11ebaf1052540025c377)
</details>
<!-- 在这里填写捐赠者名单 -->

<!-- ## 赞助商

在这里填写赞助商名单

<span id="nav-14"></span> -->

## 特别感谢

感谢不愿透露姓名的热心人士提供词库

感谢 KSE Hiyo 为资料站制作宣传 PV

<img src="Docs/Images/KSEHiyo.jpg" width="40" height="40" alt="KSEHiyo" title="KSEHiyo"/>

感谢参与编辑[《CnGal 制作组大全 V0.1》](https://docs.qq.com/sheet/DRFBsb1pEbUVpUXlj)的胖胖、小恸恸、HAna

<img src="Docs/Images/胖胖.png" width="40" height="40" alt="胖胖" title="胖胖"/> <img src="Docs/Images/小恸恸.jpg" width="40" height="40" alt="小恸恸" title="小恸恸"/> <img src="Docs/Images/HAna.jpg" width="40" height="40" alt="HAna花火" title="HAna花火"/>

感谢半年以来参与资料站补全计划，一同编辑资料表和提供参考资料的小伙伴们：

<img src="Docs/Images/九州人士.jpg" width="40" height="40" alt="九州人士" title="九州人士"/> <img src="Docs/Images/Sliots.jpg" width="40" height="40" alt="Sliots" title="Sliots"/> <img src="Docs/Images/巴格拉季昂亲王.jpg" width="40" height="40" alt="亲王" title="亲王"/> <img src="Docs/Images/声控灯.jpg" width="40" height="40" alt="声控灯" title="声控灯"/> <img src="Docs/Images/隐(不看动漫的死宅).jpg" width="40" height="40" alt="隐" title="隐"/> <img src="Docs/Images/捡垃圾的垃圾佬.jpg" width="40" height="40" alt="捡垃圾的垃圾佬" title="捡垃圾的垃圾佬"/> <img src="Docs/Images/CriAngel.jpg" width="40" height="40" alt="CriAngel" title="CriAngel"/> <img src="Docs/Images/沙雕方块.jpg" width="40" height="40" alt="沙雕の方块" title="沙雕の方块"/> <img src="Docs/Images/昊晨.jpg" width="40" height="40" alt="昊晨" title="昊晨"/> <img src="Docs/Images/我不姓高abc.jpg" width="40" height="40" alt="我不姓高abc" title="我不姓高abc"/> <img src="Docs/Images/百变一点也不怪.jpg" width="40" height="40" alt="百变一点也不怪" title="百变一点也不怪"/> <img src="Docs/Images/柳知萧.jpg" width="40" height="40" alt="柳知萧" title="柳知萧"/> <img src="Docs/Images/磁爆步兵杨永信.jpg" width="40" height="40" alt="磁爆步兵杨永信" title="磁爆步兵杨永信"/> <img src="Docs/Images/月骨琉璃.jpg" width="40" height="40" alt="月骨琉璃" title="月骨琉璃"/> <img src="Docs/Images/zhl.jpg" width="40" height="40" alt="zhl" title="zhl"/> <img src="Docs/Images/原味葱油饼干.jpg" width="40" height="40" alt="原味葱油饼干" title="原味葱油饼干"/> <img src="Docs/Images/Chr_.png" width="40" height="40" alt="Chr_" title="Chr_"/> <img src="Docs/Images/Pink Paul.jpg" width="40" height="40" alt="Pink Paul" title="Pink Paul"/> <img src="Docs/Images/快乐的老鼠宝宝.jpg" width="40" height="40" alt="快乐的老鼠宝宝" title="快乐的老鼠宝宝"/> <img src="Docs/Images/七海无涯.jpg" width="40" height="40" alt="七海无涯" title="七海无涯"/> <img src="Docs/Images/深盾亲王欧根.jpg" width="40" height="40" alt="深盾亲王欧根" title="深盾亲王欧根"/> <img src="Docs/Images/Grayson Kun.jpg" width="40" height="40" alt="Grayson Kun" title="Grayson Kun"/> <img src="Docs/Images/祢梨.jpg" width="40" height="40" alt="祢梨" title="祢梨"/> <img src="Docs/Images/ええと.jpg" width="40" height="40" alt="ええと" title="ええと"/> <img src="Docs/Images/绫光.jpg" width="40" height="40" alt="绫光" title="绫光"/>

九州人士，Sliots，亲王，声控灯，隐，捡垃圾的垃圾佬，CriAngel，沙雕の方块，昊晨，我不姓高 abc，百变一点也不怪，柳知萧，磁爆步兵杨永信，月骨 ✾ۖ͡ 琉璃 ೄ೨，zhl，原味葱油饼干，Chr\_，Pink Paul，快乐的老鼠宝宝，七海无涯，深盾亲王欧根，Grayson Kun，祢梨，ええと，绫光

<!-- 在这里填写特别感谢名单，可以是任何人或事物。 -->

<span id="nav-15"></span>

## 版权许可

[License MIT](LICENSE)
