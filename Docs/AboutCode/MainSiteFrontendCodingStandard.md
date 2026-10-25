# CnGal 主站前端重构编码规范（Blazor + SDK）

## 1. 文档定位

本规范用于指导以下项目的重构与后续增量开发：

- `CnGalWebSite.MainSite`（Host 启动层）
- `CnGalWebSite.MainSite.Shared`（页面/组件层）
- `CnGalWebSite.SDK.MainSite`（核心通信、映射与聚合层）
- `CnGalWebSite.SDK.MainSite.AspNet`（OIDC、身份与宿主适配层）

目标：在 `.NET 10` 下落地"**静态 SSR 优先 + 按页交互 + SDK 统一数据出口**"，并形成可评审、可检查、可维护的工程标准。

---

## 2. 架构总原则（强制）

### 2.1 分层职责（MUST）

- `MainSite` 负责启动、认证接入、中间件、根文档、路由、静态资源、健康检查、Circuit 服务及 HTML 缓存策略，不承载业务页面逻辑。
- `MainSite.Shared` 负责页面、布局、组件、纯 UI 状态、浏览器控制器与编辑会话；其 C# 代码不直接发业务 HTTP 请求。Markdown 编辑器保留浏览器端图片直传的既有协议，且不向浏览器注入宿主访问令牌。
- `SDK.MainSite` 负责 API 请求、DTO/ViewModel 映射、页面聚合、公共缓存、取消与错误标准化，不依赖 ASP.NET 宿主。
- `SDK.MainSite.AspNet` 负责 OIDC/Cookie 认证、令牌存储与刷新、`ICurrentUserAccessor` 实现、认证端点及宿主 HttpClient 适配。
- 前端访问后端必须经过 SDK（禁止绕过 SDK 直连业务接口）。

### 2.2 渲染策略（MUST）

- 默认页面使用静态 SSR。
- 仅在存在明确交互必要性时，在页面根组件声明 `@rendermode InteractiveServer`；编辑页和后台页可使用 `@rendermode @(new InteractiveServerRenderMode(prerender: false))`。
- 静态页中仅登录后可见的局部控件（如点赞、收藏、评分、评论等）可单独声明 `@rendermode InteractiveServer`，并通过 `AuthorizeView` 或等效机制控制可见性。
- 禁止在 `App.razor`、`Routes.razor`、`MainLayout.razor` 整体启用交互渲染；布局内确有必要的局部交互组件可单独声明。

### 2.3 依赖方向（MUST）

- 允许依赖：`MainSite -> MainSite.Shared -> SDK.MainSite`；`MainSite -> SDK.MainSite.AspNet -> SDK.MainSite`。
- 禁止依赖：`SDK.MainSite -> MainSite.Shared`、`MainSite.Shared -> APIServer 具体实现`。
- `MainSite.Shared` 当前引用 `CnGalWebSite.Extensions` 与 `SDK.MainSite`。
- `SDK.MainSite` 当前引用 `CnGalWebSite.DataModel`、`CnGalWebSite.DrawingBed.Models`、`CnGalWebSite.Extensions`、`CnGalWebSite.TimedTask.Models`。
- `SDK.MainSite.AspNet` 引用 `SDK.MainSite` 与 `Microsoft.AspNetCore.Authentication.OpenIdConnect`。

---

## 3. SDK 设计规范（重构核心）

### 3.1 接口分层（MUST）

- SDK 服务按语义拆分：
  - `Queries/`：只读查询（如 `HomeQueryService`、`EntryQueryService`、`SpaceQueryService`、`TagQueryService`、`ArticleQueryService`、`VideoQueryService`）；`IEntryPageQueryService` 等页面聚合接口也在此层实现。
  - `Commands/`：写操作，覆盖词条、文章、视频、标签、空间、抽奖、周边、收藏夹、评论、工具箱、文件、审核、后台与展会等。
