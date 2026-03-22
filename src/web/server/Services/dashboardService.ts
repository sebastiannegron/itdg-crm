import { apiFetch } from "./api-client";

export interface HealthStatusDto {
  status: string;
  timestamp: string;
}

export interface ClientStatusCountDto {
  status: string;
  count: number;
}

export interface ClientTierCountDto {
  tier_id: string | null;
  tier_name: string | null;
  count: number;
}

export interface DashboardSummaryDto {
  total_clients: number;
  clients_by_status: ClientStatusCountDto[];
  clients_by_tier: ClientTierCountDto[];
  pending_tasks_count: number;
  recent_escalations_count: number;
  upcoming_deadlines_count: number;
  unread_notifications_count: number;
}

export async function getHealthStatus(): Promise<HealthStatusDto> {
  return apiFetch<HealthStatusDto>("/api/v1/Health");
}

export async function getDashboardSummary(): Promise<DashboardSummaryDto> {
  return apiFetch<DashboardSummaryDto>("/api/v1/Dashboard/Summary");
}

export interface CalendarTeamMemberDto {
  name: string;
  color: string;
}

export interface DashboardCalendarEventDto {
  id: string;
  summary: string | null;
  description: string | null;
  location: string | null;
  start: string | null;
  end: string | null;
  calendar_id: string;
  html_link: string | null;
  status: string | null;
  organizer_email: string | null;
  created: string | null;
  updated: string | null;
  team_member_name: string;
  team_member_color: string;
}

export interface DashboardCalendarDto {
  events: DashboardCalendarEventDto[];
  team_members: CalendarTeamMemberDto[];
}

export async function getDashboardCalendar(
  startDate: string,
  endDate: string,
): Promise<DashboardCalendarDto> {
  return apiFetch<DashboardCalendarDto>(
    `/api/v1/Dashboard/Calendar?start_date=${encodeURIComponent(startDate)}&end_date=${encodeURIComponent(endDate)}`,
  );
}
