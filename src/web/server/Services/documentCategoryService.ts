import { apiFetch } from "./api-client";

export interface DocumentCategoryDto {
  category_id: string;
  name: string;
  naming_convention: string | null;
  is_default: boolean;
  sort_order: number;
  created_at: string;
  updated_at: string;
}

export interface CreateDocumentCategoryParams {
  name: string;
  naming_convention?: string | null;
  sort_order: number;
}

export interface UpdateDocumentCategoryParams {
  name: string;
  naming_convention?: string | null;
  sort_order: number;
}

export interface ReorderDocumentCategoriesParams {
  items: { category_id: string; sort_order: number }[];
}

export async function getDocumentCategories(): Promise<
  DocumentCategoryDto[]
> {
  return apiFetch<DocumentCategoryDto[]>("/api/v1/DocumentCategories");
}

export async function createDocumentCategory(
  params: CreateDocumentCategoryParams,
): Promise<void> {
  return apiFetch<void>("/api/v1/DocumentCategories", {
    method: "POST",
    body: JSON.stringify(params),
  });
}

export async function updateDocumentCategory(
  categoryId: string,
  params: UpdateDocumentCategoryParams,
): Promise<void> {
  return apiFetch<void>(`/api/v1/DocumentCategories/${categoryId}`, {
    method: "PUT",
    body: JSON.stringify(params),
  });
}

export async function deleteDocumentCategory(
  categoryId: string,
): Promise<void> {
  return apiFetch<void>(`/api/v1/DocumentCategories/${categoryId}`, {
    method: "DELETE",
  });
}

export async function reorderDocumentCategories(
  params: ReorderDocumentCategoriesParams,
): Promise<void> {
  return apiFetch<void>("/api/v1/DocumentCategories/Reorder", {
    method: "PUT",
    body: JSON.stringify(params),
  });
}
