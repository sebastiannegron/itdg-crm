"use client";

import { useState, useRef, useEffect, useCallback } from "react";
import { Bell } from "lucide-react";
import { cn } from "@/lib/utils";
import { Button } from "@/app/_components/ui/button";
import { Badge } from "@/app/_components/ui/badge";
import {
  NotificationDot,
  type NotificationType,
} from "@/app/_components/NotificationDot";

export interface NotificationItem {
  id: string;
  type: NotificationType;
  message: string;
  timestamp: string;
  read: boolean;
}

interface NotificationPanelProps {
  notifications: NotificationItem[];
  onMarkAllRead: () => void;
  bellLabel: string;
  title: string;
  markAllReadLabel: string;
  emptyLabel: string;
}

export function NotificationPanel({
  notifications,
  onMarkAllRead,
  bellLabel,
  title,
  markAllReadLabel,
  emptyLabel,
}: NotificationPanelProps) {
  const [open, setOpen] = useState(false);
  const panelRef = useRef<HTMLDivElement>(null);

  const unreadCount = notifications.filter((n) => !n.read).length;

  const handleClickOutside = useCallback(
    (event: MouseEvent) => {
      if (
        panelRef.current &&
        !panelRef.current.contains(event.target as Node)
      ) {
        setOpen(false);
      }
    },
    []
  );

  useEffect(() => {
    if (open) {
      document.addEventListener("mousedown", handleClickOutside);
    }
    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
    };
  }, [open, handleClickOutside]);

  return (
    <div className="relative" ref={panelRef}>
      <Button
        variant="ghost"
        size="icon"
        className="relative"
        aria-label={bellLabel}
        aria-expanded={open}
        aria-haspopup="true"
        onClick={() => setOpen((prev) => !prev)}
      >
        <Bell className="h-5 w-5" />
        {unreadCount > 0 && (
          <Badge className="absolute -right-1 -top-1 flex h-5 min-w-5 items-center justify-center rounded-full px-1 text-xs">
            {unreadCount}
          </Badge>
        )}
      </Button>

      {open && (
        <div
          role="dialog"
          aria-label={title}
          className="absolute right-0 top-full z-50 mt-2 w-[300px] max-w-[calc(100vw-2rem)] rounded-md border border-border bg-card shadow-lg"
        >
          {/* Header */}
          <div className="flex items-center justify-between border-b border-border px-4 py-3">
            <h2 className="text-sm font-semibold text-foreground">{title}</h2>
            {unreadCount > 0 && (
              <button
                type="button"
                className="text-xs font-medium text-warning hover:text-warning/80 transition-colors"
                onClick={() => {
                  onMarkAllRead();
                }}
              >
                {markAllReadLabel}
              </button>
            )}
          </div>

          {/* Notification list */}
          <div className="max-h-[260px] overflow-y-auto">
            {notifications.length === 0 ? (
              <p className="px-4 py-6 text-center text-sm text-muted-foreground">
                {emptyLabel}
              </p>
            ) : (
              <ul role="list">
                {notifications.map((notification) => (
                  <li
                    key={notification.id}
                    className={cn(
                      "flex items-start gap-3 px-4 py-3 text-sm transition-colors",
                      !notification.read && "bg-[#FFF8F5]"
                    )}
                  >
                    <div className="mt-1.5 flex items-center gap-2">
                      <NotificationDot type={notification.type} />
                      {!notification.read && (
                        <span
                          className="h-1.5 w-1.5 shrink-0 rounded-full bg-warning"
                          aria-label="Unread"
                        />
                      )}
                    </div>
                    <div className="min-w-0 flex-1">
                      <p className="text-foreground">{notification.message}</p>
                      <p className="mt-0.5 text-xs text-muted-foreground">
                        {notification.timestamp}
                      </p>
                    </div>
                  </li>
                ))}
              </ul>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
