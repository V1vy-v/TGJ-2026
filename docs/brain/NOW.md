# NOW

更新日期：2026-10-26

## 同步基线

- 最后同步的 commit：`abc9707`（封装UI模块），2026-10-26。
- 我每次涉及代码前都会跑：`git log --oneline <基线>..HEAD`、`git status --porcelain`、`git diff --stat`，再读实际文件；**以磁盘为准，不以记忆为准**。
- 每次同步完更新这一行。

## 已完成

- 确认技术栈：Unity 6000.3.25f1 + URP 17.0.1（仅 PC Renderer）+ Input System 1.20.1，目标平台 Windows；外部编辑器设为 Rider。
- 确认协作方式：默认给完整代码 + 挂载说明，我自己落盘；不直接改 `Assets`；提交由我执行。
- 写好 `AGENTS.md`，作为项目规则；加了可主动更新区/不可主动更新区的划分。
- 解决模板自带的 Input System 1.12.0 编译错误（`BuildTarget.ReservedCFE`）：升级到 1.20.1。
- 实时 CLI（`unity command`）全链路验证：状态、编译检查、控制台、测试、构建（dry_run + 真出包）都通过；包在 `Builds/Windows/tgj-2026.exe`（约 102 MB）。
- 首次提交并推送（`9437e70` 初始化项目）。
- UI 框架落地：四层 Canvas（Screen/Panel/Popup/Overlay）、UIRegistry（按类型查表）、UIManager（栈/缓存/Blocker/onClosed/事件）、UIBase（OnCreate / Setup / OnShow / OnHide / OnDestroyed）。
- UI 命名约定：按层加后缀（`MainMenuScreen`、`PausePopup`、`HUDPanel`、`LoadingOverlay`），预制体、类名、注册分组保持一致。
- 资源服务落地：`IAssetService` / `AssetManager`（缓存 + 引用计数 + 句柄）/ `IAssetProvider` / `YooAssetProvider`（YooAsset 3.0.6，编辑器模拟模式 + 离线模式）；`AssetKeys` 常量收口地址；Collector 按 `分组名_文件名` 生成地址，不建映射表。
- YooAsset 3.0.6 初始化链路跑通：`EditorSimulateBuildInvoker.Build` → `InitializePackageAsync` → `RequestPackageVersionAsync` → `LoadPackageManifestAsync`。

## 注意事项

- 实时 CLI 走 `unity command`（端口 7800，descriptor 在 `Library/Pipeline/.unity-pipeline-port`），编辑器开着也能用、沙盒内可跑；batchmode 是兜底，需要关编辑器 + Hub 且在沙盒外跑。
- 模板自带的 `Assets/InputSystem_Actions.inputactions` 与 6.3 的 3D 模板逐字节一致，是默认 project-wide actions。
- 仓库里还有未提交内容（2026-10-26 同步时）：19 个 `AD` 旧暂存项（文件已不在磁盘）；`Packages/manifest.json` 新增 YooAsset 3.0.6（git URL）；`Assets/Scripts/Framework/Services/Assets/` 整套资产服务；`Assets/Scripts/Gameplay/UI/`（MainMenuScreen、LoadingOverlay）与对应预制体；`Assets/Art/cute-apple-free/`（apple.fbx + 两张贴图）；`Assets/Configs/BundleCollectorSetting.asset`；`InputSystem_Actions.inputactions` 有大幅精简；`UIBase`/`UIManager`/`IUIService`/`UIRegistry.asset`/`UIRoot.prefab`/`MainScene.unity`/`test01.cs` 有本地改动。
- `InputSystem_Actions.inputactions` 从 1057 行降到约 726 行（工作区未提交），同步时未确认删掉了哪些 Action Map。
- YooAsset 编辑器模拟模式每次进 Play 都会跑一次模拟构建（`Bundles/<平台>/<包名>/Simulate/`），启动会略慢；出包走离线模式，不跑模拟构建。

## 下一步（待办）

- 主菜单实现：`MainMenuScreen`（开始/设置/退出/版本号）+ `SettingsPopup`（音量/灵敏度）+ 确认退出弹窗。
- **项目结束后**重构 UI 框架：MVC 分层、异步加载、生命周期/路由、加载界面与预加载、缓存与释放策略；本周期内不再改 UI 架构，保持同步直接引用。
- 后期：为 UI 增加 `UIScreen` / `UIPanel` / `UIPopup` / `UIOverlay` 基类，把 `OpenXxx<T>` 的约束从运行时 `CheckLayer` 变成编译期检查；需要同步调整泛型约束与注册表校验。
- 资源服务后续：音频/关卡接入 `IAssetService`；补启动时 `InitializeAsync` 与可选预加载；热更/远程模式留到后期。
- Common 目录（公用基类）：泛型 MonoBehaviour 单例基类、泛型 ScriptableObject 单例基类、可复用 FSM 状态机基类。
- 音频系统（重点）：AudioMixer 四总线 + AudioManager + 3D 音源池 + 音乐/环境淡入淡出。
- 清理并提交当前工作区（未删除的旧暂存项 + 未提交文件）。
- 确定玩法细节（视角、核心机制、关卡规模），再定 `Assets/Scripts/Gameplay` 目录结构。

## 未决问题

- 游戏名未定。
- 视角（第一人称 / 第三人称）未定。
- 迷宫是手工搭还是程序生成，未定。
- 具体解谜机制与机关类型未定。
