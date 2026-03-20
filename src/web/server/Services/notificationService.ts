import { apiFetch } from "./api-client";

export interface NotificationDto {
  notification_id: string;
  user_id: string;
  event_type: string;
  channel: string;
  title: string;
  body: string;
  status: string;
  delivered_at: string | null;
  read_at: string | null;
  created_at: string;
}

export interface PaginatedNotifications {
  items: NotificationDto[];
  total_count: number;
  page: number;
  page_size: number;
}

export async function getNotifications(
  page = 1,
  pageSize = 10,
  status?: string,
): Promise<PaginatedNotifications> {
  const params = new URLSearchParams({
    page: page.toString(),
    pageSize: pageSize.toString(),
  });
  if (status) {
    params.set("status", status);
  }
  return apiFetch<PaginatedNotifications>(
    `/api/v1/Notifications?${params.toString()}`,
  );
}

export async function getUnreadNotificationCount(): Promise<number> {
  return apiFetch<number>("/api/v1/Notifications/UnreadCount");
}

export async function markNotificationAsRead(
  notificationId: string,
): Promise<void> {
  await apiFetch<void>(`/api/v1/Notifications/${notificationId}/Read`, {
    method: "PUT",
  });
}

export async function markAllNotificationsAsRead(): Promise<void> {
  await apiFetch<void>("/api/v1/Notifications/ReadAll", {
    method: "PUT",
  });
}
