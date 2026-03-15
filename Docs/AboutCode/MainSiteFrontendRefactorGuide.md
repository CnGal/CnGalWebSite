# CnGal 主站前端重构指导（.NET 10 Blazor）

## 1. 文档目的

本文档用于指导 `CnGalWebSite.MainSite`、`CnGalWebSite.MainSite.Shared`、`CnGalWebSite.SDK.MainSite` 的前端重构，核心目标如下：

- 使用 `.NET 10 Blazor` 的**按页交互**模式（仅在需要交互的页面启用交互式渲染）。
- 让大多数页面保持**纯静态 SSR** 输出，优先首屏性能、可爬取性与稳定性。
- 去除第三方组件库依赖，统一为自研基础组件与样式体系。
- 保持 `MainSite.Shared` 只承担前端页面/组件职责，所有后端调用仅通过 `SDK.MainSite`。

---

## 2. 当前职责边界

### `CnGalWebSite.MainSite`

- Blazor 启动项目（Host），SDK 为 `Microsoft.NET.Sdk.Web`。
- 负责程序入口（`Program.cs`）、渲染模式注册（`AddInteractiveServerRenderMode`）、静态资源映射（`MapStaticAssets`）、全局中间件（认证/授权/防伪/HTTPS）、注册 `MainSite.Shared` 服务（`AddMainSiteSharedServices()`）。
- 包含根组件 `App.razor`、`Routes.razor`（使用 `AuthorizeRouteView`）、`Error.razor`、`NotFound.razor`。
- 不承载具体业务页面实现（页面在 `MainSite.Shared` 中）。
- 依赖 `MainSite.Shared` 和 `SDK.MainSite`。

### `CnGalWebSite.MainSite.Shared`

- 页面与组件层（Razor Class Library，SDK 为 `Microsoft.NET.Sdk.Razor`）。
- 包含布局、页面、组件、前端状态（仅 UI 相关）和前端服务（如 Toast 通知）。
- 不直接依赖 APIServer 接口实现，不直接写 HTTP 请求。
- 依赖 `SDK.MainSite`、`CnGalWebSite.Extensions`、`CnGalWebSite.Helper`。

### `CnGalWebSite.SDK.MainSite`

- 前后端通信封装层（SDK 为 `Microsoft.NET.Sdk`），引用 `Microsoft.AspNetCore.App` 框架。
- 包含 API Client、OIDC 认证/令牌管理、异常标准化。
- 对 `MainSite.Shared` 暴露面向业务的服务接口（而非原始 API 细节）。
- 依赖 `CnGalWebSite.DataModel`、`Microsoft.AspNetCore.Authentication.OpenIdConnect`。

### `CnGalWebSite.APIServer`

- 后端接口与业务逻辑实现。
- 前端只能通过 `SDK.MainSite` 间接访问。

---

## 3. 目标架构与渲染策略

### 3.1 页面分级策略（强制）

将页面分为两类，并在评审时明确标注：

1. **静态页（默认）**
   - 使用纯 SSR，不启用交互式渲染。
   - 当前已实现的静态页：`HomePage`、`EntryDetailPage`、`SpaceIndexPage`。
2. **交互页（按页启用）**
   - 仅当页面存在明显交互需求时启用交互式组件。
   - 当前已实现的交互页：`EntryEditPage`（`@rendermode InteractiveServer`）。

> 约束：默认新页面必须是静态页；如需交互，必须在 PR 说明中写明原因。

### 3.2 Blazor 渲染模式配置

- `Program.cs` 注册 `AddRazorComponents()` + `AddInteractiveServerComponents()`。
- `MapRazorComponents<App>()` 注册 `AddInteractiveServerRenderMode()` 并添加 `MainSite.Shared` 程序集。
- `App.razor` 和 `Routes.razor` **不写** `@rendermode`，保持全局静态 SSR。
- `Routes.razor` 使用 `CascadingAuthenticationState` + `AuthorizeRouteView`，默认布局为 `MainLayout`。
- 仅在具体页面根组件声明交互渲染（如 `@rendermode InteractiveServer`）。
- 禁止在布局级（Layout）或全局根组件上启用交互，防止"全站被动交互化"。

### 3.3 组件设计原则

- 页面组件：负责页面结构与数据编排（位于 `Pages/` 目录）。
- 业务组件：封装某一块业务视图（位于 `Components/Features/` 目录）。
- 基础组件（Design System）：按钮、输入框、弹窗、分页、Tag、表格等（位于 `Components/DesignSystem/` 目录）。
- 布局组件：全局壳与导航（位于 `Components/Layout/` 和 `Layout/` 目录）。
- 所有组件优先支持 SSR 输出；需要事件回调时再切入交互模式。

