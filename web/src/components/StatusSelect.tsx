import type { Status } from "../types/issue";
import { STATUSES, STATUS_LABELS } from "../types/issue";

interface StatusSelectProps {
  value: Status;
  onChange: (status: Status) => void;
}

export function StatusSelect({ value, onChange }: StatusSelectProps) {
  return (
    <select
      className="status-select"
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