- 认证请求构造位于 `Auth/MainSiteAuthRequestService`；OIDC、令牌和宿主认证适配位于 `SDK.MainSite.AspNet/Auth/`。
- 交互页不得直接拼接 API 路径；路径细节只允许存在于 SDK 内部。

### 3.2 契约设计（MUST）

- 对外暴露接口位于 `Abstractions/`，常规命名为 `I{Domain}{Query|Command}Service`；页面聚合接口为 `I{Domain}PageQueryService`，身份访问为 `ICurrentUserAccessor`。
- 面向页面的聚合结果模型放在 `Models/`；模型内部可组合 DataModel/DTO，但页面不直接发 HTTP。
- 所有 SDK 异步接口必须支持 `CancellationToken`。

### 3.3 返回结果模型（MUST）

- 统一使用 `SdkResult<T>` 结果对象：
  - 成功：`Success=true` + `Data`
  - 失败：`Success=false` + `SdkErrorModel(Code, Message, StatusCode)`
- 使用 `SdkResult<T>.Ok(data)` 和 `SdkResult<T>.Fail(code, message, statusCode)` 工厂方法创建结果。
- 禁止把异常原样抛到页面层作为常规流程控制。
- 页面层只消费"可展示的错误语义"，不处理底层异常类型分发。

### 3.4 容错与日志（MUST）

- Query 服务继承 `QueryServiceBase`，复用 `GetAsyncWithBody`、`Deserialize<T>`、`TrimForLog` 及模板方法 `GetSingleAsync`、`GetListSafeAsync`、`GetResultSafeAsync`、`GetAsync`、`PostAsync`。
- Command 服务继承 `CommandServiceBase`，复用 `GetFromJsonAsync`、`PostAsJsonAsync`、`PostAsJsonRawAsync`、`ReadResponseAsync`，序列化统一使用 `SdkJsonSerializerOptions.Default`。
- 每个外部调用必须覆盖 HTTP 非成功、反序列化失败和运行时异常；取消不得伪装为有效空数据。
- 日志必须包含 `Path`、`StatusCode`、`BaseAddress`、业务 `ErrorCode` 等定位信息。
- `TrimForLog` 当前只输出响应体长度并省略正文，禁止在普通日志中输出私人响应正文、令牌或认证票据。

### 3.5 序列化与兼容（MUST）

- 全 SDK 统一使用 `SdkJsonSerializerOptions.Default`，禁止服务内各自 new 不同配置。
- `SdkJsonSerializerOptions` 配置：
  - `JsonSerializerDefaults.Web`（camelCase 命名）
  - `PropertyNameCaseInsensitive = true`
  - 注册 `JsonStringEnumConverter`（兼容 APIServer 返回的字符串枚举值）
  - 注册 `DateTime` / `DateTime?` 转换器（API 按 UTC 传输，站点内部按本地时间处理）
- Command 服务中的 `GetFromJsonAsync` / `ReadFromJsonAsync` 调用也必须传入 `SdkJsonSerializerOptions.Default`。
- 传给交互根的初始模型中，可空 `required` 成员使用 `[JsonIgnore(Condition = JsonIgnoreCondition.Never)]` 保留显式 `null`，避免 Blazor 参数还原失败；不要全局放宽必需成员校验。

### 3.6 DI 注册规范（MUST）

- 所有注册统一通过 `AddMainSiteSdkCore` / `AddMainSiteSdk` 完成。
- Core 按 API、图片 API、任务 API 分别创建命名 HttpClient，并配置标准弹性处理器；只有显式标记 `AllowReplay` 的请求允许重试。写接口必须使用 POST，`GetFromJsonAsync` 仅用于可安全重试的 GET 读取。
- AspNet 注册 `ICurrentUserAccessor`、`CircuitTokenStore`、`AuthenticationCircuitHandler` 与 `AuthenticatedSdkClients`，并由后者以非拥有式传输桥接创建带身份的 HttpClient。
- 不再使用 `AddHttpClient<TInterface, TImpl>` + `AccessTokenHandler` 的旧注册模式。