---

## 4. 去第三方组件库方案

### 4.1 目标

- 不再依赖第三方"组件库级别"包（如大型 UI 套件）。
- 样式与组件行为可控、可维护、可按需裁剪。

### 4.2 迁移方式

1. 先建立 `Design Tokens`（颜色、字号、圆角、间距、阴影、层级、动效时长）。
2. 用 CSS Variables + 语义化类名构建自研基础组件。
3. 按页面迁移：先替换通用组件，再替换业务页面。
4. 每迁移一个页面，删除对应第三方依赖调用点，最后清理包引用与静态资源。

### 4.3 当前基础组件清单（Design System）

以下组件已实现（共 23 个），均位于 `Components/DesignSystem/` 目录：

| 组件 | 说明 |
|---|---|
| `CgButton` | 通用按钮（支持 Variant、Async 加载态） |
| `CgInput` | 单行文本输入 |
| `CgTextarea` | 多行文本输入 |
| `CgSelect` | 通用下拉选择 |
| `CgEnumSelect` | 枚举专用下拉选择 |
| `CgCheckbox` | 复选框 |
| `CgDateInput` | 日期输入 |
| `CgDateInputMode` | `CgDateInput` 辅助枚举类型 |
| `CgAutoComplete` | 自动补全输入 |
| `CgModal` | 模态对话框 |
| `CgPopupMenu` | 弹出菜单 |
| `CgPagination` | 分页 |
| `CgTable` | 表格 |
| `CgGrid` | 网格布局 |
| `CgToast` | 轻量提示（Toast） |
| `CgAlert` | 提示信息（Alert） |
| `CgFormSection` | 表单区块 |
| `CgFormGroup` | 表单分组 |
| `CgImageUpload` | 图片上传（含 JS Interop） |
| `CgAudioUpload` | 音频上传 |
| `CgProgressRing` | 圆环进度指示器（确定进度/无限进度） |
| `CgHtmlContent` | HTML 内容显示（渲染 markdown 解析生成的 HTML，统一排版样式） |
| `CgMarkdownEditor` | Markdown 编辑器（含 JS Interop，支持工具栏与图片直传） |
| `CgTreeView` | 树视图组件（用于标签树等层级数据展示） |
| `CgMdiIcon` | MDI 图标（配合 `CgMdiIconMap.g.cs` 自动生成映射） |

---

## 5. SDK 层设计（支撑静态优先）

为支持"静态页优先"，`SDK.MainSite` 提供两类能力：

1. **SSR 友好查询接口（Queries）**
   - 面向页面提供只读 Query 服务（详情、列表、聚合数据）。
   - 返回可直接渲染的 ViewModel，减少页面二次拼装成本。
   - 当前已实现：`HomeQueryService`、`EntryQueryService`、`SpaceQueryService`、`TagQueryService`。
2. **交互动作接口（Commands）**
   - 编辑资料、提交审核、文件上传等操作型命令。
   - 统一错误对象与用户提示模型，避免组件层重复处理异常。
   - 当前已实现：`EntryCommandService`、`FileCommandService`。

### 5.1 当前 SDK 目录结构

```text
CnGalWebSite.SDK.MainSite
├─ Abstractions/          # 7 个接口
│  ├─ IHomeQueryService
│  ├─ IEntryQueryService
│  ├─ ISpaceQueryService
│  ├─ ITagQueryService
│  ├─ IEntryCommandService
│  ├─ IFileCommandService
│  └─ IMainSiteAuthRequestService
├─ Queries/               # 只读查询服务
│  ├─ HomeQueryService
│  ├─ EntryQueryService
│  ├─ SpaceQueryService
│  └─ TagQueryService
├─ Commands/              # 写操作命令服务
│  ├─ EntryCommandService
│  └─ FileCommandService
├─ Auth/                  # OIDC 认证与令牌管理
│  ├─ MainSiteAuthRequestService
│  ├─ AccessTokenHandler
│  └─ CookieOidcRefresher
├─ Models/                # ViewModel、结果模型、配置
│  ├─ SdkResult.cs
│  ├─ HomeSummaryViewModel.cs
│  ├─ EntryDetailViewModel.cs
│  ├─ SpaceDetailViewModel.cs
│  ├─ MainSiteOidcOptions.cs
│  ├─ EntryEdit/          # 词条编辑相关模型
│  └─ Files/              # 文件上传相关模型（ImageAspectType、ImageCropRect、AudioUploadResult）
├─ Infrastructure/        # 基础设施
│  ├─ QueryServiceBase    # 查询服务基类（HttpClient 封装、反序列化、日志截断）
│  ├─ CommandServiceBase  # 命令服务基类（POST/GET + JSON 序列化辅助）
│  ├─ SdkJsonSerializerOptions  # 全局统一 JSON 序列化配置
│  └─ StaffBatchParser    # Staff 批量文本解析工具
└─ Extensions/            # DI 注册扩展
   ├─ ServiceCollectionExtensions  # AddMainSiteSdk / AddMainSiteOidcAuthentication
   └─ EndpointRouteBuilderExtensions  # MapMainSiteAuthenticationEndpoints
```

