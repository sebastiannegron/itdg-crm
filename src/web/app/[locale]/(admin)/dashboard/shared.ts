import type { DashboardSummaryDto } from "@/server/Services/dashboardService";

export type { DashboardSummaryDto };

export interface TaskItem {
  id: string;
  title: string;
  priority: "High" | "Medium" | "Low";
  status: string;
  due_date: string;
}

export interface EscalationItem {
  id: string;
  title: string;
  client_name: string;
  created_at: string;
}

export interface DeadlineItem {
  id: string;
  title: string;
  due_date: string;
}
