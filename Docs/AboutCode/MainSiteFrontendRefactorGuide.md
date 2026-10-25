# CnGal 主站前端重构指导（.NET 10 Blazor）

## 1. 文档目的

本文档用于指导 `CnGalWebSite.MainSite`、`CnGalWebSite.MainSite.Shared`、`CnGalWebSite.SDK.MainSite`、`CnGalWebSite.SDK.MainSite.AspNet` 的前端重构，核心目标如下：

- 使用 `.NET 10 Blazor` 的**按页交互**模式（仅在需要交互的页面启用交互式渲染）。
- 让大多数页面保持**纯静态 SSR** 输出，优先首屏性能、可爬取性与稳定性。
- 去除第三方组件库依赖，统一为自研基础组件与样式体系。
- 保持 `MainSite.Shared` 只承担前端页面/组件职责，所有后端调用仅通过 `SDK.MainSite`。

---

## 2. 当前职责边界

### `CnGalWebSite.MainSite`

- Blazor 启动项目（Host），项目 SDK 为 `Microsoft.NET.Sdk.Web`，目标框架 `net10.0`。
- 负责程序入口、渲染模式注册、OIDC 认证接入、SDK/Shared 服务注册、中间件、静态资源映射、健康检查、Circuit 处理器和 HTML 缓存策略。
- 包含根组件 `App.razor`、`Routes.razor`（`CascadingAuthenticationState` + `AuthorizeRouteView`）、`Error.razor`、`NotFound.razor`。
- 不承载具体业务页面实现（页面在 `MainSite.Shared` 中）。
- 引用 `MainSite.Shared`、`SDK.MainSite`、`SDK.MainSite.AspNet` 和 `HealthCheck`。

### `CnGalWebSite.MainSite.Shared`

- 页面与组件层（Razor Class Library，项目 SDK 为 `Microsoft.NET.Sdk.Razor`）。
- 包含布局、页面、组件、浏览器控制器、领域编辑会话和前端 UI 服务。
- 不直接依赖 APIServer 实现；C# 代码不直接写业务 HTTP 请求。Markdown 编辑器保留浏览器端图片直传的既有协议，且不向浏览器注入宿主访问令牌。
- 引用 `SDK.MainSite`、`CnGalWebSite.Extensions`，并直接引用 `Blazored.LocalStorage`。

### `CnGalWebSite.SDK.MainSite`

- 核心通信层（项目 SDK 为 `Microsoft.NET.Sdk`），包含 API Client、领域映射、页面聚合、缓存、取消和错误标准化。
- 不依赖 ASP.NET 宿主类型；`ICurrentUserAccessor` 等宿主能力由 `SDK.MainSite.AspNet` 实现。
- 引用 `CnGalWebSite.DataModel`、`CnGalWebSite.DrawingBed.Models`、`CnGalWebSite.Extensions`、`CnGalWebSite.TimedTask.Models`。

### `CnGalWebSite.SDK.MainSite.AspNet`

- 宿主适配层，负责 OIDC/Cookie 认证、令牌存储与刷新、`AccessTokenHandler`、`ICurrentUserAccessor` 实现、认证端点及带身份的 HttpClient 创建。
- 引用 `SDK.MainSite` 与 `Microsoft.AspNetCore.Authentication.OpenIdConnect`。

### `CnGalWebSite.APIServer`

- 后端接口与业务逻辑实现。
- 前端只能通过 `SDK.MainSite` 间接访问。

---

## 3. 目标架构与渲染策略

### 3.1 页面分级策略（强制）

将页面分为三类，并在评审时明确标注：

1. **静态页（默认）**
   - 使用纯 SSR，不启用页面级交互式渲染。
   - 当前页面根：`HomePage`、`SearchPage`、`SquarePage`、`ShareGamesPage`、`EntryDetailPage`、`ArticleDetailPage`、`VideoDetailPage`、`TagDetailPage`、`PeripheryDetailPage`、`FavoriteFolderDetailPage`、`LotteryDetailPage`、`LotteryHomePage`、`SpaceIndexPage`、`BirthdayCalendarPage`、`EditRecordPage`、`BindSteamPage`、`FreeRedirectPage`。
