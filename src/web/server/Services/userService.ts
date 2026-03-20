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

export interface UpdateUserParams {
  role: string;
  is_active: boolean;
}

export interface InviteUserParams {
  email: string;
  display_name: string;
  role: string;
}

export async function getUsers(): Promise<PaginatedUsers> {
  return apiFetch<PaginatedUsers>("/api/v1/Users?pageSize=100&isActive=true");
}

export async function getAllUsers(): Promise<PaginatedUsers> {
  return apiFetch<PaginatedUsers>("/api/v1/Users?pageSize=100");
}

export async function getUserById(userId: string): Promise<UserDto> {
  return apiFetch<UserDto>(`/api/v1/Users/${userId}`);
}

export async function updateUser(
  userId: string,
  params: UpdateUserParams,
): Promise<void> {
  return apiFetch<void>(`/api/v1/Users/${userId}`, {
    method: "PUT",
    body: JSON.stringify(params),
  });
}

export async function inviteUser(params: InviteUserParams): Promise<void> {
  return apiFetch<void>("/api/v1/Users/invite", {
    method: "POST",
    body: JSON.stringify(params),
  });
}
