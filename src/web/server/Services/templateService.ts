import { apiFetch } from "./api-client";

export interface CommunicationTemplateDto {
  id: string;
  category: number;
  name: string;
  subject_template: string;
  body_template: string;
  language: string;
  version: number;
  is_active: boolean;
  created_by_id: string;
  created_at: string;
  updated_at: string;
}

export interface RenderedTemplateDto {
  subject: string;
  body: string;
}

export interface CreateTemplateParams {
  category: number;
  name: string;
  subject_template: string;
  body_template: string;
  language: string;
}

export interface UpdateTemplateParams {
  category: number;
  name: string;
  subject_template: string;
  body_template: string;
  language: string;
}

export async function getTemplates(): Promise<CommunicationTemplateDto[]> {
  return apiFetch<CommunicationTemplateDto[]>("/api/v1/Templates");
}

export async function getTemplateById(
  id: string,
): Promise<CommunicationTemplateDto> {
  return apiFetch<CommunicationTemplateDto>(`/api/v1/Templates/${id}`);
}

export async function createTemplate(
  params: CreateTemplateParams,
): Promise<void> {
  await apiFetch<void>("/api/v1/Templates", {
    method: "POST",
    body: JSON.stringify(params),
  });
}

export async function updateTemplate(
  id: string,
  params: UpdateTemplateParams,
): Promise<void> {
  await apiFetch<void>(`/api/v1/Templates/${id}`, {
    method: "PUT",
    body: JSON.stringify(params),
  });
}

export async function retireTemplate(id: string): Promise<void> {
  await apiFetch<void>(`/api/v1/Templates/${id}`, {
    method: "DELETE",
  });
}

export async function renderTemplate(
  id: string,
  mergeFields: Record<string, string>,
): Promise<RenderedTemplateDto> {
  return apiFetch<RenderedTemplateDto>(`/api/v1/Templates/${id}/Render`, {
    method: "POST",
    body: JSON.stringify({ merge_fields: mergeFields }),
  });
}