### 3.7 模型映射（SHOULD）

- SDK 内部完成 DTO/DataModel -> ViewModel 映射，页面不做二次业务组装。
- 进行字段兜底（空字符串、默认链接、空集合）保证页面渲染稳定。

---

## 4. Blazor 页面与组件拆分规范

### 4.1 页面职责（MUST）

- `Pages/{Domain}/{PageName}.razor` 只负责：
  - 路由声明
  - 页面级数据装配（调用 SDK）
  - 页面骨架与区域编排
  - 加载态、错误态、成功态三分支渲染
- 页面中禁止出现复杂可复用 UI 结构（抽取到组件层）。
- SEO 由每页单个 `CgPageMeta` 统一设置标题、描述、主图与 JSON-LD；`CgStructuredData` 只输出数据标签。
- 可被 CDN 共享的页面须通过 `PageResponseState` 标记成功/失败；主数据失败时返回 `Failed`，不得让错误页进入公共缓存。

### 4.2 组件分层（MUST）

- `Components/DesignSystem/`：基础组件（Button/Input/Modal/图表/上传/SEO 等），当前 47 个 `.razor` 组件，除 `UserAtmosphere` 外使用 `Cg` 前缀。
- `Components/Features/{Domain}/`：业务组件，当前覆盖 `Admin`、`Analytics`、`Article`、`Comment`、`Commodities`、`Entry`、`Examine`、`Expo`、`FavoriteFolder`、`Home`、`Kanban`、`Lottery`、`Message`、`Perfections`、`Periphery`、`Space`、`Tag`、`Toolbox`、`Video`；按 `Detail/`、`Editor/` 等子目录分组。
- `Components/Layout/`：布局辅助组件（`UserMenu`、`CreateMenu`、`ThemeToggle`、`UserSessionTracker`、`FloatingActionToolbar`、`KanbanToggleContainer`）。
- `Layout/`：全局壳（`MainLayout`、`AdminLayout`），不承载业务查询逻辑。

### 4.3 组件 API 设计（MUST）

- 参数命名语义化：`Variant`、`Disabled`、`CurrentPage`、`OnClose`。
- 事件统一使用 `EventCallback` / `EventCallback<T>`。
- 可双向绑定组件必须遵循 `Value` + `ValueChanged` 模式。
- 不可变输入优先，避免子组件修改父状态。

### 4.4 生命周期规范（MUST）

- 首次加载优先 `OnInitializedAsync`。
- 依赖路由参数变化的加载逻辑使用 `OnParametersSetAsync`（如详情页、编辑页与个人空间页）。
- 页面必须传递 `CancellationToken`，路由切换时取消旧请求，并在回写前核对请求代次。

### 4.5 交互边界（MUST）

- 静态页禁止引入仅交互页需要的脚本与状态逻辑。
- 如果页面只需局部交互，优先局部交互组件而非整页交互化。
- **仅登录可见控件的交互性**：静态页中仅登录后展示的控件（如操作按钮、互动入口等）允许声明 `@rendermode InteractiveServer`。这些控件必须被 `AuthorizeView` 包裹，对未登录用户不产生任何交互开销。
- 静态页中的其他局部交互根（如评论、评分、游玩记录、工具箱、看板娘等）也应独立声明交互模式，避免整页升级。

### 4.6 浏览器模块（MUST）

