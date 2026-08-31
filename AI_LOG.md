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
- React 管理端：Vite + TypeScript + 5 个组件 + API client + Vite 代理，构建通过
- Unity AR：4 个 C# 脚本占位，等 Unity Editor 安装后集成
- 修改了 Go model/issue.go 缺失的 fmt import 编译错误

## P-004

- 目标：修复 Go 后端编译错误

### Prompt 原文

> (自动发现) model/issue.go 编译报 undefined: fmt

### 结果

- 修改后采用
- 添加 `"fmt"` 到 import 列表，编译通过
