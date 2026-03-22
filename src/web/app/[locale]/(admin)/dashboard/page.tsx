import {
  getDashboardSummary,
  getDashboardCalendar,
  type DashboardSummaryDto,
  type DashboardCalendarDto,
} from "@/server/Services/dashboardService";
import DashboardView from "./DashboardView";
import type { TaskItem, EscalationItem, DeadlineItem } from "./shared";

export default async function DashboardPage() {
  let summary: DashboardSummaryDto | null = null;
  let calendar: DashboardCalendarDto | null = null;

  try {
    summary = await getDashboardSummary();
  } catch (error) {
    console.error("[DashboardPage] Failed to fetch dashboard summary:", error);
  }

  try {
    const now = new Date();
    const weekStart = new Date(now);
    weekStart.setDate(weekStart.getDate() - weekStart.getDay());
    weekStart.setHours(0, 0, 0, 0);
    const weekEnd = new Date(weekStart);
    weekEnd.setDate(weekEnd.getDate() + 7);
    calendar = await getDashboardCalendar(
      weekStart.toISOString(),
      weekEnd.toISOString(),
    );
  } catch (error) {
    console.error("[DashboardPage] Failed to fetch dashboard calendar:", error);
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
      initialCalendar={calendar}
    />
  );
}
