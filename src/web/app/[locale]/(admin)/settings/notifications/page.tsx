import { getNotificationPreferences } from "@/server/Services/notificationPreferenceService";
import NotificationPreferencesView from "./NotificationPreferencesView";
import type { NotificationPreferenceDto } from "./shared";

export default async function NotificationPreferencesPage() {
  let preferences: NotificationPreferenceDto[];

  try {
    preferences = await getNotificationPreferences();
  } catch (error) {
    console.error(
      "[NotificationPreferencesPage] Failed to fetch preferences:",
      error,
    );
    preferences = [];
  }

  return <NotificationPreferencesView initialPreferences={preferences} />;
}
