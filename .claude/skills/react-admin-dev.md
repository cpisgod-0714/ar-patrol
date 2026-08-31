---
name: react-admin-dev
description: React + TypeScript 管理端开发规范，涵盖组件设计、API 调用、错误容错、状态管理、构建配置
---

# React 管理端开发规范

## 1. 项目结构

```
web/
├── src/
│   ├── components/
│   │   ├── IssueList.tsx        # 问题列表主组件
│   │   ├── IssueCard.tsx        # 单条问题卡片
│   │   ├── StatusSelect.tsx     # 状态下拉选择器
│   │   ├── PriorityBadge.tsx    # 优先级标签
│   │   └── ErrorBanner.tsx      # 错误提示横幅
│   ├── api/
│   │   └── client.ts           # fetch 封装 + API 调用
│   ├── types/
│   │   └── issue.ts            # Issue 类型定义
│   ├── App.tsx                 # 根组件
│   └── main.tsx                # 入口
├── index.html
├── vite.config.ts
├── tsconfig.json
├── package.json
└── README.md
```

## 2. 技术选型

| 项目 | 选择 | 说明 |
|---|---|---|
| 构建工具 | Vite | 快速开发，内置 TypeScript 支持 |
| 框架 | React 18+ | — |
| 语言 | TypeScript | 类型安全，字段名与后端一致 |
| 状态管理 | useState + useEffect | 项目简单，不需要 Redux/Zustand |
| 样式 | Tailwind CSS 或纯 CSS | 项目简单，Tailwind 更快 |
| HTTP 客户端 | 原生 fetch | 无需 axios，减少依赖 |

## 3. 类型定义 (types/issue.ts)

**字段名必须与数据契约完全一致：**

```typescript
export interface Position {
  x: number;
  y: number;
  z: number;
}

export interface Issue {
  id: string;
  title: string;
  description: string;
  priority: "low" | "medium" | "high";
  status: "open" | "in_progress" | "resolved";
  position: Position;
  createdAt: string;
  updatedAt: string;
}

export type Priority = Issue["priority"];
export type Status = Issue["status"];

export const PRIORITIES: Priority[] = ["low", "medium", "high"];
export const STATUSES: Status[] = ["open", "in_progress", "resolved"];

export const STATUS_LABELS: Record<Status, string> = {
  open: "待处理",
  in_progress: "处理中",
  resolved: "已解决",
};

export const PRIORITY_LABELS: Record<Priority, string> = {
  low: "低",
  medium: "中",
  high: "高",
};
```

## 4. API 客户端 (api/client.ts)

```typescript
const API_BASE = "/api"; // 开发时通过 Vite 代理，生产时同域

export async function fetchIssues(): Promise<Issue[]> {
  const res = await fetch(`${API_BASE}/issues`);
  if (!res.ok) {
    throw new Error(`获取问题列表失败: ${res.status}`);
  }
  return res.json();
}

export async function updateIssueStatus(
  id: string,
  status: Status
): Promise<Issue> {
  const res = await fetch(`${API_BASE}/issues/${id}`, {
    method: "PATCH",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ status }),
  });
  if (!res.ok) {
    throw new Error(`修改状态失败: ${res.status}`);
  }
  return res.json();
}

export async function checkHealth(): Promise<boolean> {
  try {
    const res = await fetch(`${API_BASE}/health`);
    return res.ok;
  } catch {
    return false;
  }
}
```

## 5. 组件设计

### IssueList.tsx — 主列表

```tsx
import { useEffect, useState } from "react";
import { Issue } from "../types/issue";
import { fetchIssues, updateIssueStatus } from "../api/client";
import { IssueCard } from "./IssueCard";
import { ErrorBanner } from "./ErrorBanner";

export function IssueList() {
  const [issues, setIssues] = useState<Issue[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadIssues();
    // 可选：定时刷新
    const timer = setInterval(loadIssues, 10000);
    return () => clearInterval(timer);
  }, []);

  async function loadIssues() {
    try {
      const data = await fetchIssues();
      setIssues(data);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : "未知错误");
    } finally {
      setLoading(false);
    }
  }

  async function handleStatusChange(id: string, status: Status) {
    try {
      const updated = await updateIssueStatus(id, status);
      setIssues((prev) =>
        prev.map((i) => (i.id === id ? updated : i))
      );
    } catch (err) {
      setError(err instanceof Error ? err.message : "修改失败");
    }
  }

  if (error) {
    return <ErrorBanner message={error} onRetry={loadIssues} />;
  }

  if (loading) {
    return <div>加载中...</div>;
  }

  return (
    <div>
      {issues.length === 0 ? (
        <div>暂无问题记录</div>
      ) : (
        issues.map((issue) => (
          <IssueCard
            key={issue.id}
            issue={issue}
            onStatusChange={handleStatusChange}
          />
        ))
      )}
    </div>
  );
}
```

