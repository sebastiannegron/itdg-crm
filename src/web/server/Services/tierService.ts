import { apiFetch } from "./api-client";

export interface ClientTierDto {
  tier_id: string;
  name: string;
  sort_order: number;
  created_at: string;
  updated_at: string;
}

export interface CreateTierParams {
  name: string;
  sort_order: number;
}

export interface UpdateTierParams {
  name: string;
  sort_order: number;
}

export async function getTiers(): Promise<ClientTierDto[]> {
  return apiFetch<ClientTierDto[]>("/api/v1/Tiers");
}

export async function createTier(
  params: CreateTierParams,
): Promise<void> {
  return apiFetch<void>("/api/v1/Tiers", {
    method: "POST",
    body: JSON.stringify(params),
  });
}

export async function updateTier(
  tierId: string,
  params: UpdateTierParams,
): Promise<void> {
  return apiFetch<void>(`/api/v1/Tiers/${tierId}`, {
    method: "PUT",
    body: JSON.stringify(params),
  });
}
