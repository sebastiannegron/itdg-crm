"use client";

import { useState, useCallback, useTransition } from "react";
import { useLocale } from "next-intl";
import { PageHeader } from "@/app/_components/PageHeader";
import { Button } from "@/app/_components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/app/_components/ui/card";
import { Switch } from "@/app/_components/ui/switch";
import { fieldnames, type Locale } from "@/app/[locale]/_shared/app-fieldnames";
import type { PageStatus } from "@/app/[locale]/_shared/app-enums";
import {
  EVENT_TYPES,
  type NotificationPreferenceDto,
  type PreferenceRow,
  buildPreferenceRows,
  getEventTypeLabel,
  rowsToPreferences,
} from "./shared";
import { updateNotificationPreferencesAction } from "./actions";

interface NotificationPreferencesViewProps {
  initialPreferences: NotificationPreferenceDto[];
}

export default function NotificationPreferencesView({
  initialPreferences,
}: NotificationPreferencesViewProps) {
  const locale = useLocale() as Locale;
  const t = fieldnames[locale];

  const [rows, setRows] = useState<PreferenceRow[]>(
    buildPreferenceRows(initialPreferences),
  );
  const [status, setStatus] = useState<PageStatus>("idle");
  const [message, setMessage] = useState("");
  const [isPending, startTransition] = useTransition();

  const togglePreference = useCallback(
    (eventType: string, channel: "in_app" | "email") => {
      setRows((prev) =>
        prev.map((row) =>
          row.event_type === eventType
            ? { ...row, [channel]: !row[channel] }
            : row,
        ),
      );
    },
    [],
  );

  const handleSave = useCallback(() => {
    setStatus("loading");
    setMessage("");

    startTransition(async () => {
      const preferences = rowsToPreferences(rows);
      const result = await updateNotificationPreferencesAction(preferences);

      if (result.success) {
        setStatus("success");
        setMessage(t.settings_notifications_save_success);
      } else {
        setStatus("failed");
        setMessage(t.settings_notifications_save_error);
      }
    });
  }, [rows, t, startTransition]);

  return (
    <div className="space-y-6">
      <PageHeader
        title={t.settings_notifications_title}
        breadcrumbs={[
          { label: t.nav_settings, href: "/settings" },
          { label: t.settings_notifications_title },
        ]}
      />

      {status === "success" && message && (
        <div
          role="alert"
          className="rounded-md border border-green-200 bg-green-50 p-3 text-sm text-green-800 dark:border-green-800 dark:bg-green-950 dark:text-green-200"
        >
          {message}
        </div>
      )}

      {status === "failed" && message && (
        <div
          role="alert"
          className="rounded-md border border-red-200 bg-red-50 p-3 text-sm text-red-800 dark:border-red-800 dark:bg-red-950 dark:text-red-200"
        >
          {message}
        </div>
      )}

      <Card>
        <CardHeader>
          <CardTitle>{t.settings_notifications_title}</CardTitle>
          <CardDescription>
            {t.settings_notifications_subtitle}
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b">
                  <th className="py-3 pr-4 text-left font-medium text-muted-foreground">
                    {t.settings_notifications_event_type}
                  </th>
                  <th className="px-4 py-3 text-center font-medium text-muted-foreground">
                    {t.settings_notifications_in_app}
                  </th>
                  <th className="px-4 py-3 text-center font-medium text-muted-foreground">
                    {t.settings_notifications_email}
                  </th>
                </tr>
              </thead>
              <tbody>
                {EVENT_TYPES.map((eventType) => {
                  const row = rows.find((r) => r.event_type === eventType);
                  if (!row) return null;
                  return (
                    <tr
                      key={eventType}
                      className="border-b last:border-b-0"
                    >
                      <td className="py-3 pr-4 font-medium">
                        {getEventTypeLabel(eventType, locale)}
                      </td>
                      <td className="px-4 py-3 text-center">
                        <div className="flex justify-center">
                          <Switch
                            checked={row.in_app}
                            onCheckedChange={() =>
                              togglePreference(eventType, "in_app")
                            }
                            disabled={isPending}
                            aria-label={`${getEventTypeLabel(eventType, locale)} ${t.settings_notifications_in_app}`}
                          />
                        </div>
                      </td>
                      <td className="px-4 py-3 text-center">
                        <div className="flex justify-center">
                          <Switch
                            checked={row.email}
                            onCheckedChange={() =>
                              togglePreference(eventType, "email")
                            }
                            disabled={isPending}
                            aria-label={`${getEventTypeLabel(eventType, locale)} ${t.settings_notifications_email}`}
                          />
                        </div>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>

          <div className="mt-6 flex justify-end">
            <Button
              onClick={handleSave}
              disabled={isPending}
            >
              {isPending
                ? t.settings_notifications_saving
                : t.settings_notifications_save}
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
