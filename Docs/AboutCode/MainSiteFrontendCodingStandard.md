# CnGal 主站前端重构编码规范（Blazor + SDK）

## 1. 文档定位

本规范用于指导以下项目的重构与后续增量开发：

- `CnGalWebSite.MainSite`（Host 启动层）
- `CnGalWebSite.MainSite.Shared`（页面/组件层）
- `CnGalWebSite.SDK.MainSite`（前后端通信与业务数据聚合层）

目标：在 `.NET 10` 下落地“**静态 SSR 优先 + 按页交互 + SDK 统一数据出口**”，并形成可评审、可检查、可维护的工程标准。

---

## 2. 架构总原则（强制）

### 2.1 分层职责（MUST）

- `MainSite` 只负责应用启动、中间件、渲染能力注册、静态资源映射，不承载业务页面逻辑。
- `MainSite.Shared` 只负责页面、布局、组件与纯 UI 状态，不直接发业务 HTTP 请求。
- `SDK.MainSite` 负责所有对 APIServer 的访问、聚合、容错、缓存、日志、错误标准化。
- 前端访问后端必须经过 SDK（禁止绕过 SDK 直连业务接口）。

### 2.2 渲染策略（MUST）

- 默认页面必须使用静态 SSR。
- 仅在“存在明确交互必要性”的页面根组件上声明 `@rendermode`。
- 禁止在 `App`、`Routes`、`Layout` 层启用全局交互渲染，避免全站被动交互化。

### 2.3 依赖方向（MUST）

- 允许依赖：`MainSite -> MainSite.Shared -> SDK.MainSite`。
- 禁止依赖：`SDK.MainSite -> MainSite.Shared`、`MainSite.Shared -> APIServer 具体实现`。

---

## 3. SDK 设计规范（重构核心）

## 3.1 接口分层（MUST）

- SDK 服务按语义拆分为：
  - `Queries/`：只读查询，不产生副作用。
  - `Commands/`：写操作（点赞、收藏、评论、资料更新等）。
- 交互页不得直接拼接 API 路径；路径细节只允许存在于 SDK 内部。

## 3.2 契约设计（MUST）

- 对外暴露接口位于 `Abstractions/`，命名采用 `I{Domain}{Query|Command}Service`。
- 返回模型使用面向页面的 `ViewModel`，避免在页面层出现 DataModel 细节。
- 所有 SDK 接口必须支持 `CancellationToken`。

## 3.3 返回结果模型（MUST）

- 统一使用 `SdkResult<T>` 风格的结果对象：
  - 成功：`Success=true` + `Data`
  - 失败：`Success=false` + 标准化 `Error(Code, Message, StatusCode)`
- 禁止把异常原样抛到页面层作为常规流程控制。
- 页面层只消费“可展示的错误语义”，不处理底层异常类型分发。

## 3.4 容错与日志（MUST）

- 每个外部调用必须覆盖三类失败：HTTP 非成功、反序列化失败、运行时异常。
- 日志必须包含：`Path`、`StatusCode`、`BaseAddress`、业务 `ErrorCode`。
- 响应体日志必须截断（参考 `TrimForLog`），禁止无上限输出。

## 3.5 序列化与兼容（MUST）

- 全 SDK 统一 `JsonSerializerOptions`，禁止服务内各自 new 不同配置。
- 必须开启对 APIServer 枚举/大小写差异的兼容策略（如字符串枚举转换、大小写不敏感）。

## 3.6 缓存策略（MUST）

- 查询服务可使用缓存，缓存键必须可读且包含域语义（如 `main-site:entry-detail:{id}`）。
- 缓存时长按场景明确配置：
  - 高频首页聚合：短缓存（分钟级）
  - 详情页：中缓存（数分钟）
- 缓存命中是优化，不得改变业务正确性；失效后必须可回源。

## 3.7 模型映射（SHOULD）

- SDK 内部完成 DTO/DataModel -> ViewModel 映射，页面不做二次业务组装。
- 进行字段兜底（空字符串、默认链接、空集合）保证页面渲染稳定。

---

## 4. Blazor 页面与组件拆分规范

## 4.1 页面职责（MUST）

- `Pages/*.razor` 只负责：
  - 路由声明
  - 页面级数据装配（调用 SDK）
  - 页面骨架与区域编排
- 页面中禁止出现复杂可复用 UI 结构（抽取到组件层）。

## 4.2 组件分层（MUST）

- `DesignSystem/Components`：基础组件（Button/Input/Modal/Pagination...），不包含业务术语。
- `Features/{Domain}/Components`：业务组件（如词条卡片、文章摘要、评分区）。
- `Layout/`：全局壳与导航，不承载业务查询逻辑。

## 4.3 组件 API 设计（MUST）

- 参数命名语义化：`Variant`、`Disabled`、`CurrentPage`、`OnClose`。
- 事件统一使用 `EventCallback` / `EventCallback<T>`。
- 可双向绑定组件必须遵循 `Value` + `ValueChanged` 模式。
- 不可变输入优先，避免子组件修改父状态。

## 4.4 生命周期规范（MUST）

- 首次加载优先 `OnInitializedAsync`。
- 依赖路由参数变化的加载逻辑使用 `OnParametersSetAsync`。
- 所有异步调用都要传递 `CancellationToken`（页面可在后续引入取消源统一管理）。

## 4.5 交互边界（MUST）

