# AI 使用记录

## P-001

- 目标：创建项目 Skills 规范文档

### Prompt 原文

> 帮我创建 4 个自定义 Skill：ar-patrol-schema（数据契约与 API 规范）、unity-ar-dev（Unity AR 开发规范）、go-api-dev（Go 后端规范）、react-admin-dev（React 管理端规范）

### 结果

- 采用
- 4 个 Skill 文件已创建在 `.claude/skills/` 目录下，后续开发中按 Skill 规范实现

## P-002

- 目标：安装 Go 开发环境

### Prompt 原文

> 帮我安装 Go

### 结果

- 采用
- 通过 winget 安装 Go 1.27.0，设置 GOPROXY=https://goproxy.cn,direct 解决国内网络问题

## P-003

- 目标：创建三端项目骨架

### Prompt 原文

> 先创建出项目骨架吧

### 结果

- 采用
- Go 后端：model + handler + store + middleware + main，编译通过
- React 管理端：Vite + TypeScript + 5 组件 + API client + Vite 代理，构建通过
- Unity AR：4 个 C# 脚本占位，等 Unity Editor 安装后集成
- 修改了 Go model/issue.go 缺失的 fmt import 编译错误

## P-004

- 目标：修复 Go 后端编译错误

### Prompt 原文

> (自动发现) model/issue.go 编译报 undefined: fmt

### 结果

- 修改后采用
- 添加 `"fmt"` 到 import 列表，编译通过

## P-005

- 目标：修复 IssueReporter.cs 使用老版 UI 组件导致 Unity 编译失败

### Prompt 原文

> ARPlacementManager 没有搜索到这个（Add Component 中找不到脚本）

### 结果

- 修改后采用
- 原因：IssueReporter.cs 使用 `InputField` / `Dropdown` / `Text`（老版 UI），但项目中使用 TextMeshPro 版本
- 修复：`InputField` → `TMP_InputField`，`Dropdown` → `TMP_Dropdown`，`Text` → `TextMeshProUGUI`，`using UnityEngine.UI` → `using TMPro`
- 同时移除 emoji 字符避免 Unity 兼容性问题

## P-006

- 目标：指导 Unity 场景搭建和脚本挂载

### Prompt 原文

> 第七步详细跟我描述一下 / ARMarker 预制体在哪里 / 没有找到 Placeholder 子对象

### 结果

- 采用
- 提供了详细的分步操作指南
- 发现 TMP Input Field 子对象结构为 `TitleInput → Text Area → Placeholder/Text`（与老版不同）
- 指导完成场景搭建、预制体创建、UI 面板、脚本挂载、引用绑定、按钮事件绑定

## P-007

- 目标：Go + React 端到端联调测试

### Prompt 原文

> (主动执行) 启动 Go 后端 → 创建 3 条问题 → 列表查询 → 修改状态 → 验证数据持久化

### 结果

- 采用
- 全部 API 验证通过：health/check → create(201) → list(200) → update(in_progress/resolved)
- Go 重启后 SQLite 数据保留，3 条记录完整存在
