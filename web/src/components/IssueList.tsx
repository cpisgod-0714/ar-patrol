import { useEffect, useState } from "react";
import type { Issue, Status } from "../types/issue";
import { fetchIssues, updateIssueStatus } from "../api/client";
import { IssueCard } from "./IssueCard";
import { ErrorBanner } from "./ErrorBanner";

export function IssueList() {
  const [issues, setIssues] = useState<Issue[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadIssues();
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
      setIssues((prev) => prev.map((i) => (i.id === id ? updated : i)));
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : "修改失败");
    }
  }

  if (error) {
    return <ErrorBanner message={error} onRetry={loadIssues} />;
  }

  if (loading) {
    return <div className="loading">加载中...</div>;
  }

  return (
    <div className="issue-list">
      {issues.length === 0 ? (
        <div className="empty-state">暂无问题记录</div>
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