- `wwwroot/CnGalWebSite.MainSite.Shared.lib.module.js` 是唯一初始化入口，负责注册 IME 事件、`cg-image-gallery`、`cg-carousel`、`cg-edit-nav`、组件生命周期与 `site-shell`。
- 专用实现放在同名 `.razor.js`；自动挂载契约是 `mount(root, { signal }) => { update, dispose, suspend?, resume? }`，使用 Light DOM。
- 异步初始化必须检查连接代次与取消信号；`update` 幂等，`dispose` 幂等并清理监听、观察器、定时器、动画及对象 URL。
- 组件发现不得依赖全局扫描；局部内容变化使用 `observeContent` 等局部观察，并排除嵌套实例。`cg-owned` 只负责 DOM 移除时释放控制器。
- 初始化失败必须清理并显示可重试状态（`data-cg-module-error` / `data-cg-module-retry`），不得吞错。

### 4.7 编辑会话与草稿（MUST）

- 编辑会话按领域、按编辑根创建，不能作为整个 Circuit 的共享实例；页面切换由 `EditSessionHost<TSession>` 管理。
- 七种有草稿的领域会话组合 `EditDraftController<TDraft>`；收藏夹保持直接保存语义，不接入不存在的草稿格式。
- `DraftSaveLoop` 每十秒串行保存修改快照；保存、提交和丢弃共用互斥入口，提交前合并编辑器输入。
- 草稿读取失败时暂停自动保存；仅明确提交成功或用户丢弃时删除，失败或结果不确定时保留。
- 柱状图、折线图和饼图通过 `CanvasChartController` 统一创建、更新、重试和释放。

---

## 5. 样式与 Design System 规范

### 5.1 Token-first（MUST）

- 设计变量统一收敛到 `wwwroot/styles/design-tokens.css`，使用 `:root` 声明。
- 当前 Token 分类：
  - **颜色**：`--cg-color-bg`、`--cg-color-surface`、`--cg-color-border`、`--cg-color-text`、`--cg-color-text-muted`、`--cg-color-primary`、`--cg-color-secondary`、`--cg-color-error`、`--cg-color-link`、`--cg-color-on-primary`、`--cg-color-focus-ring`、`--cg-color-hover-overlay`、`--cg-color-primary-hover`、`--cg-color-secondary-hover`、`--cg-color-danger`（`var(--cg-color-error)` 别名）、`--cg-color-success`、`--cg-color-warning`、`--cg-color-text-primary`、`--cg-color-text-secondary`、`--cg-color-section-bg` 等。
  - **间距**：`--cg-spacing-1` 至 `--cg-spacing-6`、`--cg-spacing-8`、`--cg-spacing-10`。
  - **圆角**：`--cg-radius-sm` / `md` / `lg` / `xl` / `full`。
  - **字号**：`--cg-font-size-xs` 到 `--cg-font-size-2xl`。
  - **字重**：`--cg-font-weight-medium` / `semibold` / `bold`。
  - **阴影**：`--cg-shadow-sm` / `md` / `lg`、`--cg-shadow-card-hover`。
  - **过渡**：`--cg-transition-fast` / `normal`。
  - **字体**：`--cg-font-family`（Plus Jakarta Sans）。
  - **组件 Token**：`--cg-header-height`、`--cg-bottom-nav-height`。
  - **编辑器状态色**：`--cg-color-editor-warning-text`、`--cg-color-editor-error-text`、`--cg-color-editor-error-bg`、`--cg-color-editor-error-border`。
  - **覆盖层/暗底文字**：`--cg-color-overlay`、`--cg-color-on-dark`、`--cg-color-on-dark-secondary`、`--cg-color-on-dark-muted`。
  - **徽章/星标色**：`--cg-color-amber`、`--cg-color-emerald`、`--cg-color-emerald-subtle`。
  - **品牌色**：`--cg-color-brand-bilibili`、`--cg-color-brand-zhihu`、`--cg-color-brand-weibo`、`--cg-color-brand-purple`、`--cg-color-brand-teal`。
  - **分类标签色**：`--cg-color-badge-entry`/`-text`、`--cg-color-badge-article`/`-text`、`--cg-color-badge-tag`/`-text`、`--cg-color-badge-video`/`-text`、`--cg-color-badge-periphery`/`-text`。