2. **整页交互页（按页启用）**
   - 预渲染交互：`CommoditiesPage`、`CnGalGenerationPage`、`ExpoAnniversaryPage`、`ExpoLotteryPage`、`CVThematicPage`、`DiscountPage`、`GamePublishTimesPage`、`RevenuePage`、`PerfectionHomePage`、`TableSummaryPage`。
   - 不预渲染交互（`new InteractiveServerRenderMode(prerender: false)`）：`Admin/*`、`EntryEditPage`、`ArticleEditPage`、`VideoEditPage`、`TagEditPage`、`PeripheryEditPage`、`LotteryEditPage`、`FavoriteFolderEditPage`、`SpaceEditPage`、`ExamineDetailPage`、`DrawLotteryPage`、`LotteryManagementPage`、`MessagePage`、`TasksPage`、`UserCenterPage`。
3. **静态页中的局部交互根（允许）**
   - 评论、点赞/收藏、评分、游玩记录、工具箱、看板娘等由局部组件声明交互模式，不升级整页。
   - 仅登录后可见的局部控件必须由 `AuthorizeView` 或等效授权机制包裹。

> 约束：默认新页面必须是静态页；如需整页交互，必须在 PR 说明中写明原因。局部交互根应沿用现有组件边界，不通过页面基类或全局状态实现。`CgMiniModeContainer` 用于 MiniMode/审核模式，不是移动端设备检测；设备适配使用 CSS `@media` 查询。

### 3.2 Blazor 渲染模式配置

- `Program.cs` 注册 `AddRazorComponents()` + `AddInteractiveServerComponents()`，并注册 `AddMainSiteOidcAuthentication()`、`AddMainSiteSdk()`、`AddMainSiteSharedServices()`。
- `MapRazorComponents<App>()` 使用 `AddAdditionalAssemblies(typeof(AssemblyMarker).Assembly)`、`AddInteractiveServerRenderMode()` 和 `DisableAntiforgery()`。
- `App.razor` 和 `Routes.razor` **不写** `@rendermode`，保持全局静态 SSR。
- `Routes.razor` 使用 `CascadingAuthenticationState` + `AuthorizeRouteView`，默认布局为 `MainLayout`。
- 仅在具体页面根组件或明确的局部业务根声明交互渲染。编辑页、后台页通常使用 `prerender: false`，避免无意义的预渲染。
- 禁止在布局级（Layout）或全局根组件上整体启用交互，防止"全站被动交互化"。

### 3.3 组件设计原则

- 页面组件负责页面结构与数据编排，位于 `Pages/`。
- 业务组件封装某一块业务视图，位于 `Components/Features/`。
- 基础组件（Design System）位于 `Components/DesignSystem/`，当前共 47 个 `.razor` 组件，除 `UserAtmosphere` 外使用 `Cg` 前缀。
- 布局组件位于 `Components/Layout/` 和 `Layout/`；全局壳为 `MainLayout` 与 `AdminLayout`。
- 展示组件优先支持 SSR 输出；需要事件回调时再声明交互模式，并由最小业务根持有状态。

### 3.4 浏览器模块与生命周期（强制）

- `wwwroot/CnGalWebSite.MainSite.Shared.lib.module.js` 是唯一初始化入口，负责 IME 事件、`cg-image-gallery`、`cg-carousel`、`cg-edit-nav`、组件生命周期与 `site-shell`。
- 专用实现放在同名 `.razor.js`；自动挂载契约是 `mount(root, { signal }) => { update, dispose, suspend?, resume? }`，使用 Light DOM。
- 异步初始化必须检查连接代次和取消信号；`update` 幂等，`dispose` 幂等并清理监听、观察器、定时器、动画和对象 URL。
- 组件发现不得依赖全局扫描；局部变化使用 `observeContent` 等局部观察，并排除嵌套实例。`cg-owned` 只负责 DOM 移除时释放控制器。
- 初始化失败必须清理并显示可重试状态（`data-cg-module-error` / `data-cg-module-retry`）。

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

当前 `Components/DesignSystem/` 有 47 个 `.razor` 组件和 10 个辅助 `.cs` 文件。按用途分组如下（完整清单以目录为准）：

