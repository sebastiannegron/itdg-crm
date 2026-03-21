import { apiFetch } from "./api-client";

export interface DraftEmailParams {
  client_name: string;
  topic: string;
  language: string;
  additional_context?: string;
}

export interface DraftEmailResponse {
  draft: string;
}

export async function generateDraftEmail(
  params: DraftEmailParams,
): Promise<DraftEmailResponse> {
  return apiFetch<DraftEmailResponse>("/api/v1/Ai/DraftEmail", {
    method: "POST",
    body: JSON.stringify(params),
  });
}
