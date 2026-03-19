import { cn } from "@/lib/utils";

type NotificationType = "doc" | "alert" | "payment" | "task" | "msg";

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
    <div
      className={cn("h-2 w-2 shrink-0 rounded-full", color, className)}
      aria-hidden="true"
    />
  );
}
