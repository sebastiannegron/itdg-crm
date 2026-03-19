import { Badge } from "@/app/_components/ui/badge";
import { cn } from "@/lib/utils";

interface StatusBadgeProps {
  status: string;
  className?: string;
}

const statusColorMap: Record<string, string> = {
  active: "bg-emerald-100 text-emerald-800 border-emerald-200",
  "pending docs": "bg-amber-100 text-amber-800 border-amber-200",
  "awaiting payment": "bg-red-100 text-red-800 border-red-200",
  completed: "bg-emerald-100 text-emerald-800 border-emerald-200",
  "in progress": "bg-blue-100 text-blue-800 border-blue-200",
  "not started": "bg-gray-100 text-gray-800 border-gray-200",
  overdue: "bg-red-100 text-red-800 border-red-200",
  cancelled: "bg-gray-100 text-gray-500 border-gray-200",
};

const defaultColor = "bg-gray-100 text-gray-800 border-gray-200";

export function StatusBadge({ status, className }: StatusBadgeProps) {
  const colorClasses = statusColorMap[status.toLowerCase()] ?? defaultColor;

  return <Badge className={cn(colorClasses, className)}>{status}</Badge>;
}
