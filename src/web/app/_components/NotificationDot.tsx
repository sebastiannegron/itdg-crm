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

const dotColors: Record<NotificationType, string> = {
  doc: "bg-info",
  alert: "bg-destructive",
  payment: "bg-destructive",
  task: "bg-accent",
  msg: "bg-[#7C3AED]",
};

export function NotificationDot({
  type,
  className,
}: {
  type: string;
  className?: string;
}) {
  const color = dotColors[type as NotificationType] ?? "bg-muted-foreground";

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