- 组件样式与页面样式禁止硬编码重复色值；优先消费 `--cg-*` 变量。

### 5.2 样式隔离（MUST）

- 组件样式使用 Blazor CSS 隔离；有自定义样式的组件使用同名 `.razor.css`，无自定义 CSS 时无需创建。
- 跨组件样式穿透仅在必要时使用 `::deep`，并在 PR 说明原因。

### 5.3 命名规范（MUST）

- 基础组件类名前缀统一 `cg-`（如 `.cg-btn`、`.cg-modal`）。
- 页面块级样式按页面语义命名（如 `.home-hero`、`.entry-detail`），避免通用歧义名。

### 5.4 可访问性（MUST）

- 可交互元素必须使用语义标签（`button`、`a`、`input`）。
- 对话框等复杂组件必须包含基本 ARIA（如 `role="dialog"`、`aria-modal="true"`）。
- 链接按钮必须有可感知文本，禁用仅图标无可读描述的交互入口。

### 5.5 图标生成与使用规范（MUST）

- 主站图标统一使用 `Components/DesignSystem/CgMdiIcon.razor`，禁止在业务组件中直接内联 SVG 路径数据。
- 图标名称必须使用 `@mdi/js` 导出的标准符号名（如 `mdiAccount`、`mdiControllerClassicOutline`），禁止自定义别名。
- 图标映射文件 `CgMdiIconMap.g.cs` 属于自动生成文件，禁止手工编辑。
- 当新增、删除或重命名任何 `mdi*` 图标引用后，必须在仓库根目录执行 `python mdi_extract.py` 重新生成映射文件。
- `mdi_extract.py` 会扫描 `MainSite.Shared` 与 `MainSite` 中的 `.razor` / `.cs` 文件并按实际使用生成最小映射；若报 `non-icon or unresolved names`，必须修正对应名称后再提交。
- 组件样式中若需从父组件调整 `CgMdiIcon` 尺寸/颜色，需遵循 CSS 隔离规则，使用 `::deep .cg-mdi-icon` 进行穿透，避免样式失效。
- 图标默认应标记 `aria-hidden="true"`；若图标承担语义信息，必须提供等价可读文本（可见文本或 `aria-label`）。

### 5.6 响应式设计规范（MUST）

- 允许使用 `min-width` 和 `max-width` 两种断点方向，但必须统一使用以下断点阈值：
- 统一断点约定（定义在 `design-tokens.css` 注释中）：
  - `sm`：`640px`（大手机/手机横屏）
  - `md`：`768px`（平板竖屏）
  - `lg`：`1024px`（平板横屏/小笔记本）
  - `xl`：`1440px`（桌面）
- **禁止使用 C# 条件渲染区分设备类型**（如旧站 `MiniModeContainer`），必须使用 CSS `@media` 查询实现响应式适配。
- 页面级、组件级均需验证 **360px 最小宽度** 下的基本可用性。
- 可交互元素最小触控区域 **44×44px**（遵循 WCAG 2.5.5）。
- 布局网格、卡片列表等必须提供至少一个移动端断点的列数适配。
- 间距使用 `--cg-page-gutter`、`--cg-section-gap` 等响应式 Token，禁止在页面/组件中硬编码与断点相关的重复 padding 值。
- `MainLayout` 中 Footer 在移动端（`< 768px`）隐藏，底部导航栏（`CgBottomNav`）在所有主 tab 页面（HomePage、SearchPage、SquarePage、ShareGamesPage、SpaceIndexPage）的移动端显示。
- 新增页面 PR 必须附带移动端视窗截图（至少 375px 宽度）。

---

## 6. 命名、目录与代码风格

### 6.1 目录规范（MUST）

- `MainSite` 当前结构：
  - `Components/`（`App.razor`、`Routes.razor`、`Pages/`）
  - `Services/`（`PublicHtmlCachePolicy`、`IdleCircuitHandler`、`KanbanEventCircuitHandler`）
  - `Program.cs`、`appsettings.json`、`wwwroot/`
