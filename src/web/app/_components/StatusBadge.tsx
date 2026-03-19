import { cn } from "@/lib/utils";

type ClientStatus = "Active" | "Pending Docs" | "Awaiting Payment";
type TaskStatus = "To Do" | "In Progress" | "Review" | "Done";
type Status = ClientStatus | TaskStatus;

const statusStyles: Record<Status, string> = {
  Active: "text-[#065F46] bg-[#ECFDF5]",
  "Pending Docs": "text-[#92400E] bg-[#FFFBEB]",
  "Awaiting Payment": "text-[#991B1B] bg-[#FEF2F2]",
  "To Do": "text-[#374151] bg-[#F9FAFB]",
  "In Progress": "text-[#1E40AF] bg-[#EFF6FF]",
  Review: "text-[#6D28D9] bg-[#F5F3FF]",
  Done: "text-[#065F46] bg-[#ECFDF5]",
};

export function StatusBadge({
  status,
  className,
}: {
  status: string;
  className?: string;
}) {
  const styles = statusStyles[status as Status] ?? statusStyles["To Do"];

  return (
    <span
      className={cn(
        "inline-flex items-center rounded-full px-2 py-0.5 text-[10px] font-semibold",
        styles,
        className
      )}
    >
      {status}
    </span>
  );
}
