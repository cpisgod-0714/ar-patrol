---
name: ar-patrol-schema
description: AR 巡检项目数据契约与 API 规范，保证 Unity/Go/React 三端字段名、枚举值、接口路径完全一致
---

# AR 巡检 — 数据契约与 API 规范

> 所有涉及 Issue 数据结构或 API 调用的代码，必须严格遵循本文档，确保三端一致性。

## 1. Issue 数据结构

三端字段名**必须完全一致**：

```json
{
  "id": "issue_1709123456789",
  "title": "入口墙面破损",
  "description": "左侧墙体存在裂缝",
  "priority": "high",
  "status": "open",
  "position": {
    "x": 0.42,
    "y": 0.03,
    "z": 1.26
  },
  "createdAt": "2024-03-01T10:00:00Z",
  "updatedAt": "2024-03-01T10:00:00Z"
}
```

### 字段规则

| 字段 | 类型 | 必填 | 说明 |
|---|---|---|---|
| `id` | string | 后端生成 | 格式 `issue_{unix毫秒时间戳}`，全局唯一 |
| `title` | string | ✅ 必填 | 不能为空字符串 |
| `description` | string | 可选 | 默认空字符串 `""` |
| `priority` | enum | ✅ 必填 | 仅限 `low` / `medium` / `high` |
| `status` | enum | 后端生成 | 仅限 `open` / `in_progress` / `resolved`，新建时默认 `open` |
| `position` | object | ✅ 必填 | 包含 `x`, `y`, `z` 三个 float64 |
| `position.x` | float64 | ✅ | AR 标记世界坐标 X |
| `position.y` | float64 | ✅ | AR 标记世界坐标 Y |
| `position.z` | float64 | ✅ | AR 标记世界坐标 Z |
| `createdAt` | string (ISO 8601) | 后端生成 | RFC 3339 格式，如 `2024-03-01T10:00:00Z` |
| `updatedAt` | string (ISO 8601) | 后端生成 | 同上，修改状态时更新 |

## 2. API 路径设计

基础路径：`/api`

### 2.1 健康检查

```
GET /api/health
```

**响应 200：**
```json
{ "status": "ok" }
```

### 2.2 创建问题

```
POST /api/issues
```

**请求体：**
```json
{
  "title": "入口墙面破损",
  "description": "左侧墙体存在裂缝",
  "priority": "high",
  "position": { "x": 0.42, "y": 0.03, "z": 1.26 }
}
```

**成功响应 201：** 完整 Issue 对象（含后端生成的 id, status, createdAt, updatedAt）

**校验错误响应 400：**
```json
{ "error": "title is required" }
```
```json
{ "error": "invalid priority: must be low, medium, or high" }
```

### 2.3 获取问题列表

```
GET /api/issues
```

**成功响应 200：**
```json
[
  { "id": "...", "title": "...", ... },
  { "id": "...", "title": "...", ... }
]
```

空列表时返回 `[]`，不返回 `null`。

### 2.4 修改问题状态

```
PATCH /api/issues/:id
```

**请求体：**
```json
{ "status": "in_progress" }
```

**成功响应 200：** 更新后的完整 Issue 对象

**校验错误响应 400：**
```json
{ "error": "invalid status: must be open, in_progress, or resolved" }
```

**未找到响应 404：**
```json
{ "error": "issue not found" }
```

## 3. CORS 配置

Go 后端**必须**配置 CORS，允许 React 开发服务器（默认 `http://localhost:5173`）和 Unity 客户端跨域访问：

```
Access-Control-Allow-Origin: *
Access-Control-Allow-Methods: GET, POST, PATCH, OPTIONS
Access-Control-Allow-Headers: Content-Type
```

OPTIONS 请求返回 204，不带 body。

## 4. 错误响应格式

所有错误统一格式：
```json
{ "error": "具体错误描述" }
```

HTTP 状态码使用：
- `400` — 参数校验失败
- `404` — 资源未找到
- `405` — 方法不允许
- `500` — 服务器内部错误

## 5. 三端一致性检查清单

在每端开发时，逐条确认：

- [ ] 字段名拼写与本文档完全一致（包括驼峰/下划线风格）
- [ ] 枚举值字符串与本文档完全一致
- [ ] API 路径与本文档完全一致
- [ ] JSON key 使用双引号，无尾部逗号
- [ ] 时间格式为 ISO 8601 / RFC 3339
- [ ] 空列表返回 `[]` 而非 `null`
- [ ] 错误响应使用统一的 `{ "error": "..." }` 格式
