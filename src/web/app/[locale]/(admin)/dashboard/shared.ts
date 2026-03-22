import type { DashboardSummaryDto } from "@/server/Services/dashboardService";
import type {
  DashboardCalendarDto,
  DashboardCalendarEventDto,
  CalendarTeamMemberDto,
} from "@/server/Services/dashboardService";

export type {
  DashboardSummaryDto,
  DashboardCalendarDto,
  DashboardCalendarEventDto,
  CalendarTeamMemberDto,
};

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
