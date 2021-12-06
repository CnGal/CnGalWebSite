# 代码文档概述
## 内容目录

<details open="open">
  <summary>点我 打开/关闭 目录列表</summary>

- [代码文档概述](#代码文档概述)
  - [内容目录](#内容目录)
  - [简介](#简介)
  - [API端项目结构](#api端项目结构)

</details>

<span id="nav-1"></span>

## 简介
- 推荐第三方客户端开发者阅读 [API使用流程](/Docs/AboutCode/APIInstructions/Summary.md)，其在Swagger文档的基础上解释了各个接口之间如何协调实现业务流程
- 推荐想要修改代码实现功能或修复Bug的人员阅读 [代码逻辑思路]()，大致讲解了各个控制器方法的实现步骤，调用了哪些基础类库
- 项目整体是数据驱动类型的，整体仍然是MVC的架构，没有很复杂的对象包装继承。也限于开发者水平有限，没有再进一步提取公共方法封装，还请见谅

## API端项目结构
- Application 文件夹下是各个模块的公共方法，例如 BackUpArchiveService 是互联网档案馆备份模块的公共方法类。
- Controller 文件夹下是控制器，最外层暴露给用户的接口
- IRepository 是数据仓储接口，例如 IRepository\<Entry\> 是 词条仓储，提供了一些方法操作数据库
- AppDbContext 是数据库上下文
- appsettings.json 是配置文件