- `MainSite.Shared` 当前结构：
  - `Pages/{Domain}/`：`Admin`、`Article`、`Commodities`、`Entry`、`Examine`、`Expo`、`FavoriteFolder`、`Home`、`Lottery`、`Message`、`Perfections`、`Periphery`、`Space`、`Table`、`Tag`、`UserCenter`、`Video`。
  - `Components/DesignSystem/`：47 个基础组件及 `CgMdiIconMap.g.cs` 等辅助文件。
  - `Components/Features/{Domain}/`：按领域和 `Detail/`、`Editor/` 等子场景分组。
  - `Components/Layout/`：`UserMenu`、`CreateMenu`、`ThemeToggle`、`UserSessionTracker` 等。
  - `Layout/`：`MainLayout`、`AdminLayout`。
  - `Services/`：Toast、主题常量、MiniMode、设备识别、Circuit、页面响应状态、草稿与编辑会话、图表控制器、看板服务及 `ExternalLinkHelper` 等。
  - `Services/Editing/`：`EditSessionHost<T>`、`EditDraftController<T>` 及各领域 EditSession。
  - `wwwroot/`：设计 Token、`site-shell.js`、`component-lifecycle.js`、`dialog.js`、`edit-nav.js`、`canvas-controller.js` 等。
  - `Extensions/`：`AddMainSiteSharedServices`。
- `SDK.MainSite` 当前结构：
  - `Abstractions/`：查询、命令、页面聚合、身份与任务接口。
  - `Queries/`、`Commands/`：API 查询与写操作实现。
  - `Auth/MainSiteAuthRequestService.cs`。
  - `Models/`：页面模型、结果模型及 `ArticleEdit/`、`EntryEdit/`、`VideoEdit/`、`TagEdit/`、`PeripheryEdit/`、`SpaceEdit/`、`Files/`、`Comments/`、`Kanban/`、`Toolbox/` 等子目录。
  - `Infrastructure/`：`QueryServiceBase`、`CommandServiceBase`、`PublicDataCache`、`SdkRequestPolicy`、`SdkJsonSerializerOptions`、`SdkFailure`、`StaffBatchParser` 等。
  - `Services/Toolbox/`：工具箱业务编排与外站内容处理。
  - `Extensions/ServiceCollectionExtensions.cs`：`AddMainSiteSdkCore`。
- `SDK.MainSite.AspNet` 当前结构：
  - `Auth/`：OIDC、Cookie 刷新、令牌存储、`AccessTokenHandler`、`ICurrentUserAccessor` 实现及 `AuthenticatedSdkClients`。
  - `Extensions/`：`AddMainSiteOidcAuthentication`、`AddMainSiteSdk`、认证端点映射。
  - `Services/Toolbox/ToolboxLocalRepository.cs`。

### 6.2 命名规范（MUST）

- 页面组件：`{Domain}{Function}Page.razor`（如 `HomePage.razor`、`EntryDetailPage.razor`、`EntryEditPage.razor`）。
- 基础组件：`Cg{Component}.razor`（如 `CgButton.razor`、`CgEnumSelect.razor`）。
- 业务组件：`{Domain}{Feature}.razor`（如 `EntryHero.razor`、`HomeGameCard.razor`、`SpaceHero.razor`）。
- 业务组件按领域分组后可进一步按子场景分子目录，如 `Entry/Detail/EntryHero.razor`、`Entry/Editor/EntryMainEditor.razor`。
- Query Service：`{Domain}QueryService`，接口 `I{Domain}QueryService`。
- Command Service：`{Domain}CommandService`，接口 `I{Domain}CommandService`。
- Result 错误码：全大写蛇形，带领域前缀（如 `ENTRY_NOT_FOUND`、`HOME_ALL_REQUESTS_FAILED`）。

### 6.3 C# 编码风格（MUST）

