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
