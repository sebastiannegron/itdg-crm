import { apiFetch } from "./api-client";

export interface ClientDto {
  client_id: string;
  name: string;
  contact_email: string | null;
  phone: string | null;
  address: string | null;
  tier_id: string | null;
  tier_name: string | null;
  status: string;
  industry_tag: string | null;
  notes: string | null;
  custom_fields: string | null;
  created_at: string;
  updated_at: string;
}

export interface PaginatedResult<T> {
  items: T[];
  total_count: number;
  page: number;
  page_size: number;
}

export interface GetClientsParams {
  page?: number;
  pageSize?: number;
  status?: string;
  tierId?: string;
  search?: string;
}

export async function getClients(
  params?: GetClientsParams,
): Promise<PaginatedResult<ClientDto>> {
  const searchParams = new URLSearchParams();

  if (params?.page) searchParams.set("page", String(params.page));
  if (params?.pageSize) searchParams.set("pageSize", String(params.pageSize));
  if (params?.status) searchParams.set("status", params.status);
  if (params?.tierId) searchParams.set("tierId", params.tierId);
  if (params?.search) searchParams.set("search", params.search);

  const query = searchParams.toString();
  const path = `/api/v1/Clients${query ? `?${query}` : ""}`;

  return apiFetch<PaginatedResult<ClientDto>>(path);
}
