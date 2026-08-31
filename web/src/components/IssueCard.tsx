import type { Issue, Status } from "../types/issue";
import { StatusSelect } from "./StatusSelect";
import { PriorityBadge } from "./PriorityBadge";
import { STATUS_LABELS, STATUS_COLORS } from "../types/issue";

interface IssueCardProps {
  issue: Issue;
  onStatusChange: (id: string, status: Status) => void;
}

export function IssueCard({ issue, onStatusChange }: IssueCardProps) {
  const formatTime = (iso: string) => {
    try {
      return new Date(iso).toLocaleString("zh-CN");
    } catch {
      return iso;
    }
  };

  return (
    <div className="issue-card">
      <div className="issue-header">
        <h3 className="issue-title">{issue.title}</h3>
        <PriorityBadge priority={issue.priority} />
      </div>
      {issue.description && (
        <p className="issue-desc">{issue.description}</p>
      )}
      <div className="issue-meta">
        <span className="issue-status-label">
          状态：
          <span
            className="status-dot"
            style={{ backgroundColor: STATUS_COLORS[issue.status] }}
          />
          {STATUS_LABELS[issue.status]}
        </span>
        <StatusSelect
          value={issue.status}
          onChange={(status) => onStatusChange(issue.id, status)}
        />
      </div>
      <div className="issue-footer">
        <span className="issue-position">
          位置: ({issue.position.x.toFixed(2)}, {issue.position.y.toFixed(2)}, {issue.position.z.toFixed(2)})
        </span>
        <span className="issue-time">
          创建: {formatTime(issue.createdAt)}
        </span>
        <span className="issue-time">
          更新: {formatTime(issue.updatedAt)}
        </span>
      </div>
    </div>
  );
}
