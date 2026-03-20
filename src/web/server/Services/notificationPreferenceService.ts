import { apiFetch } from "./api-client";

export interface NotificationPreferenceDto {
  preference_id: string;
  event_type: string;
  channel: string;
  is_enabled: boolean;
  digest_mode: string;
}

export interface UpdateNotificationPreferencesParams {
  preferences: {
    event_type: string;
    channel: string;
    is_enabled: boolean;
    digest_mode: string;
  }[];
}

export async function getNotificationPreferences(): Promise<
  NotificationPreferenceDto[]
> {
  return apiFetch<NotificationPreferenceDto[]>(
    "/api/v1/Notifications/Preferences",
  );
}

export async function updateNotificationPreferences(
  params: UpdateNotificationPreferencesParams,
): Promise<void> {
  await apiFetch<void>("/api/v1/Notifications/Preferences", {
    method: "PUT",
    body: JSON.stringify(params),
  });
}