- **表单、上传与编辑器**：`CgButton`、`CgInput`、`CgTextarea`、`CgSelect`、`CgEnumSelect`、`CgCheckbox`、`CgDateInput`、`CgAutoComplete`、`CgSlider`、`CgImageUpload`、`CgAudioUpload`、`CgMarkdownEditor`、`CgHumanMachineVerification`。
- **弹层、反馈与导航**：`CgModal`、`CgPopupMenu`、`CgToast`、`CgAlert`、`CgProgressRing`、`CgTabs`、`CgBottomNav`、`CgEditSidebar`、`CgHiddenTip`、`CgHideToggleButton`。
- **数据、图表与媒体**：`CgTable`、`CgDataTable`、`CgGrid`、`CgPagination`、`CgTreeView`、`CgRating`、`CgRankBadge`、`CgRankList`、`CgHeatMap`、`CgBarChart`、`CgLineChart`、`CgPieChart`、`CgCarousel`、`CgImageGallery`、`CgMdiIcon`、`CgHtmlContent`。
- **布局、SEO 与氛围**：`CgPageMeta`、`CgStructuredData`、`CgFormSection`、`CgFormGroup`、`CgAtmosphere`、`CgAtmosphereUser`、`UserAtmosphere`、`CgMiniModeContainer`。

---

## 5. SDK 层设计（支撑静态优先）

`SDK.MainSite` 是纯核心层；`SDK.MainSite.AspNet` 承担 OIDC、令牌和宿主适配。

### 5.1 能力划分

1. **SSR 友好查询接口（Queries）**
   - 面向页面提供只读 Query 服务，返回可直接渲染的模型。
   - 除领域查询外，还包括 `IEntryPageQueryService`、`IArticlePageQueryService`、`IVideoPageQueryService`、`ITagPageQueryService`、`IPeripheryPageQueryService` 等页面聚合接口。
2. **交互动作接口（Commands）**
   - 编辑提交、审核、收藏、评论、抽奖、周边、文件上传等写操作。
   - 当前覆盖 Entry、Article、Video、Tag、Space、Lottery、Periphery、PlayedGame、Comment、FavoriteFolder、Toolbox、File、Examine、Admin、Expo 等领域。
3. **认证与工具**
   - `Auth/MainSiteAuthRequestService` 负责登录/退出 URL 构造；OIDC、令牌和 HttpClient 适配位于 `SDK.MainSite.AspNet`。
   - 所有异步接口接受 `CancellationToken`，查询与命令统一返回 `SdkResult<T>`。

### 5.2 当前目录结构

```text
CnGalWebSite.SDK.MainSite
├─ Abstractions/          # 查询、命令、页面聚合、身份与任务接口
├─ Queries/               # *QueryService、*PageQueryService
├─ Commands/              # *CommandService
├─ Auth/                  # MainSiteAuthRequestService
├─ Models/                # 页面/结果模型及编辑、文件、评论、看板、工具箱子目录
├─ Infrastructure/        # 基础类、PublicDataCache、SdkRequestPolicy、序列化、解析工具
├─ Services/Toolbox/      # 工具箱业务编排
└─ Extensions/            # AddMainSiteSdkCore

CnGalWebSite.SDK.MainSite.AspNet
├─ Auth/                  # OIDC、令牌存储、AccessTokenHandler、身份访问器
├─ Extensions/            # DI 注册与认证端点
└─ Services/Toolbox/      # ToolboxLocalRepository
```

### 5.3 关键基础设施