### ErrorBanner.tsx — 错误容错

**评审硬指标：后端不可用时不能白屏。**

```tsx
interface ErrorBannerProps {
  message: string;
  onRetry?: () => void;
}

export function ErrorBanner({ message, onRetry }: ErrorBannerProps) {
  return (
    <div className="error-banner">
      <p>⚠️ {message}</p>
      {onRetry && (
        <button onClick={onRetry}>重试</button>
      )}
    </div>
  );
}
```

### StatusSelect.tsx — 状态修改

```tsx
import { Status, STATUSES, STATUS_LABELS } from "../types/issue";

interface StatusSelectProps {
  value: Status;
  onChange: (status: Status) => void;
}

export function StatusSelect({ value, onChange }: StatusSelectProps) {
  return (
    <select
      value={value}
      onChange={(e) => onChange(e.target.value as Status)}
    >
      {STATUSES.map((s) => (
        <option key={s} value={s}>
          {STATUS_LABELS[s]}
        </option>
      ))}
    </select>
  );
}
```

### IssueCard.tsx — 单条问题

至少显示：标题、优先级、状态（可修改）

```tsx
import { Issue, Status } from "../types/issue";
import { StatusSelect } from "./StatusSelect";
import { PriorityBadge } from "./PriorityBadge";

interface IssueCardProps {
  issue: Issue;
  onStatusChange: (id: string, status: Status) => void;
}

export function IssueCard({ issue, onStatusChange }: IssueCardProps) {
  return (
    <div className="issue-card">
      <h3>{issue.title}</h3>
      <PriorityBadge priority={issue.priority} />
      <StatusSelect
        value={issue.status}
        onChange={(status) => onStatusChange(issue.id, status)}
      />
      {issue.description && <p>{issue.description}</p>}
      <span className="timestamp">
        创建: {issue.createdAt} | 更新: {issue.updatedAt}
      </span>
    </div>
  );
}
```

## 6. 错误容错策略

**后端不可用时，页面必须显示错误提示，不能白屏。**

| 场景 | 处理 |
|---|---|
| 首次加载失败 | 显示 ErrorBanner + 重试按钮 |
| 修改状态失败 | 显示临时错误提示，保留当前状态 |
| 网络断开 | fetch 抛出 TypeError，被 catch 捕获显示 ErrorBanner |
| 后端返回非 JSON | try/catch 解析失败，显示"响应格式错误" |

### 根组件兜底

```tsx
// App.tsx — 最外层 ErrorBoundary 防止白屏
export default function App() {
  return (
    <div>
      <h1>巡检问题管理</h1>
      <IssueList />
    </div>
  );
}
```

如果 React 抛出未捕获异常，考虑添加 `ErrorBoundary` 组件兜底。

## 7. Vite 代理配置

开发时避免跨域问题，让 Vite 代理 API 请求到 Go 后端：

```typescript
// vite.config.ts
import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      "/api": {
        target: "http://localhost:8080",
        changeOrigin: true,
      },
    },
  },
});
```

这样前端 `fetch("/api/issues")` 会代理到 Go 的 `http://localhost:8080/api/issues`。

## 8. 检查清单

- [ ] TypeScript 类型定义与数据契约字段名一致
- [ ] 至少显示标题、优先级、状态
- [ ] 状态可通过下拉框修改为 open / in_progress / resolved
- [ ] 后端不可用时显示错误提示，不白屏
- [ ] 错误状态可重试
- [ ] 列表为空时显示友好提示（而非空白）
- [ ] API 路径与后端一致（/api/issues）
- [ ] 开发代理配置正确
- [ ] 构建后可通过同一 Go 后端访问（或配置 CORS）
