import { apiFetch } from "./api-client";

export interface TimelineItemDto {
  id: string;
  type: string;
  description: string;
  timestamp: string;
  actor: string | null;
}

export interface PaginatedTimeline {
  items: TimelineItemDto[];
  total_count: number;
  page: number;
  page_size: number;
}

export interface GetClientTimelineParams {
  clientId: string;
  page?: number;
  pageSize?: number;
}

export async function getClientTimeline(
  params: GetClientTimelineParams,
): Promise<PaginatedTimeline> {
  const searchParams = new URLSearchParams();

  if (params.page) searchParams.set("page", String(params.page));
  if (params.pageSize) searchParams.set("pageSize", String(params.pageSize));

  const query = searchParams.toString();
  const path = `/api/v1/Clients/${params.clientId}/Timeline${query ? `?${query}` : ""}`;

  return apiFetch<PaginatedTimeline>(path);
}
