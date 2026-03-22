import { apiFetch } from "./api-client";

export interface DocumentDto {
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

export interface DocumentDownloadDto {
  document_id: string;
  file_name: string;
  mime_type: string;
  file_size: number;
  google_drive_file_id: string;
  web_view_link: string | null;
}

export interface DocumentVersionDto {
  version_id: string;
  document_id: string;
  version_number: number;
  google_drive_file_id: string;
  uploaded_by_id: string;
  uploaded_at: string;
}

export interface DocumentDetailDto {
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
  web_view_link: string | null;
  versions: DocumentVersionDto[];
}

export interface PaginatedDocuments {
  items: DocumentDto[];
  total_count: number;
  page: number;
  page_size: number;
}

export interface GetClientDocumentsParams {
  clientId: string;
  page?: number;
  pageSize?: number;
  categoryId?: string;
  year?: number;
  search?: string;
}

export async function getClientDocuments(
  params: GetClientDocumentsParams,
): Promise<PaginatedDocuments> {
  const searchParams = new URLSearchParams();

  if (params.page) searchParams.set("page", String(params.page));
  if (params.pageSize) searchParams.set("pageSize", String(params.pageSize));
  if (params.categoryId) searchParams.set("categoryId", params.categoryId);
  if (params.year) searchParams.set("year", String(params.year));
  if (params.search) searchParams.set("search", params.search);

  const query = searchParams.toString();
  const path = `/api/v1/Clients/${params.clientId}/Documents${query ? `?${query}` : ""}`;

  return apiFetch<PaginatedDocuments>(path);
}

export async function getDocumentDownload(
  documentId: string,
): Promise<DocumentDownloadDto> {
  return apiFetch<DocumentDownloadDto>(`/api/v1/Documents/${documentId}`);
}

export async function getDocumentDetail(
  documentId: string,
): Promise<DocumentDetailDto> {
  return apiFetch<DocumentDetailDto>(`/api/v1/Documents/${documentId}/Detail`);
}

export async function uploadNewVersion(
  documentId: string,
  file: File,
): Promise<void> {
  const formData = new FormData();
  formData.append("file", file);

  await apiFetch<void>(`/api/v1/Documents/${documentId}/Versions`, {
    method: "POST",
    body: formData,
    headers: {},
  });
}

export async function deleteDocument(documentId: string): Promise<void> {
  await apiFetch<void>(`/api/v1/Documents/${documentId}`, {
    method: "DELETE",
  });
}

export interface DocumentSearchResultDto {
  document_id: string;
  client_id: string;
  client_name: string;
  file_name: string;
  category: string;
  uploaded_at: string;
  relevance_snippet: string | null;
}

export interface PaginatedSearchResults {
  items: DocumentSearchResultDto[];
  total_count: number;
  page: number;
  page_size: number;
}

export interface SearchDocumentsParams {
  query: string;
  clientId?: string;
  category?: string;
  dateFrom?: string;
  dateTo?: string;
  page?: number;
  pageSize?: number;
}

export async function searchDocuments(
  params: SearchDocumentsParams,
): Promise<PaginatedSearchResults> {
  const searchParams = new URLSearchParams();

  if (params.page) searchParams.set("page", String(params.page));
  if (params.pageSize) searchParams.set("pageSize", String(params.pageSize));

  const queryString = searchParams.toString();
  const path = `/api/v1/Documents/Search${queryString ? `?${queryString}` : ""}`;

  const body: Record<string, unknown> = {
    query: params.query,
  };

  if (params.clientId) body.client_id = params.clientId;
  if (params.category) body.category = params.category;
  if (params.dateFrom) body.date_from = params.dateFrom;
  if (params.dateTo) body.date_to = params.dateTo;

  return apiFetch<PaginatedSearchResults>(path, {
    method: "POST",
    body: JSON.stringify(body),
    headers: { "Content-Type": "application/json" },
  });
}
