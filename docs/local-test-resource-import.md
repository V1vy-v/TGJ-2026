# 本地测试工程资源导入

日期：2026-10-28

本次仅从“测试”工程导入新增资源，目录为 `Assets/Art/ImportedLocalTest/`：

- `Models/`：3 个 FBX 模型。
- `Materials/`：7 个材质。
- `Textures/`：4 张树木贴图。
- `Prefabs/`：Tree 预制体，包含其内嵌网格和材质。
- `Terrain/`：1 个 Terrain 数据资源。

保留复制资源原有的 `.meta` 和 GUID，不改写资源内容。本次只新增文件；原有场景、脚本、资源、Packages、ProjectSettings 均不修改。本地未保存的场景状态没有被保存或上传。

`测试白膜 02.fbx` 的 GUID 已存在于远程 `Assets/Art/Models/`，因此跳过本地副本，以远程资源为准。新资源与原资源的 GUID 无重复；材质引用的 URP Shader 与远程现有材质一致。Tree 的四张外部贴图均在本次导入范围内。

本次通过 Git 原生推送，FBX 和 PNG 沿用仓库现有的 Git LFS 规则。原有 `.gitattributes` 和资源的 LFS 规则不变。

## 默认以远程解决合并冲突

隔离同步副本已设置以下仓库本地配置。其他克隆需要各自设置一次；Git 不会随提交分发 `.git/config`。

```powershell
git config pull.rebase false
git config pull.ff false
git config branch.main.mergeOptions "-X theirs"
```

拉取前运行 `git status --porcelain`，若有未提交改动则先停止，不自动覆盖或暂存它们。干净工作区中使用：

```powershell
git pull --no-rebase -X theirs origin main
```

此策略在常规内容冲突时优先远程；本地不冲突的改动仍会保留。对于 Git 不能自动处理的结构冲突，停止并检查，不强制推送、不自动删除文件。

## 验证边界

已检查新增路径、资源与 meta 成对、GUID、引用以及源资源字节哈希。未启动远程工程进行 Unity 导入或运行测试；Tree 使用 Unity 内置 Tree Shader，在远程 URP 中的实际显示效果仍需在编辑器检查。本次不改包或材质以修复显示差异。
