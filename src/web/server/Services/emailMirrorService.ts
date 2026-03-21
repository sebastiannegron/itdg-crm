import { apiFetch } from "./api-client";

export interface EmailMirrorDto {
  email_id: string;
  client_id: string;
  gmail_message_id: string;
  gmail_thread_id: string;
  subject: string;
  from: string;
  to: string;
  body_preview: string | null;
  has_attachments: boolean;
  received_at: string;
}

export interface PaginatedEmails {
  items: EmailMirrorDto[];
  total_count: number;
  page: number;
  page_size: number;
}

export interface GetClientEmailsParams {
  clientId: string;
  page?: number;
  pageSize?: number;
  search?: string;
}

export async function getClientEmails(
  params: GetClientEmailsParams,
): Promise<PaginatedEmails> {
  const searchParams = new URLSearchParams();

  if (params.page) searchParams.set("page", String(params.page));
  if (params.pageSize) searchParams.set("pageSize", String(params.pageSize));
  if (params.search) searchParams.set("search", params.search);

  const query = searchParams.toString();
  const path = `/api/v1/Clients/${params.clientId}/Emails${query ? `?${query}` : ""}`;

  return apiFetch<PaginatedEmails>(path);
}