- 开启空引用上下文（四个项目均已启用 `<Nullable>enable</Nullable>`），禁止关闭 `nullable`。
- 公共模型属性使用 `required init`（如 `SdkErrorModel` 中的 `Code`、`Message`）。
- 对外只读集合类型优先 `IReadOnlyList<T>`。
- 能表达"无结果"时返回空集合，不返回 `null` 集合。

---

## 7. 性能与稳定性规范

### 7.1 SSR 输出（MUST）

- 页面首屏必须可在无客户端交互脚本前提下展示核心内容。
- 避免在首屏输出中引入与当前页面无关的重型资源。

### 7.2 网络调用（MUST）

- 单页面多接口聚合由 SDK 统一编排；页面层禁止并发细节外泄。
- 同类聚合接口失败时采用"部分可用"策略（如 warnings），保证页面尽量可渲染。

### 7.3 异常降级（MUST）

- 页面必须提供失败态 UI（错误文案 + 可恢复动作）；`HomePage`、各类详情页和编辑页均已按此约定实现。
- 交互页面可使用 `ICgToastService` 进行轻量级操作反馈通知（成功/失败/警告/信息），服务通过 `AddMainSiteSharedServices()` 注册。
- 静态公共页面应通过 `PageResponseState` 上报结果，供 `PublicHtmlCachePolicy` 决定是否允许共享缓存。
- 不可恢复错误要有明确回退路径（返回首页、重试等）。
- Command 响应中的 4xx JSON 业务拒绝（`Successful=false` 且 `Error` 非空）应通过 `SdkResponse` / `SdkFailure` 保留业务提示和 HTTP 状态码；非约定格式及 5xx 响应继续显示通用 HTTP 错误，不直接回显响应正文。

### 7.4 缓存与身份（MUST）

- SDK 公共数据缓存只保存确认匿名、身份代次未变化的数据；`PublicDataCache.BeginQueryAsync` 每次查询捕获身份与失效代次。
- 写请求通过 `CommandServiceBase` 推进对应 API 来源的失效代次；旧代次结果不得回填。
- 缓存失效只影响缓存命中和回填，不把并发读取的正常快照或原始错误改为页面失败，也不因此重发请求；身份代次变化仍必须拒绝旧身份结果。
- HTML 是否可共享只由 `PublicHtmlCachePolicy` 决定；默认 `private, no-store`。当前实现要求页面通过 `PageResponseState` 标记成功、请求为 GET、响应 200、未认证、无 `Authorization` / `Set-Cookie`、无 MiniMode Cookie 或查询参数，且除主题 Cookie 外无其他 Cookie。
- 因此新增/改造静态公共页面时必须调用 `PageResponseState.Succeeded()` / `Failed(...)`；未上报结果的页面不会被共享缓存。

---

## 8. 测试与验收规范

### 8.1 最低测试要求（MUST）

- SDK Query/Command 服务：至少覆盖成功、HTTP 失败、反序列化失败三类用例。
- 页面组件：至少验证成功态、失败态两个渲染分支。
- 关键页面（首页、详情）需有基础回归测试。

### 8.2 PR 检查清单（MUST）

每个主站重构 PR 必须回答：

- 页面是否默认静态 SSR？若启用交互，必要性是什么？
- 是否新增或修改了 SDK 抽象接口？是否复用既有能力？
- 是否在 `MainSite.Shared` 出现了业务 API 直连？
- 是否新增硬编码样式值而未进入 Tokens？
- 是否覆盖了失败态与日志关键信息？
- 若改动涉及图标：是否仅使用 `CgMdiIcon`，并在图标引用变更后执行了 `python mdi_extract.py`？
- 静态公共页是否正确使用 `PageResponseState`，并符合 `PublicHtmlCachePolicy`？
- 若改动浏览器模块：是否遵守 `mount(root, { signal })` 生命周期、取消与失败重试契约？
