# AGENTS.md：tgj-2026（3D 迷宫解谜冒险）

## 本文件的维护分区

- **不可主动更新区**（只有我能改，你只能指出不一致，不能自行修改）：项目概览、目标、技术栈、如何与我协作、始终、绝不、构建与提交、安全规则、记忆与备份、完成定义、本维护分区。
- **可主动更新区**（你可以追加）：`## 实测记录`。只能追加或修正事实，必须带日期和证据，不改任何规则的含义。
- 实测记录只写观察到的事实，不写推测；推测放在 `docs/brain/NOW.md` 或对话里。
- 任何不在实测记录里的改动，先问我；我没回应就保持原样。
- 你改这个文件时，必须在回复里说明改了哪一条、依据是什么。
- 本文件不是 API 依据；API 行为以 Unity 6000.3 文档和实测为准。

## 项目概览

- 名称：待定。工程名 `tgj-2026`（`productName` 暂同）；游戏名确定后同步改 Player Settings 和本文档。
- 类型：3D 迷宫解谜冒险，Game Jam 项目。视角、具体机制、关卡规模还没最终定，允许调整。
- 它是什么：一个面向 Jam 评审的小体量 3D 迷宫解谜游戏。玩家在迷宫里探索、解开机关、找到出口；目标是在 Jam 时间内交付一个几十分钟内能完整通关的版本。
- 当前状态：Unity 模板空项目（只有 `SampleScene` 和 URP 设置），还没有玩法代码；现在优先解决框架问题。

## 目标

1. 当前：解决框架问题——按下面的分层原则把脚本骨架立起来，让玩法确定后能直接往里填，而不是边写边返工。
2. Jam 截止前：做出能从头玩到通关的 Windows 版本，能构建出包并上传比赛平台。
3. 始终保持可运行：每个小阶段结束时代码能编译、场景能进入。

## 技术栈

- Unity 6000.3.25f1（Unity 6.3 LTS），C#
- 渲染：URP 17.0.1，只用 PC Renderer（`Assets/Settings/PC_RPAsset` + `PC_Renderer`）；当前质量等级为 PC（索引 1），Mobile 那套不再使用
- 输入：Input System 1.20.1（`activeInputHandler = 1`，只启用新输入系统；不要用 `UnityEngine.Input`）。注意：模板自带的 1.12.0 与 6000.3 不兼容（`BuildTarget.ReservedCFE` 已被移除），已升到 1.20.1
- UI：uGUI 2.0.0（TextMeshPro 随包）
- 相机：Cinemachine 3.1.7（`CinemachineCamera` + `CinemachineBrain`，不是 v2 的 Virtual Camera）
- 其他已装：Test Framework 1.4.2（写测试用）；已移除 AI Navigation、Timeline、Visual Scripting、Collab Proxy，需要时再加
- 脚本：单一 `Assembly-CSharp`，命名空间按目录分段、不带 `TGJ` 根（`Framework.Core`、`Framework.Services.Input`、`Gameplay`…），不建 asmdef
- 目标平台：Windows，StandaloneWindows64
- 编辑器路径：`D:\Programdata\UnityHub\Editor\6000.3.25f1\Editor\Unity.exe`
- 外部代码编辑器：Rider 2026.2.3.1（`D:\Programdata\JetBrains Rider 2026.2.3.1\bin\rider64.exe`），已在 Unity 的 Preferences > External Tools 中设为默认；代码补全、调试走 Rider 的 Unity 集成
- 包版本以 `Packages/manifest.json` 为准。版本或包与本文档冲突时，说明冲突并停下，不要擅自升级、降级或切换渲染管线。

## 命令

图形界面：

- 打开项目：Unity Hub > Add > 选择 `D:\Study\ComputerScience\Projects\Unity\Projects\tgj-2026`
- 运行游戏：在编辑器中按 Play
- 测试：Window > General > Test Runner（EditMode / PlayMode）
- 构建：File > Build Profiles（Unity 6）> Windows > Build，输出到 `Builds\Windows\`

命令行（实时模式：编辑器开着也能用，不用关 Hub，也不用在沙盒外跑；在项目根目录执行，`unity` 即 `%LOCALAPPDATA%\Unity\bin\unity.exe`）：

```powershell
# 编译检查
unity command script recompile
unity command script recompile_status        # completed / up_to_date / failed
unity command console status                 # 看 compilationFailed
unity command console get --level error --tail 20

