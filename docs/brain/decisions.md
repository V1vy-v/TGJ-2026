# 决策记录

格式：日期、决策、原因、考虑过的替代方案。

## 2026-10-23

- 渲染管线：URP，只保留 PC Renderer（`PC_RPAsset` / `PC_Renderer`），删除 Mobile 那套。
  - 原因：项目已用 URP；Jam 只做 Windows，维护两套 Renderer 是额外成本。
  - 替代方案：HDRP（太重）、内置管线（要重配所有设置）。
- 输入：Input System 1.12.0，只启用新输入系统。
  - 原因：项目设置已是 `activeInputHandler = 1`；Action 方案方便键鼠/手柄扩展。
  - 替代方案：新旧同时启用（两套并存易混）、旧 Input Manager（需改设置）。
- 目标平台：Windows StandaloneWindows64。
  - 原因：本地调试和出包最省事，比赛平台接受 Windows 包。
  - 替代方案：WebGL（构建慢、性能受限）；两者都要（成本高）。
- 脚本组织：单 `Assembly-CSharp`，命名空间根 `TGJ`，靠文件夹 + 命名空间分层，不建 asmdef。
  - 原因：Jam 规模下 asmdef 是额外维护成本；分层靠约定和代码审查保证。
  - 替代方案：asmdef 拆 Runtime/Editor/Tests（编译隔离更好，但前期负担大）。
- 目录结构：暂不定具体目录树，只写分层原则。
  - 原因：玩法未定，过早固定目录容易返工。
  - 替代方案：现在就按功能分目录（等玩法确定后再补）。
- 代码交付：默认给完整代码方案，不直接改项目文件。
  - 原因：我在学习阶段，需要自己写代码；避免我跳过实现细节。
  - 替代方案：直接代改（快，但失去练习）、按情况混合。
- Unity CLI：用 `Unity.exe -batchmode` 做编译/测试/构建，但低频使用，且必须先关编辑器。
  - 原因：同一项目不能双开；CLI 适合必须的验证，不适合日常迭代。
  - 替代方案：复制项目到临时目录跑（同步麻烦）、完全不用 CLI（无法自动验证构建）。
- 提交：由我执行，每个可运行小步提交一次。
  - 原因：保持回滚粒度，同时避免 agent 误操作版本库。
  - 替代方案：agent 直接提交（风险高）。
- 文档记忆：只保留 `docs/brain/NOW.md` 和 `decisions.md`。
  - 原因：Jam 项目文档从简，够用即可。
  - 替代方案：完整三件套（含架构/术语表，前期收益低）。
- Input System 升级：1.12.0 → 1.20.1。
  - 原因：模板自带的 1.12.0 在 6000.3 编译失败（`BuildTarget.ReservedCFE` 已移除，CS0117）；1.20.x 是 6.3 的对应版本。
  - 替代方案：回退编辑器版本（不现实）、改包源码（不可维护）。
- Unity CLI 前置条件：执行前关闭编辑器和 Unity Hub，并在沙盒外运行。
  - 原因：实测沙盒内启动即退出（127）；Hub 在跑时 batchmode 在引擎初始化前退出（1）；两者都关后编译检查通过（退出码 0）。
  - 替代方案：只用编辑器验证（无法自动化构建检查）。
- CLI 使用方式更正：优先 `unity command`（Pipeline 实时模式），batchmode 只作兜底。
  - 原因：项目装了 com.unity.pipeline 0.9.1-exp.1 和 `unity` CLI；实测编辑器开着时 `editor status`、`console status`、`script recompile`、`build get_settings` 可用，沙盒内可跑、不用关编辑器。
  - 替代方案：只用 batchmode（每步都要关编辑器+Hub）；dotnet build（只查 C# 编译）。
- 命名空间不带 `TGJ` 根：改成 `Framework.Core`、`Framework.Bootstrap`、`Framework.Services.Input/Audio/SceneFlow/UI`。
  - 原因：所有代码都在 `Framework/` 目录下，层级本身已经区分清楚，多余根只增加输入量；项目无同名冲突。
  - 替代方案：保留 `TGJ` 根（便于将来抽成 UPM 包时防冲突）。
- 框架目录结构：`Assets/Scripts/Framework/{Core,Bootstrap,Services/{Input,Audio,SceneFlow,UI}}`，每个叶子目录一个命名空间，Framework 根目录不放 .cs。
  - 原因：一个根下整包复制即可复用；Core 纯 C#、Bootstrap 是装配点、Services 是可替换实现。
  - 替代方案：Core 与 Services 平级放在 Assets/Scripts 下（复用时要挑文件夹）。
