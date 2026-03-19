import { cn } from "@/lib/utils";

export type NotificationType =
  | "document"
  | "payment"
  | "task"
  | "escalation"
  | "message"
  | "system";

const dotColors: Record<NotificationType, string> = {
  document: "bg-primary",
  payment: "bg-accent",
  task: "bg-warning",
  escalation: "bg-destructive",
  message: "bg-primary",
  system: "bg-muted-foreground",
};

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
        "inline-block h-2 w-2 shrink-0 rounded-full",
        dotColors[type],
        className
      )}
      aria-hidden="true"
      role="status"
      aria-label={`${type} notification`}
    />
  );
}