# 测试（目前 0 个测试）
unity command test list
unity command test run --mode editor         # PlayMode 用 --mode playmode
unity command test status                    # 异步时轮询

# 构建
unity command build get_settings
unity command build run --target StandaloneWindows64 --outputPath "Builds\Windows\tgj-2026.exe" --dry_run    # 只校验
unity command build run --target StandaloneWindows64 --outputPath "Builds\Windows\tgj-2026.exe" --confirm   # 真出包
unity command build status                   # 轮询到 completed
```

- 实时模式的连接信息在 `Library/Pipeline/.unity-pipeline-port`（端口 7800），里面有 token；不要把 token 写进文档、日志或提交。
- 兜底方式（编辑器打不开、或 CI）：`Unity.exe -batchmode ...`，必须先关闭编辑器和 Unity Hub，并在 DSH 沙盒外运行。
- 命令失败时先读 `console get` 和日志定位，不要把没验证过的修法当成结论给我。

## 实测记录

- 2026-10-23：编辑器或 Unity Hub 在跑时，batchmode 会在引擎初始化前退出（退出码 1，日志停在 `Successfully changed project path`）；两者都关时编译检查通过（退出码 0，日志末尾 `Exiting batchmode successfully now!`）。
- 2026-10-23：`Start-Process -Wait` 等 Unity 退出会挂住（Unity 进程已退出，包装进程不返回）；判断结束看日志末尾的 `Exiting batchmode` / `Exiting without the bug reporter`，不要只等进程。
- 2026-10-23：Unity 在 DSH 沙盒内启动即退出（退出码 127，且不写日志），必须在沙盒外运行。
- 2026-10-23：项目装有 `com.unity.pipeline` 0.9.1-exp.1 和 `unity` CLI（`%LOCALAPPDATA%\Unity\bin\unity.exe`），编辑器开着也能实时驱动：`unity command ...` 通过 `Library/Pipeline/.unity-pipeline-port`（端口 7800）连接，**不需要关闭编辑器，也不需要沙盒外运行**。
- 2026-10-23：已验证的实时命令：`editor status`（ready）、`console status`（compilationFailed=false）、`script recompile`（up_to_date）、`build get_settings`（StandaloneWindows64，MainScene 已启用）。
- 2026-10-23：`test list` / `test run --mode editor` 可用（当前 0 个测试，success=true）；`build run --dry_run` 校验通过；`build run --confirm` 在编辑器开着的情况下出包成功（约 57 秒、102 MB、0 warning / 0 error，输出 `Builds\Windows\tgj-2026.exe`），`build status` 轮询到 `completed`。

## 如何与我协作

- 你是我的开发助手，默认用引导式协作：先给约束、决策点和验收标准，我自己写代码；只有我卡住或明确要代码时，你才给最小可用的片段或完整实现，并说明给了什么、为什么给。
- 我负责建文件、挂组件、运行验证；只有我说「你来改」时，你才直接改 `Assets` 里的文件。
- 你给我的每段代码（或我写完的代码）都要能对应到：文件放哪、挂到哪个 GameObject、Inspector 填什么、怎么验证生效。
- 重大改动（新增系统、调整场景结构、改数据结构）前，先用要点列出计划和影响，等我确认。
- 我在学习阶段：解释为什么这样分层、为什么用这个 API；指出可替换的地方，也指出你省略了什么。不要堆用不上的抽象。
- 报错时不允许给我不保证正确的解决步骤。先复现、读日志、查 6000.3 文档；能确认的说明依据，不能确认的直说不确定，给出排查方案，和我一起定位。
- 提交动作由我执行：每个可运行小步完成后，你提醒我该提交，列出改了哪些文件并给出建议的 commit message；不要自己 commit 或 push。
- 对 API、版本、文件内容不确定时，说明并检查，不要猜。
- 同步约定：每次回答涉及代码前，先读磁盘（`git status`、`git diff`、直接读文件），不凭记忆假设文件内容；发现我没见过的改动，先说明「我看到的是哪个版本」再回答。
- 我维护 `docs/brain/NOW.md` 的「同步基线」（最后同步的 commit）；回答代码问题前先跑 `git log --oneline <基线>..HEAD`、`git status --porcelain`、`git diff --stat`，找出我错过的改动。
- 我的说法与磁盘/提交不一致时，以磁盘为准；不确定就先问，不猜。

## 始终

- 使用 Unity 6000.3.25f1 对应的 API，不写更旧或更新版本的用法。
- Inspector 值用 `[SerializeField] private` 字段，不用 public 字段。
- 命名：非序列化私有字段 `_camelCase`；序列化私有字段（`[SerializeField]`）`camelCase`；公开成员（类型、属性、方法、事件）PascalCase；参数 camelCase；不用无意义缩写。
- 代码分区：用 `#region` 分四块，标题用中文——`状态`、`生命周期`、`对外接口`、`内部实现`；不嵌套 region。
- 在 `Awake` / `Start` 中缓存组件引用，不要每帧 `GetComponent`。
- 输入走 Input System（InputAction / PlayerInput / 生成的 C# 包装类）。
- 代码放 `Assets/Scripts/<层>/`，命名空间按目录（`Framework.Core`、`Framework.Services.Audio`、`Gameplay...`），不加 `TGJ` 根。
- 分层与依赖方向（具体目录树等玩法确定后再定）：
  - 表现层：MonoBehaviour——输入读取、相机、动画、UI 绑定、音效触发，只负责把引擎事件转成调用。
  - 逻辑层：纯 C# 规则——迷宫状态、机关判定、解谜条件、流程推进；不继承 MonoBehaviour，方便脱离场景测试。
  - 数据层：ScriptableObject 或可序列化数据——关卡、道具、机关参数等配置。
  - 依赖只能单向：表现 → 逻辑 → 数据。逻辑层不认识场景对象，靠接口/事件向上通知。
  - 层与层之间用接口或事件解耦，不直接引用对方的具体实现类。
