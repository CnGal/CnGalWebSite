# 编辑记录与审核详情验收

自动测试使用本地模拟 API，不读取生产数据，也不向真实审核接口提交操作。浏览器测试标记为 Explicit，不会随常规单元测试启动。

## 回归测试

在仓库根目录执行：

```powershell
dotnet test CnGalWebSite/CnGalWebSite.UnitTest --filter "FullyQualifiedName~ExaminePresentationTests|FullyQualifiedName~ExamineComparisonSdkTests|FullyQualifiedName~MainSiteSdkRegressionTests|FullyQualifiedName~MainSiteEditorStateTests"
```

覆盖六类内容的全部 64 种有无组合及左右空侧、重复字段、媒体链接、单份提交、审核状态与权限矩阵、前置审核、重复提交、批注保留、记录切换与过期响应，以及五类历史重建接口和词条专属分组映射。

## 浏览器验收

需要本机安装 Microsoft Edge。以下命令分别在三个 PowerShell 终端中执行；环境变量只对第二个终端有效，不需修改配置文件。

```powershell
python CnGalWebSite/CnGalWebSite.UnitTest/Fixtures/examine_api.py
```

```powershell
$env:MainSiteApi__BaseAddress='http://127.0.0.1:5361/'
$env:MainSiteApi__TaskApiPath='http://127.0.0.1:5361/'
$env:ImageApi__BaseAddress='http://127.0.0.1:5361/'
$env:ASPNETCORE_ENVIRONMENT='Development'
$env:ASPNETCORE_URLS='http://127.0.0.1:5167'
dotnet run --project CnGalWebSite/CnGalWebSite.MainSite --no-launch-profile
```

```powershell
dotnet test CnGalWebSite/CnGalWebSite.UnitTest --filter "FullyQualifiedName~ExamineBrowserTests"
```

浏览器测试启动无头 Edge，阻止访问外部站点，并在每个用例前重置本地 API 状态。覆盖 375/768/1024/1440px、浅色/深色主题、手机记录导航折叠、桌面记录导航常驻、浏览器前进后退与刷新、公开版本选择、同版本与非法范围、无效参数、零条和单条记录、前置审核与只读权限、图片失效、提交失败重试、成功返回内容中心及并发处理。

截图输出到仓库 `.cache/examine-screenshots/`。测试结束后可用 Ctrl+C 停止两个本地服务。

## 数据语义

`GetExamineDetailAsync` 返回记录相关内容和查询时内容，不能用来组合完整的历史版本。新 SDK 方法 `GetEditRecordComparisonAsync` 分别调用五类对象已有的 `GetContrastEditRecordViews` 接口，后端按 ID 顺序回放已通过记录。旧 SDK 方法保留并转接到正确的数据来源。

历史页面保留 `ContrastId`、`CurrentId`、`SelectedId`，新增可选 `Mode=record|compare`；显式模式优先，否则携带 `CurrentId` 的旧链接进入对比模式。`CurrentId=0` 解析为最新已通过记录。页面对非数字、越界、反向和跨对象选择显示局部错误，不将缺失内容推断为删除。
