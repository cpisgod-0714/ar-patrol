import type { Priority } from "../types/issue";
import { PRIORITY_LABELS, PRIORITY_COLORS } from "../types/issue";

interface PriorityBadgeProps {
  priority: Priority;
}

export function PriorityBadge({ priority }: PriorityBadgeProps) {
  return (
    <span
      className="priority-badge"
      style={{ backgroundColor: PRIORITY_COLORS[priority] }}
    >
      {PRIORITY_LABELS[priority]}
    </span>
  );
}
