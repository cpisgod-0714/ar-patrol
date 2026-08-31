import type { Issue, Status } from "../types/issue";

const API_BASE = "/api";

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
