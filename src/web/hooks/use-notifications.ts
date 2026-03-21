"use client";

import { useState, useEffect, useRef, useCallback } from "react";
import {
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
  type HubConnection,
} from "@microsoft/signalr";
import type { NotificationItem } from "@/app/_components/NotificationPanel";
import type { NotificationType } from "@/app/_components/NotificationDot";

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

const eventTypeMap: Record<string, NotificationType> = {
  DocumentUploaded: "document",
  PaymentCompleted: "payment",
  PaymentFailed: "payment",
  TaskAssigned: "task",
  TaskDueSoon: "task",
  EscalationReceived: "escalation",
  PortalMessageReceived: "message",
  SystemAlert: "system",
};

function toNotificationItem(dto: NotificationDto): NotificationItem {
  return {
    id: dto.notification_id,
    type: eventTypeMap[dto.event_type] ?? "system",
    message: dto.body,
    timestamp: new Date(dto.created_at).toLocaleString(),
    read: dto.read_at !== null,
  };
}

interface UseNotificationsOptions {
  /** Initial notification items to populate state with */
  initialNotifications?: NotificationItem[];
  /** Initial unread count */
  initialUnreadCount?: number;
  /** The SignalR hub URL. Defaults to NEXT_PUBLIC_API_BASE_URL + /hubs/notifications */
  hubUrl?: string;
}

interface UseNotificationsReturn {
  notifications: NotificationItem[];
  unreadCount: number;
  setNotifications: React.Dispatch<React.SetStateAction<NotificationItem[]>>;
  setUnreadCount: React.Dispatch<React.SetStateAction<number>>;
  connectionState: HubConnectionState;
}

export function useNotifications(
  options: UseNotificationsOptions = {},
): UseNotificationsReturn {
  const {
    initialNotifications = [],
    initialUnreadCount = 0,
    hubUrl,
  } = options;

  const [notifications, setNotifications] =
    useState<NotificationItem[]>(initialNotifications);
  const [unreadCount, setUnreadCount] = useState(initialUnreadCount);
  const [connectionState, setConnectionState] = useState<HubConnectionState>(
    HubConnectionState.Disconnected,
  );
  const connectionRef = useRef<HubConnection | null>(null);

  const handleReceiveNotification = useCallback((dto: NotificationDto) => {
    const item = toNotificationItem(dto);
    setNotifications((prev) => [item, ...prev]);
  }, []);

  const handleUnreadCountUpdated = useCallback((count: number) => {
    setUnreadCount(count);
  }, []);

  useEffect(() => {
    const baseUrl =
      hubUrl ??
      `${process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5000"}/hubs/notifications`;

    const connection = new HubConnectionBuilder()
      .withUrl(baseUrl)
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();

    connectionRef.current = connection;

    connection.on("ReceiveNotification", handleReceiveNotification);
    connection.on("UnreadCountUpdated", handleUnreadCountUpdated);

    connection.onreconnecting(() => {
      setConnectionState(HubConnectionState.Reconnecting);
    });

    connection.onreconnected(() => {
      setConnectionState(HubConnectionState.Connected);
    });

    connection.onclose(() => {
      setConnectionState(HubConnectionState.Disconnected);
    });

    connection
      .start()
      .then(() => {
        setConnectionState(HubConnectionState.Connected);
      })
      .catch(() => {
        setConnectionState(HubConnectionState.Disconnected);
      });

    return () => {
      connection.off("ReceiveNotification", handleReceiveNotification);
      connection.off("UnreadCountUpdated", handleUnreadCountUpdated);
      connection.stop();
    };
  }, [hubUrl, handleReceiveNotification, handleUnreadCountUpdated]);

  return {
    notifications,
    unreadCount,
    setNotifications,
    setUnreadCount,
    connectionState,
  };
}
