import { apiFetch } from "./api-client";

export interface UserDto {
  user_id: string;
  entra_object_id: string;
  email: string;
  display_name: string;
  role: string;
  is_active: boolean;
  created_at: string;
  updated_at: string;
}

export interface PaginatedUsers {
  items: UserDto[];
  total_count: number;
  page: number;
  page_size: number;
}

export async function getUsers(): Promise<PaginatedUsers> {
  return apiFetch<PaginatedUsers>("/api/v1/Users?pageSize=100&isActive=true");
}