### 5.2 关键基础设施

- **`SdkResult<T>`**：统一结果模型（`Success` + `Data` / `Error(Code, Message, StatusCode)`）。
- **`QueryServiceBase`**：查询服务基类，封装底层辅助方法（`GetAsyncWithBody`、`Deserialize<T>`、`TrimForLog`）及模板方法（`GetSingleAsync<TDto,TResult>`、`GetListSafeAsync<TItem>`、`GetAsync<T>`）。
- **`CommandServiceBase`**：命令服务基类，封装 `GetFromJsonAsync`、`PostAsJsonAsync`、`PostAsJsonRawAsync`、`ReadResponseAsync`，统一使用 `SdkJsonSerializerOptions.Default`。
- **`StaffBatchParser`**：Staff 批量文本解析工具（从文本批量导入 Staff 信息，移植自 `ToolHelper`）。
- **`SdkJsonSerializerOptions`**：全局统一 `JsonSerializerOptions`，开启 `PropertyNameCaseInsensitive` 和 `JsonStringEnumConverter`。
- **`AccessTokenHandler`**：`DelegatingHandler`，为 HttpClient 请求自动附加访问令牌。
- **DI 注册**：每个服务通过 `AddHttpClient<TInterface, TImpl>` 注册为类型化 HttpClient，并附加 `AccessTokenHandler`。

---

## 6. 分阶段实施进展

### 阶段 A：基础设施与规范 ✅ 已完成

- [x] 建立目录规范（`Pages/` / `Components/Features/` / `Components/DesignSystem/` / `Components/Layout/` / `Layout/`）。
- [x] 建立样式变量体系（`design-tokens.css`，包含颜色、间距、圆角、字号、字重、阴影、过渡等 `--cg-*` 变量）。
- [x] 建立基础组件（23 个 DesignSystem 组件）。
- [x] 确立页面渲染模式策略（静态默认、按页交互）。

### 阶段 B：核心页面迁移 ✅ 已完成

- [x] 首页（`HomePage`）— 纯静态 SSR。
- [x] 条目详情页（`EntryDetailPage`）— 纯静态 SSR。
- [x] 条目编辑页（`EntryEditPage`）— InteractiveServer（含草稿恢复、自动保存、客户端校验）。
- [x] 个人空间页（`SpaceIndexPage`）— 纯静态 SSR。
- [x] 封装 SDK Query 服务（Home、Entry、Space、Tag）与 Command 服务（Entry、File）。
- [x] 数据读取全部改为 SDK 服务。

### 阶段 C：交互页面精细化 🔄 进行中

- [x] 条目编辑页已按页启用 `@rendermode InteractiveServer`。
- [x] 编辑页包含完整交互功能（自动保存草稿、丢弃草稿、类型联动、表单校验、提交审核）。
- [ ] 其他需交互页面（如登录、个人中心编辑）待按需增加。
- [ ] 统一异常提示机制进一步完善。

### 阶段 D：收尾与治理

- [ ] 删除已不使用的第三方组件库包与静态资源。
- [ ] 统一 UI 一致性检查（间距、字号、颜色、交互反馈）。
- [ ] 补充更多页面（文章详情、搜索列表等）。
- [ ] 完成 E2E 冒烟与关键路径压测。

---

## 7. 当前目录结构

```text
CnGalWebSite.MainSite
├─ Components/
│  ├─ App.razor              # 根组件（HTML shell，引用 design-tokens.css，含 circuit-dead 检测脚本与 reconnect modal 逻辑）
│  ├─ Routes.razor            # 路由组件（CascadingAuthenticationState + AuthorizeRouteView）
│  ├─ _Imports.razor
│  ├─ Pages/
│  │  ├─ Error.razor
│  │  └─ NotFound.razor
│  └─ Layout/                 # （当前为空）
├─ Program.cs
├─ appsettings.json
└─ wwwroot/
```

