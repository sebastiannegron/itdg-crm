import { apiFetch } from "./api-client";

export interface MessageDto {
  id: string;
  client_id: string;
  sender_id: string;
  direction: string;
  subject: string;
  body: string;
  template_id: string | null;
  is_portal_message: boolean;
  is_read: boolean;
  attachments: string | null;
  created_at: string;
}

export async function getPortalMessages(): Promise<MessageDto[]> {
  return apiFetch<MessageDto[]>("/api/v1/Portal/Messages");
}

export async function sendPortalMessage(
  subject: string,
  body: string,
): Promise<void> {
  await apiFetch<void>("/api/v1/Portal/Messages", {
    method: "POST",
    body: JSON.stringify({ subject, body }),
  });
}

export async function markMessageAsRead(messageId: string): Promise<void> {
  await apiFetch<void>(`/api/v1/Portal/Messages/${messageId}/Read`, {
    method: "PUT",
  });
}
