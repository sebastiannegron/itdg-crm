import { apiFetch } from "./api-client";

export interface SendTemplateMessageParams {
  template_id: string;
  client_id: string;
  merge_fields: Record<string, string>;
  send_via_portal: boolean;
  send_via_email: boolean;
  recipient_email?: string;
}

export async function sendTemplateMessage(
  params: SendTemplateMessageParams,
): Promise<void> {
  await apiFetch<void>("/api/v1/Messages/SendTemplate", {
    method: "POST",
    body: JSON.stringify(params),
  });
}
