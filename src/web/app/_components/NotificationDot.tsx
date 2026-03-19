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

export function NotificationDot({
  type,
  className,
}: {
  type: NotificationType;
  className?: string;
}) {
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
