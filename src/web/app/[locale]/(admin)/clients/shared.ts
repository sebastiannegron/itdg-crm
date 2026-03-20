import type { ClientDto, PaginatedResult } from "@/server/Services/clientService";

export type { ClientDto, PaginatedResult };

export const CLIENT_STATUSES = ["Active", "Inactive", "Suspended"] as const;
export type ClientStatus = (typeof CLIENT_STATUSES)[number];

export interface ClientsPageData {
  clients: PaginatedResult<ClientDto>;
}

export interface ClientFilters {
  search: string;
  status: string;
  tier: string;
}
