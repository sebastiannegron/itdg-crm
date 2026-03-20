import {
  getDashboardSummary,
  type DashboardSummaryDto,
} from "@/server/Services/dashboardService";
import DashboardView from "./DashboardView";
import type { TaskItem, EscalationItem, DeadlineItem } from "./shared";

export default async function DashboardPage() {
  let summary: DashboardSummaryDto | null = null;
  try {
    summary = await getDashboardSummary();
  } catch (error) {
    console.error("[DashboardPage] Failed to fetch dashboard summary:", error);
  }

  // Placeholder data until backend task/escalation/deadline endpoints are available
  const tasks: TaskItem[] = [];
  const escalations: EscalationItem[] = [];
  const deadlines: DeadlineItem[] = [];

  return (
    <DashboardView
      initialSummary={summary}
      initialTasks={tasks}
      initialEscalations={escalations}
      initialDeadlines={deadlines}
    />
  );
}
