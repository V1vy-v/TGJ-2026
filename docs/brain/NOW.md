# NOW

更新日期：2026-10-24

## 同步基线

- 最后同步的 commit：`9437e70`（初始化项目），2026-10-24。
- 我每次涉及代码前都会跑：`git log --oneline <基线>..HEAD`、`git status --porcelain`、`git diff --stat`，再读实际文件；**以磁盘为准，不以记忆为准**。
- 每次同步完更新这一行。

## 已完成

- 确认技术栈：Unity 6000.3.25f1 + URP 17.0.1（仅 PC Renderer）+ Input System 1.20.1，目标平台 Windows；外部编辑器设为 Rider。
- 确认协作方式：默认给完整代码 + 挂载说明，我自己落盘；不直接改 `Assets`；提交由我执行。
- 写好 `AGENTS.md`，作为项目规则；加了可主动更新区/不可主动更新区的划分。
- 解决模板自带的 Input System 1.12.0 编译错误（`BuildTarget.ReservedCFE`）：升级到 1.20.1。
- 实时 CLI（`unity command`）全链路验证：状态、编译检查、控制台、测试、构建（dry_run + 真出包）都通过；包在 `Builds/Windows/tgj-2026.exe`（约 102 MB）。
- 首次提交并推送（`9437e70` 初始化项目）。

## 注意事项

- 实时 CLI 走 `unity command`（端口 7800，descriptor 在 `Library/Pipeline/.unity-pipeline-port`），编辑器开着也能用、沙盒内可跑；batchmode 是兜底，需要关编辑器 + Hub 且在沙盒外跑。
- 模板自带的 `Assets/InputSystem_Actions.inputactions` 与 6.3 的 3D 模板逐字节一致，是默认 project-wide actions。
- 仓库里还有未提交内容：`AGENTS.md`、`docs/`、`Assets/Scripts/`、`Assets/Scenes/MainScene.unity` 等 8 个未跟踪项，19 个 `AD` 旧暂存项，以及 3 个 URP 设置资源的本地改动。

## 下一步

- 清理并提交当前工作区（未跟踪文件 + 旧暂存项 + URP 设置改动）。
- 确定玩法细节（视角、核心机制、关卡规模），再定 `Assets/Scripts` 目录结构。
- 搭框架骨架：表现层 / 逻辑层 / 数据层的空壳 + 一个能跑通的最小循环。

## 未决问题

- 游戏名未定。
- 视角（第一人称 / 第三人称）未定。
- 迷宫是手工搭还是程序生成，未定。
- 具体解谜机制与机关类型未定。
