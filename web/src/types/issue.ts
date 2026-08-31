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

export const PRIORITY_COLORS: Record<Priority, string> = {
  low: "#22c55e",
  medium: "#f59e0b",
  high: "#ef4444",
};

export const STATUS_COLORS: Record<Status, string> = {
  open: "#3b82f6",
  in_progress: "#f59e0b",
  resolved: "#22c55e",
};
