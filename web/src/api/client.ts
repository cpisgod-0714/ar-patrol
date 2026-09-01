import type { Issue, Status } from "../types/issue";

// 生产环境通过 VITE_API_BASE 环境变量注入后端地址；开发环境走 Vite 代理
const API_BASE = import.meta.env.VITE_API_BASE || "/api";

export async function checkHealth(): Promise<boolean> {
  try {
    const res = await fetch(`${API_BASE}/health`);
    return res.ok;
  } catch {
    return false;
  }
}

export async function fetchIssues(): Promise<Issue[]> {
  const res = await fetch(`${API_BASE}/issues`);
  if (!res.ok) {
    const err = await res.json().catch(() => null);
    throw new Error(err?.error || `获取问题列表失败 (${res.status})`);
  }
  return res.json();
}

export async function createIssue(data: {
  title: string;
  description: string;
  priority: string;
  position: { x: number; y: number; z: number };
}): Promise<Issue> {
  const res = await fetch(`${API_BASE}/issues`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  if (!res.ok) {
    const err = await res.json().catch(() => null);
    throw new Error(err?.error || `创建问题失败 (${res.status})`);
  }
  return res.json();
}

export async function updateIssueStatus(id: string, status: Status): Promise<Issue> {
  const res = await fetch(`${API_BASE}/issues/${id}`, {
    method: "PATCH",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ status }),
  });
  if (!res.ok) {
    const err = await res.json().catch(() => null);
    throw new Error(err?.error || `修改状态失败 (${res.status})`);
  }
  return res.json();
}
