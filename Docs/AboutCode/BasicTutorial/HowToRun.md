# 开发环境配置&项目运行教程

## 内容目录

<details open="open">
  <summary>点我 打开/关闭 目录列表</summary>

- [开发环境配置&项目运行教程](#开发环境配置项目运行教程)
  - [内容目录](#内容目录)
  - [安装开发环境](#安装开发环境)
  - [配置数据库](#配置数据库)
  - [执行数据库迁移](#执行数据库迁移)
  - [更改API网址为本地项目地址](#更改api网址为本地项目地址)
  - [本地调试项目](#本地调试项目)

</details>

## 安装开发环境

选择安装 [Visual Studio 2022](https://visualstudio.microsoft.com/zh-hans/vs/) 需要安装 ASP .Net 和Web 开发的工作负载，也可以使用 VS Code 等编辑器并安装 .Net 6.0 SDK

## 配置数据库

打开NewCngalWebSite解决方案

切换到NewCngalWebSite.APIServer项目

![](https://cdn.nlark.com/yuque/0/2021/png/2357630/1625319336283-343044c2-2bc2-49c8-a05b-b4fb24dabe31.png)


打开appsetting.json编辑数据库连接字符串

![](https://cdn.nlark.com/yuque/0/2021/png/2357630/1625319437648-b12317f7-be27-4e46-826e-f49790bd8491.png)


## 执行数据库迁移

打开程序包管理控制台，工具->NuGet包管理器->程序包管理器控制台

切换启动项目为NewCngalWebSite.APIServer

![](https://cdn.nlark.com/yuque/0/2021/png/2357630/1625319744686-d0a6c336-e84c-4e69-a22d-affd8e43428b.png)


输入Update-Databse命令，按下回车

![](https://cdn.nlark.com/yuque/0/2021/png/2357630/1625319821276-d45561aa-4ca0-4cef-9763-05db104cd38f.png)

## 更改API网址为本地项目地址

打开ToolHeler.cs文件

![](https://cdn.nlark.com/yuque/0/2021/png/2357630/1625320430587-6193aed2-2a25-4799-926d-fe2f0032fe0a.png)

如图修改为本地网址localhost:51313

![](https://cdn.nlark.com/yuque/0/2021/png/2357630/1625320453757-59e39f5c-78a6-4423-a81a-6fa33a16d8b6.png)


## 本地调试项目

设置启动项目

右击NewCngalWebSite解决方案，单击弹出菜单上的设置启动项目

![](https://cdn.nlark.com/yuque/0/2021/png/2357630/1625320020938-049901ea-a8f9-4e29-a762-5375db1d860e.png)


选择合适的项目启动，一般为API端+Server模式渲染

![](https://cdn.nlark.com/yuque/0/2021/png/2357630/1625320089476-69eb9005-4881-4103-91b5-e538f4c7c9fe.png)

按F5或点击上方绿箭头调试