- 纯逻辑代码要能脱离场景测试（Test Framework）。
- 平台相关路径和设置集中在一处，不要在各处散落 `Application.dataPath`。
- 交付代码时说明怎么验证：进哪个场景、按什么键、应该看到什么。

## 绝不

- 除非我明确要求，不手动编辑 `.unity` 场景或 `.prefab` 文件；给我编辑器操作步骤。
- 不在编辑器外移动、重命名或删除资源与 `.meta` 文件（会断引用）；需要时告诉我编辑器里的操作。
- 不擅自改 ProjectSettings（渲染管线、输入、包版本、质量设置、Player Settings）；要改先说明影响。
- 不添加许可证不明的 Asset Store 包或插件；Jam 期间默认不加新依赖。
- 不执行 git 提交或推送（由我操作），不做 `git reset --hard`、强制推送等破坏性操作。

## 构建与提交

- 构建目标：Windows StandaloneWindows64，输出 `Builds\Windows\`（`Builds/` 已加入 `.gitignore`，构建产物不进仓库）。
- 上传比赛平台由我操作；你只保证能出包、包能运行，不上传、不填表。
- 出包前检查：无编译错误、Console 无新增 warning、能从开头玩到通关、窗口与分辨率正常、退出正常。
- 提交由我执行；每完成一个可运行小步提交一次，你负责提醒并给出建议信息。

## 安全规则

- 绝不要把机密（API 密钥、令牌、密码）写进代码、文档、日志或提交；用环境变量，`.env` 留在 `.gitignore` 里。
- 只分发 `Builds\` 下的构建产物；绝不上传或暴露项目根目录、`.env`、源数据、配置文件。
- 运行破坏性命令（删除文件、`git reset --hard`、强制推送、删除数据）前先问我。
- 添加新依赖、服务或付费 API 前先问我。

## 记忆与备份

- `docs/brain/NOW.md`：会话开始时先读；每次会话结束时更新——已完成什么、下一步是什么、未决问题。
- `docs/brain/decisions.md`：重要决策记成「日期、决策、原因、考虑过的替代方案」；玩法、架构、技术选型都记。
- 同一个错误被我纠正两次，就提议往本文件加一条新规则。
- git 提交由我执行；你只负责提醒和给信息。

## 完成定义

- 能编译、能运行，没有错误，也没有新增警告。
- 只做被要求的内容，不多做。
- 行为或规则发生变化时，`docs/brain` 和本文件已同步更新。
