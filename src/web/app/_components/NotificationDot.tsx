import { cn } from "@/lib/utils";

type NotificationType = "doc" | "alert" | "task" | "msg";

interface NotificationDotProps {
  type: NotificationType;
  className?: string;
}

const dotColorMap: Record<NotificationType, string> = {
  doc: "bg-blue-500",
  alert: "bg-red-500",
  task: "bg-emerald-500",
  msg: "bg-purple-500",
};

export function NotificationDot({ type, className }: NotificationDotProps) {
  return (
    <span
      className={cn(
        "inline-block h-2 w-2 rounded-full",
        dotColorMap[type],
        className
      )}
      role="status"
      aria-label={`${type} notification`}
    />
  );
}
