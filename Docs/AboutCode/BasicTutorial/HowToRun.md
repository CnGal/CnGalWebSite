# 开发环境配置&项目运行教程

## 内容目录

<details open="open">
  <summary>点我 打开/关闭 目录列表</summary>

- [开发环境配置&项目运行教程](#开发环境配置项目运行教程)
  - [内容目录](#内容目录)
  - [安装开发环境](#安装开发环境)
  - [添加配置文件](#添加配置文件)
  - [配置数据库](#配置数据库)
  - [执行数据库迁移](#执行数据库迁移)
  - [更改API网址为本地项目地址](#更改api网址为本地项目地址)
  - [本地调试项目](#本地调试项目)

</details>

## 安装开发环境

选择安装 [Visual Studio 2022](https://visualstudio.microsoft.com/zh-hans/vs/) 需要安装 ASP .Net 和Web 开发的工作负载，也可以使用 VS Code 等编辑器并安装 .Net 6.0 SDK

## 添加配置文件

打开CnGalWebSite解决方案

切换到CnGalWebSite.APIServer项目

新建Json配置文件，按需求填充以下内容

````json
{
  "ConnectionStrings": {
    //数据库连接字符串
    "CnGalDBConnection": ""
  },
  //发送验证邮件的电子邮箱
  "Server": "",
  "Port": "465",
  "SenderName": "CnGal资料站",
  "SenderEmail": "",
  "Account": "",
  "Password": "",
  //百度API
  "BaiduAPIKey": "",
  "BaiduSecretKey": "",
  //微软第三方登入相关配置
  "ThirdPartyLoginMicrosoft_client_id": "",
  "ThirdPartyLoginMicrosoft_client_secret": "",
  "ThirdPartyLoginMicrosoft_resource": "",
  "ThirdPartyLoginMicrosoft_tenant": "",
  //Github第三方登入相关配置 SSR
  "ThirdPartyLoginGithub_SSR_client_id": "",
  "ThirdPartyLoginGithub_SSR_client_secret": "",
  //Github第三方登入相关配置 WASM
  "ThirdPartyLoginGithub_WASM_client_id": "",
  "ThirdPartyLoginGithub_WASM_client_secret": "",
  //Gitee第三方登入相关配置
  "ThirdPartyLoginGitee_client_id": "",
  "ThirdPartyLoginGitee_client_secret": "",
  //QQ第三方登入相关配置
  "ThirdPartyLoginQQ_client_id": "",
  "ThirdPartyLoginQQ_client_secret": "",

  //阿里云 短信账户
  "AccessKeyId": "",
  "AccessKeySecret": "",
  //互联网档案馆跳转链接
  "BackUpArchiveUrl": "",
  //image.cngal.org API密匙
  "CnGalImageAPIToken": "",
  //pic.cngal.top API密匙
  "SliotsImageAPIToken": "",
  //steam 密匙
  "SteamAPIToken": "",
  //Isthereanydeal 密匙
  "IsthereanydealAPIToken": "",
  //腾讯人机验证密匙
  "TencentCaptchaAppId": "",
  "TencentAppSecretKey": "",
  //腾讯人机验证密匙
  "GEETEST_ID": "",
  "GEETEST_KEY": "",
  //谷歌人机验证配置
  "RecaptchaSettings": {
    "SiteKey": "",
    "SecretKey": "",
    "Version": "",
    "Domain": ""
  },
  "RcaptchaAPIToken": "",
  //JWT令牌公匙
  "JwtSecurityKey": "",
  "JwtIssuer": "http://localhost",
  "JwtAudience": "http://localhost",
  "JwtExpiryInDays": 15
}

````
打开 Program.cs 文件

添加以下代码到 CreateHostBuilder 方法中

````csharp
.ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("你的配置文件的路径",
                        optional: true,
                        reloadOnChange: true);
                })
````
完整的 Program.cs 文件如下:
````csharp
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.APIServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(builder =>
                {
                    builder.AddCommandLine(args);//设置添加命令行
                })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("你的配置文件的路径",
                        optional: true,
                        reloadOnChange: true);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                });
        }
    }
}

````


## 配置数据库

默认使用MySQL数据库，如要使用其他数据库，请参阅 EF Core 使用说明

将数据库连接字符串填充到上一步配置文件里的 CnaglDBConnection 字段


## 执行数据库迁移

打开程序包管理控制台，工具->NuGet包管理器->程序包管理器控制台

切换启动项目为CnGalWebSite.APIServer

![](https://cdn.nlark.com/yuque/0/2021/png/2357630/1625319744686-d0a6c336-e84c-4e69-a22d-affd8e43428b.png)


输入Update-Databse命令，按下回车

![](https://cdn.nlark.com/yuque/0/2021/png/2357630/1625319821276-d45561aa-4ca0-4cef-9763-05db104cd38f.png)

## 更改API网址为本地项目地址

切换 CnGalWebSite.DataModel 项目

```
-- CnGalWebSite.DataModel
   |-- Helper
       |-- ToolHelper.cs
```


打开 ToolHelper.cs 文件

打开ToolHeler.cs文件

如图修改为本地地址 http://localhost:45160/
````csharp
public const string WebApiPath = "http://172.17.0.1:2001/";
````


## 本地调试项目

设置启动项目

右击NewCngalWebSite解决方案，单击弹出菜单上的设置启动项目

![](https://cdn.nlark.com/yuque/0/2021/png/2357630/1625320020938-049901ea-a8f9-4e29-a762-5375db1d860e.png)


选择合适的项目启动，一般为API端+Server模式渲染

![](https://cdn.nlark.com/yuque/0/2021/png/2357630/1625320089476-69eb9005-4881-4103-91b5-e538f4c7c9fe.png)

按F5或点击上方绿箭头调试