- **`SdkResult<T>`**：统一结果模型（`Success` + `Data` / `Error(Code, Message, StatusCode)`）。
- **`QueryServiceBase`**：封装 `GetAsyncWithBody`、`Deserialize<T>`、`TrimForLog` 及 `GetSingleAsync`、`GetListSafeAsync`、`GetResultSafeAsync`、`GetAsync`、`PostAsync`。
- **`CommandServiceBase`**：封装 `GetFromJsonAsync`、`PostAsJsonAsync`、`PostAsJsonRawAsync`、`ReadResponseAsync`，写请求出口会推进对应 API 来源的公共缓存失效代次。
- **`SdkResponse` / `SdkFailure`**：统一 Command 响应读取和异常映射；4xx JSON 中明确的业务拒绝保留提示和 HTTP 状态码，其他 HTTP 错误不回显原始正文。
- **`SdkJsonSerializerOptions`**：统一 Web JSON 配置、字符串枚举转换和 DateTime 转换。
- **`PublicDataCache`**：按 API 来源、身份代次和失效代次封装匿名公共缓存；查询开始时调用 `BeginQueryAsync` 捕获作用域。
- **`SdkRequestPolicy`**：只有显式标记 `AllowReplay` 的请求允许重试；GET 读取辅助方法会设置该标记，写接口统一使用 POST。标准弹性处理器和 `AccessTokenHandler` 的 401 刷新重发都只作用于可重放请求。
- **`PublicHtmlCachePolicy` / `PageResponseState`**：由页面上报渲染结果，宿主决定 HTML 是否允许共享缓存；默认 `private, no-store`。
- **DI 注册**：`AddMainSiteSdkCore` 统一创建命名 HttpClient；`SDK.MainSite.AspNet.AddMainSiteSdk` 注册身份访问器、Circuit 令牌存储和带认证的客户端工厂。

### 5.4 身份、缓存与 HTML 共享

- `ICurrentUserAccessor` 显式区分 `Unknown`、`Anonymous`、`Authenticated` 并提供身份代次；登录或身份未确定时不得写入匿名公共缓存。
- `SessionTokenStore`、`CircuitTokenStore`、`AuthenticationCircuitHandler` 和 `AuthenticatedSdkClients` 负责令牌快照、Circuit 生命周期和认证传输；POST 默认不自动重发，`GetFromJsonAsync` 仅用于可安全重试的 GET 读取，写接口统一使用 POST。
- `PublicDataCache` 失效采用来源级代次推进，旧代次响应不得回填；并发读取仍返回其正常快照或原始错误，不因其他用户的写操作失败或重试。身份变化仍会拦截旧结果；缓存对象按只读快照使用。
- `PublicHtmlCachePolicy` 不是路由白名单：页面只要通过 `PageResponseState` 标记成功，且请求为匿名 GET、响应为 200、无 `Authorization` / `Set-Cookie`、无 MiniMode Cookie 或查询参数、除主题 Cookie 外无其他 Cookie，即可共享缓存。主题只由客户端脚本应用；MiniMode 会改变 SSR 输出，因此被明确排除。

### 5.5 SEO、主题与通知

- 每页使用单个 `CgPageMeta` 作为 `HeadContent` 所有者，组合标题、描述、主图和 JSON-LD；`CgStructuredData` 只输出数据标签。交互页在 `prerender: false` 时由 `CgPageMeta` 同步更新 `document.head`。
- 主题以 `cg-theme` Cookie 持久化，由首屏脚本和 `site-shell` 在浏览器应用；主题不生成服务器 HTML 变体。MiniMode 保留服务端判定语义。
- Toast 的展示、数量限制和退出时间由浏览器模块处理，C# 通过 `ICgToastService` 发送通知。

---

## 6. 分阶段实施进展

### 阶段 A：基础设施与规范 ✅ 已完成

- [x] 建立 `MainSite` / `MainSite.Shared` / `SDK.MainSite` / `SDK.MainSite.AspNet` 分层。
- [x] 建立样式变量体系与 47 个 DesignSystem 基础组件。
- [x] 确立静态默认、按页交互和局部交互根策略。

### 阶段 B：核心页面迁移 ✅ 已完成

- [x] 首页、搜索、广场、分享游玩记录、词条/文章/视频/标签/周边/收藏夹/抽奖详情、个人空间等公开页面已迁移为静态 SSR。
- [x] 词条、文章、视频、标签、周边、抽奖、空间和收藏夹编辑页已接入 SDK 与领域编辑会话。
- [x] 评论、收藏、评分、游玩记录、抽奖操作、工具箱等静态页局部交互根已按需启用交互。

### 阶段 C：交互与缓存精细化 🔄 进行中

