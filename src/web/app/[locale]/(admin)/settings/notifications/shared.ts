import { fieldnames, type Locale } from "@/app/[locale]/_shared/app-fieldnames";

export const EVENT_TYPES = [
  "DocumentUploaded",
  "PaymentCompleted",
  "PaymentFailed",
  "TaskAssigned",
  "TaskDueSoon",
  "EscalationReceived",
  "PortalMessageReceived",
  "SystemAlert",
] as const;

export type EventType = (typeof EVENT_TYPES)[number];

export const CHANNELS = ["InApp", "Email"] as const;

export type Channel = (typeof CHANNELS)[number];

export interface PreferenceRow {
  event_type: EventType;
  in_app: boolean;
  email: boolean;
}

export interface NotificationPreferenceDto {
  preference_id: string;
  event_type: string;
  channel: string;
  is_enabled: boolean;
  digest_mode: string;
}

export function getEventTypeLabel(eventType: EventType, locale: Locale): string {
  const t = fieldnames[locale];
  const map: Record<EventType, string> = {
    DocumentUploaded: t.settings_notifications_event_document_uploaded,
    PaymentCompleted: t.settings_notifications_event_payment_completed,
    PaymentFailed: t.settings_notifications_event_payment_failed,
    TaskAssigned: t.settings_notifications_event_task_assigned,
    TaskDueSoon: t.settings_notifications_event_task_due_soon,
    EscalationReceived: t.settings_notifications_event_escalation_received,
    PortalMessageReceived:
      t.settings_notifications_event_portal_message_received,
    SystemAlert: t.settings_notifications_event_system_alert,
  };
  return map[eventType];
}

export function buildPreferenceRows(
  preferences: NotificationPreferenceDto[],
): PreferenceRow[] {
  return EVENT_TYPES.map((eventType) => {
    const inApp = preferences.find(
      (p) => p.event_type === eventType && p.channel === "InApp",
    );
    const email = preferences.find(
      (p) => p.event_type === eventType && p.channel === "Email",
    );
    return {
      event_type: eventType,
      in_app: inApp?.is_enabled ?? true,
      email: email?.is_enabled ?? true,
    };
  });
}

export function rowsToPreferences(
  rows: PreferenceRow[],
): { event_type: string; channel: string; is_enabled: boolean; digest_mode: string }[] {
  return rows.flatMap((row) => [
    {
      event_type: row.event_type,
      channel: "InApp",
      is_enabled: row.in_app,
      digest_mode: "Immediate",
    },
    {
      event_type: row.event_type,
      channel: "Email",
      is_enabled: row.email,
      digest_mode: "Immediate",
    },
  ]);
}
