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

export interface CreateClientParams {
  name: string;
  contact_email?: string;
  phone?: string;
  address?: string;
  tier_id?: string;
  status: string;
  industry_tag?: string;
  notes?: string;
  custom_fields?: string;
}

export interface UpdateClientParams {
  name: string;
  contact_email?: string;
  phone?: string;
  address?: string;
  tier_id?: string;
  status: string;
  industry_tag?: string;
  notes?: string;
  custom_fields?: string;
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

export async function getClientById(clientId: string): Promise<ClientDto> {
  return apiFetch<ClientDto>(`/api/v1/Clients/${clientId}`);
}

export async function createClient(
  params: CreateClientParams,
): Promise<ClientDto> {
  return apiFetch<ClientDto>("/api/v1/Clients", {
    method: "POST",
    body: JSON.stringify(params),
  });
}

export async function updateClient(
  clientId: string,
  params: UpdateClientParams,
): Promise<void> {
  return apiFetch<void>(`/api/v1/Clients/${clientId}`, {
    method: "PUT",
    body: JSON.stringify(params),
  });
}

export interface ClientAssignmentDto {
  user_id: string;
  display_name: string;
  email: string;
  assigned_at: string;
}

export async function getClientAssignments(
  clientId: string,
): Promise<ClientAssignmentDto[]> {
  return apiFetch<ClientAssignmentDto[]>(
    `/api/v1/Clients/${clientId}/Assignments`,
  );
}

export async function assignClient(
  clientId: string,
  userId: string,
): Promise<void> {
  return apiFetch<void>(`/api/v1/Clients/${clientId}/Assignments`, {
    method: "POST",
    body: JSON.stringify({ user_id: userId }),
  });
}

export async function unassignClient(
  clientId: string,
  userId: string,
): Promise<void> {
  return apiFetch<void>(`/api/v1/Clients/${clientId}/Assignments/${userId}`, {
    method: "DELETE",
  });
}