- [x] 编辑页接入 `EditSessionHost<TSession>`、`EditDraftController<TDraft>`、`DraftSaveLoop` 和各领域 EditSession。
- [x] 管理后台、用户内容中心、审核、消息、任务及多种筛选页已按页启用 InteractiveServer。
- [x] 公共数据缓存、请求重放策略和 HTML 共享缓存策略已落地。
- [ ] 统一异常提示、UI 一致性和可访问性审计仍需持续完善。
- [ ] 关键路径 E2E 冒烟与压测尚未执行。

### 阶段 D：收尾与治理 🔄 进行中

- [x] `MainSite.Shared` 未直接引用第三方 UI 组件库；当前直接包引用仅 `Blazored.LocalStorage`。
- [ ] 统一 UI 一致性检查（间距、字号、颜色、交互反馈）。
- [ ] 完成关键路径与缓存边界的自动化回归。
- [ ] 发布环境仍需落实 CDN 清理、不可变镜像和旧指纹资源保留策略。

---

## 7. 当前目录结构

```text
CnGalWebSite.MainSite
├─ Components/
│  ├─ App.razor
│  ├─ Routes.razor
│  └─ Pages/                 # Error.razor、NotFound.razor
├─ Services/                 # PublicHtmlCachePolicy、Circuit 处理器
├─ Program.cs
├─ appsettings.json
└─ wwwroot/
```

```text
CnGalWebSite.MainSite.Shared
├─ Components/
│  ├─ DesignSystem/          # 47 个基础组件、CgMdiIconMap.g.cs
│  ├─ Features/              # Admin、Article、Comment、Entry、Examine、Home、Lottery、Space、Tag、Toolbox 等
│  └─ Layout/                # UserMenu、CreateMenu、ThemeToggle、UserSessionTracker 等
├─ Layout/                   # MainLayout、AdminLayout
├─ Pages/                    # 各领域页面
├─ Services/                 # Toast、MiniMode、草稿/编辑会话、图表控制器、看板服务等
│  └─ Editing/               # EditSessionHost、EditDraftController、各领域 EditSession
├─ wwwroot/
│  ├─ CnGalWebSite.MainSite.Shared.lib.module.js
│  ├─ scripts/               # site-shell、component-lifecycle、dialog、edit-nav、canvas-controller 等
│  └─ styles/design-tokens.css
├─ Extensions/               # AddMainSiteSharedServices
├─ _Imports.razor
└─ AssemblyMarker.cs
```

```text
CnGalWebSite.SDK.MainSite
├─ Abstractions/
├─ Queries/
├─ Commands/
├─ Auth/
├─ Models/
├─ Infrastructure/
├─ Services/Toolbox/
└─ Extensions/

CnGalWebSite.SDK.MainSite.AspNet
├─ Auth/
├─ Extensions/
└─ Services/Toolbox/
```

---

## 8. 工程约束（建议写入 CI 检查）

- 禁止在 `MainSite.Shared` 中出现业务 API 直连逻辑。
- 新增页面若含 `@rendermode`，必须附带"交互必要性"说明。
- 静态页中仅登录可见的控件允许声明 `@rendermode InteractiveServer`，此类控件必须被 `AuthorizeView` 包裹，无需整页交互声明，但需在 PR 中注明涉及的控件。
- 静态页禁止引入无必要 JS 互操作。
- 页面 PR 必须包含"替换了哪些第三方组件调用点"的说明。
- 组件必须提供基础可访问性（键盘焦点、语义标签、ARIA 基本属性）。
- 浏览器模块是否遵守 `mount(root, { signal })` 生命周期与失败重试契约？
- 改动静态公共页面时，是否同步核对 `PageResponseState` 与 `PublicHtmlCachePolicy`？

---

## 9. 验收标准（Definition of Done）

- 公开内容页优先静态 SSR；整页交互仅限明确需要即时状态的页面或后台。
- 交互能力仅在必要页面/局部根启用，且未出现全局交互化回退。
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

1. 持续核对文档与目录/渲染模式清单，避免页面新增后清单漂移。
2. 统一 UI 一致性检查与可访问性审计。
3. 补充 `PublicDataCache`、`PublicHtmlCachePolicy` 及取消/重放边界的自动化测试。
4. 补充关键路径 E2E 冒烟与压测。
5. 发布前确认 CDN 缓存规则、不可变镜像和旧指纹资源回退方案。