- 静态页禁止引入仅交互页需要的脚本与状态逻辑。
- 如果页面只需局部交互，优先局部交互组件而非整页交互化。

---

## 5. 样式与 Design System 规范

## 5.1 Token-first（MUST）

- 颜色、圆角、阴影、间距等设计变量统一收敛到 `design-tokens.css`。
- 组件样式与页面样式禁止硬编码重复色值；优先消费 `--cg-*` 变量。

## 5.2 样式隔离（MUST）

- 每个 `.razor` 组件使用同名 `.razor.css`，避免样式污染。
- 跨组件样式穿透仅在必要时使用 `::deep`，并在 PR 说明原因。

## 5.3 命名规范（MUST）

- 组件类名前缀统一 `cg-`（如 `.cg-btn`、`.cg-modal`）。
- 页面块级样式按页面语义命名（如 `.home-hero`、`.entry-detail`），避免通用歧义名。

## 5.4 可访问性（MUST）

- 可交互元素必须使用语义标签（`button`、`a`、`input`）。
- 对话框等复杂组件必须包含基本 ARIA（如 `role="dialog"`、`aria-modal="true"`）。
- 链接按钮必须有可感知文本，禁用仅图标无可读描述的交互入口。

## 5.5 图标生成与使用规范（MUST）

- 主站图标统一使用 `DesignSystem/Components/CgMdiIcon.razor`，禁止在业务组件中直接内联 SVG 路径数据。
- 图标名称必须使用 `@mdi/js` 导出的标准符号名（如 `mdiAccount`、`mdiControllerClassicOutline`），禁止自定义别名。
- 图标映射文件 `CgMdiIconMap.g.cs` 属于自动生成文件，禁止手工编辑。
- 当新增、删除或重命名任何 `mdi*` 图标引用后，必须在仓库根目录执行 `python mdi_extract.py` 重新生成映射文件。
- `mdi_extract.py` 会扫描 `MainSite.Shared` 与 `MainSite` 中的 `.razor` 文件并按实际使用生成最小映射；若输出 `skipped non-icon or unresolved names`，必须修正对应名称后再提交。
- 组件样式中若需从父组件调整 `CgMdiIcon` 尺寸/颜色，需遵循 CSS 隔离规则，使用 `::deep .cg-mdi-icon` 进行穿透，避免样式失效。
- 图标默认应标记 `aria-hidden="true"`；若图标承担语义信息，必须提供等价可读文本（可见文本或 `aria-label`）。

---

## 6. 命名、目录与代码风格

## 6.1 目录规范（MUST）

- `MainSite.Shared` 推荐结构：
  - `Pages/`
  - `Layout/`
  - `DesignSystem/Components/`
  - `Features/{Domain}/Components/`
  - `Features/{Domain}/Models/`（仅前端展示模型）
- `SDK.MainSite` 推荐结构：
  - `Abstractions/`
  - `Queries/`
  - `Commands/`
  - `Models/`
  - `Infrastructure/`
  - `Extensions/`

## 6.2 命名规范（MUST）

- 页面组件：`{Domain}Page.razor`（如 `HomePage.razor`）。
- 基础组件：`Cg{Component}.razor`（如 `CgButton.razor`）。
- Query Service：`{Domain}QueryService`，接口 `I{Domain}QueryService`。
- Result 错误码：全大写蛇形，带领域前缀（如 `ENTRY_NOT_FOUND`、`HOME_ALL_REQUESTS_FAILED`）。

## 6.3 C# 编码风格（MUST）

- 开启空引用上下文（当前项目已启用），禁止关闭 `nullable`。
- 公共模型属性使用 `required init`（如当前 SDK ViewModel）。
- 对外只读集合类型优先 `IReadOnlyList<T>`。
- 能表达“无结果”时返回空集合，不返回 `null` 集合。

---

## 7. 性能与稳定性规范

## 7.1 SSR 输出（MUST）

- 页面首屏必须可在无客户端交互脚本前提下展示核心内容。
- 避免在首屏输出中引入与当前页面无关的重型资源。

## 7.2 网络调用（MUST）

- 单页面多接口聚合由 SDK 统一编排；页面层禁止并发细节外泄。
- 同类聚合接口失败时采用“部分可用”策略（如 warnings），保证页面尽量可渲染。

## 7.3 异常降级（MUST）

- 页面必须提供失败态 UI（错误文案 + 可恢复动作）。
- 不可恢复错误要有明确回退路径（返回首页、重试等）。

---

## 8. 测试与验收规范

## 8.1 最低测试要求（MUST）

- SDK Query/Command 服务：至少覆盖成功、HTTP 失败、反序列化失败三类用例。
- 页面组件：至少验证成功态、失败态两个渲染分支。
- 关键页面（首页、详情）需有基础回归测试。

## 8.2 PR 检查清单（MUST）

每个主站重构 PR 必须回答：

- 页面是否默认静态 SSR？若启用交互，必要性是什么？
- 是否新增或修改了 SDK 抽象接口？是否复用既有能力？
- 是否在 `MainSite.Shared` 出现了业务 API 直连？
- 是否新增硬编码样式值而未进入 Tokens？
- 是否覆盖了失败态与日志关键信息？
- 若改动涉及图标：是否仅使用 `CgMdiIcon`，并在图标引用变更后执行了 `python mdi_extract.py`？

