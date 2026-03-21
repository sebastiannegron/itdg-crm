import { apiFetch } from "./api-client";

export interface PortalDocumentDto {
  document_id: string;
  client_id: string;
  category_id: string;
  category_name: string | null;
  file_name: string;
  google_drive_file_id: string;
  uploaded_by_id: string;
  current_version: number;
  file_size: number;
  mime_type: string;
  created_at: string;
  updated_at: string;
}

export interface PaginatedPortalDocuments {
  items: PortalDocumentDto[];
  total_count: number;
  page: number;
  page_size: number;
}

export interface GetPortalDocumentsParams {
  page?: number;
  pageSize?: number;
  categoryId?: string;
  year?: number;
  search?: string;
}

export async function getPortalDocuments(
  params: GetPortalDocumentsParams = {},
): Promise<PaginatedPortalDocuments> {
  const searchParams = new URLSearchParams();

  if (params.page) searchParams.set("page", String(params.page));
  if (params.pageSize) searchParams.set("pageSize", String(params.pageSize));
  if (params.categoryId) searchParams.set("categoryId", params.categoryId);
  if (params.year) searchParams.set("year", String(params.year));
  if (params.search) searchParams.set("search", params.search);

  const query = searchParams.toString();
  const path = `/api/v1/Portal/Documents${query ? `?${query}` : ""}`;

  return apiFetch<PaginatedPortalDocuments>(path);
}

export async function uploadPortalDocument(
  file: File,
  categoryId: string,
): Promise<void> {
  const formData = new FormData();
  formData.append("file", file);
  formData.append("category_id", categoryId);

  await apiFetch<void>("/api/v1/Portal/Documents", {
    method: "POST",
    body: formData,
    headers: {},
  });
}