```text
CnGalWebSite.MainSite.Shared
├─ Components/
│  ├─ DesignSystem/           # 23 个基础 UI 组件
│  │  ├─ CgButton.razor / .razor.css
│  │  ├─ CgInput.razor / .razor.css
│  │  ├─ CgSelect.razor / .razor.css
│  │  ├─ CgEnumSelect.razor
│  │  ├─ CgCheckbox.razor / .razor.css
│  │  ├─ CgDateInput.razor / .razor.css
│  │  ├─ CgDateInputMode.cs   # CgDateInput 辅助枚举
│  │  ├─ CgAutoComplete.razor / .razor.css
│  │  ├─ CgTextarea.razor / .razor.css
│  │  ├─ CgModal.razor / .razor.css
│  │  ├─ CgPopupMenu.razor / .razor.css
│  │  ├─ CgPagination.razor / .razor.css
│  │  ├─ CgTable.razor / .razor.css
│  │  ├─ CgGrid.razor / .razor.css
│  │  ├─ CgToast.razor / .razor.css
│  │  ├─ CgAlert.razor / .razor.css
│  │  ├─ CgFormSection.razor / .razor.css
│  │  ├─ CgFormGroup.razor / .razor.css
│  │  ├─ CgImageUpload.razor / .razor.css
│  │  ├─ CgAudioUpload.razor / .razor.css
│  │  ├─ CgProgressRing.razor / .razor.css
│  │  ├─ CgHtmlContent.razor / .razor.css
│  │  ├─ CgMarkdownEditor.razor / .razor.css
│  │  ├─ CgTreeView.razor / .razor.css
│  │  ├─ CgMdiIcon.razor / .razor.css
│  │  └─ CgMdiIconMap.g.cs   # 自动生成的图标映射
│  ├─ Features/               # 业务组件
│  │  ├─ Entry/
│  │  │  ├─ Detail/           # 词条详情（18 个组件）
│  │  │  └─ Editor/           # 词条编辑（16 个组件）
│  │  ├─ Home/                # 首页（14 个组件）
│  │  └─ Space/               # 个人空间（4 个组件）
│  └─ Layout/
│     └─ UserMenu.razor / .razor.css
├─ Layout/
│  └─ MainLayout.razor / .razor.css
├─ Pages/
│  ├─ Home/
│  │  └─ HomePage.razor / .razor.css
│  ├─ Entry/
│  │  ├─ EntryDetailPage.razor / .razor.css
│  │  └─ EntryEditPage.razor / .razor.css
│  └─ Space/
│     └─ SpaceIndexPage.razor / .razor.css
├─ wwwroot/
│  ├─ styles/
│  │  └─ design-tokens.css    # 全局设计变量
│  └─ scripts/
│     ├─ cg-image-upload.js   # 图片上传 JS Interop
│     ├─ cg-markdown-editor.js # Markdown 编辑器 JS Interop（工具栏、图片直传）
│     └─ cg-edit-nav.js       # 编辑页导航辅助 JS
├─ Services/
│  ├─ ICgToastService.cs      # Toast 通知服务接口
│  └─ CgToastService.cs       # Toast 通知服务实现
├─ Extensions/
│  └─ ServiceCollectionExtensions.cs  # AddMainSiteSharedServices
├─ _Imports.razor
└─ AssemblyMarker.cs
```

---

## 8. 工程约束（建议写入 CI 检查）

- 禁止在 `MainSite.Shared` 中出现业务 API 直连逻辑。
- 新增页面若含 `@rendermode`，必须附带"交互必要性"说明。
- 静态页禁止引入无必要 JS 互操作。
- 页面 PR 必须包含"替换了哪些第三方组件调用点"的说明。
- 组件必须提供基础可访问性（键盘焦点、语义标签、ARIA 基本属性）。

---

## 9. 验收标准（Definition of Done）

- 大多数页面以静态 SSR 渲染（当前目标：`>= 80%` 页面）。
- 交互能力仅在必要页面启用，且未出现全局交互化回退。
- 第三方组件库依赖移除完成（包、样式、脚本、组件调用点）。
- `MainSite.Shared` 不直接依赖 APIServer，全部通过 `SDK.MainSite`。
- 核心链路（首页 -> 详情 -> 登录 -> 互动）通过回归测试。

---

## 10. 风险与回滚预案

- 风险：样式重构导致 UI 回归  
  预案：按页面分批上线，保留截图对比和回滚分支。

- 风险：静态化后交互功能缺失  
  预案：交互页统一梳理清单，逐页确认 `@rendermode` 配置。

- 风险：SDK 接口抽象不足造成页面重复逻辑  
  预案：先定义 Query/Command 契约，再迁移页面。

---

## 11. 下一步工作

1. 继续补充更多页面（文章详情、搜索列表、登录/注册等）。
2. 清理并移除不再使用的第三方组件库依赖。
3. 完善 SDK 缓存策略（当前尚未建立独立 `Caching/` 模块）。
4. 统一 UI 一致性检查与可访问性审计。
5. 补充关键路径 E2E 测试。
