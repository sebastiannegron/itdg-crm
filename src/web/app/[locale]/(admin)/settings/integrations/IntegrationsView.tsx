"use client";

import { useState, useCallback, useTransition } from "react";
import { useLocale } from "next-intl";
import {
  Mail,
  Calendar,
  HardDrive,
  Send,
  BrainCircuit,
  CheckCircle2,
  XCircle,
  ExternalLink,
} from "lucide-react";
import { PageHeader } from "@/app/_components/PageHeader";
import { Button } from "@/app/_components/ui/button";
import { Badge } from "@/app/_components/ui/badge";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/app/_components/ui/card";
import { fieldnames, type Locale } from "@/app/[locale]/_shared/app-fieldnames";
import type { PageStatus } from "@/app/[locale]/_shared/app-enums";
import type {
  GoogleConnectionStatusDto,
  GmailConnectionStatusDto,
  CalendarConnectionStatusDto,
  MsGraphConnectionStatusDto,
  AzureOpenAiConnectionStatusDto,
} from "./shared";
import {
  disconnectGoogleAction,
  disconnectGmailAction,
} from "./actions";
import {
  getGoogleAuthUrl,
  getGmailAuthUrl,
} from "@/server/Services/integrationService";

interface IntegrationsViewProps {
  initialGoogleDrive: GoogleConnectionStatusDto;
  initialGmail: GmailConnectionStatusDto;
  initialCalendar: CalendarConnectionStatusDto;
  initialMsGraph: MsGraphConnectionStatusDto;
  initialAzureOpenAi: AzureOpenAiConnectionStatusDto;
}

function formatConnectedDate(dateStr: string | null, locale: Locale): string {
  if (!dateStr) return "";
  try {
    const date = new Date(dateStr);
    return date.toLocaleDateString(locale === "es-pr" ? "es-PR" : "en-PR", {
      year: "numeric",
      month: "short",
      day: "numeric",
      hour: "2-digit",
      minute: "2-digit",
      timeZone: "America/Puerto_Rico",
    });
  } catch {
    return dateStr;
  }
}

export default function IntegrationsView({
  initialGoogleDrive,
  initialGmail,
  initialCalendar,
  initialMsGraph,
  initialAzureOpenAi,
}: IntegrationsViewProps) {
  const locale = useLocale() as Locale;
  const t = fieldnames[locale];

  const [googleDrive, setGoogleDrive] =
    useState<GoogleConnectionStatusDto>(initialGoogleDrive);
  const [gmail, setGmail] =
    useState<GmailConnectionStatusDto>(initialGmail);

  const calendar = initialCalendar;
  const msGraph = initialMsGraph;
  const azureOpenAi = initialAzureOpenAi;

  const [status, setStatus] = useState<PageStatus>("idle");
  const [message, setMessage] = useState("");
  const [loadingService, setLoadingService] = useState<string | null>(null);
  const [isPending, startTransition] = useTransition();

  const handleDisconnectGoogle = useCallback(() => {
    if (!confirm(t.settings_integrations_disconnect_confirm)) return;
    setLoadingService("drive");
    setStatus("loading");
    setMessage("");

    startTransition(async () => {
      const result = await disconnectGoogleAction();
      if (result.success) {
        setGoogleDrive({ is_connected: false, connected_at: null });
        setStatus("success");
        setMessage(t.settings_integrations_disconnect_success);
      } else {
        setStatus("failed");
        setMessage(t.settings_integrations_disconnect_error);
      }
      setLoadingService(null);
    });
  }, [t, startTransition]);

  const handleDisconnectGmail = useCallback(() => {
    if (!confirm(t.settings_integrations_disconnect_confirm)) return;
    setLoadingService("gmail");
    setStatus("loading");
    setMessage("");

    startTransition(async () => {
      const result = await disconnectGmailAction();
      if (result.success) {
        setGmail({ is_connected: false, connected_at: null });
        setStatus("success");
        setMessage(t.settings_integrations_disconnect_success);
      } else {
        setStatus("failed");
        setMessage(t.settings_integrations_disconnect_error);
      }
      setLoadingService(null);
    });
  }, [t, startTransition]);

  const handleConnectGoogle = useCallback(() => {
    const authUrl = getGoogleAuthUrl();
    window.location.href = authUrl;
  }, []);

  const handleConnectGmail = useCallback(() => {
    const authUrl = getGmailAuthUrl();
    window.location.href = authUrl;
  }, []);

  return (
    <div className="space-y-6">
      <PageHeader
        title={t.settings_integrations_title}
        breadcrumbs={[
          { label: t.nav_settings, href: "/settings" },
          { label: t.settings_integrations_title },
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

      <p className="text-sm text-muted-foreground">
        {t.settings_integrations_subtitle}
      </p>

      {/* Google Workspace Section */}
      <div className="space-y-4">
        <h2 className="text-lg font-semibold text-foreground">
          {t.settings_integrations_google_workspace}
        </h2>
        <p className="text-sm text-muted-foreground">
          {t.settings_integrations_google_workspace_description}
        </p>

        <div className="grid gap-4 sm:grid-cols-1 md:grid-cols-3">
          {/* Gmail Card */}
          <Card>
            <CardHeader className="pb-3">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <Mail className="h-5 w-5 text-muted-foreground" />
                  <CardTitle>{t.settings_integrations_gmail}</CardTitle>
                </div>
                <Badge
                  variant={gmail.is_connected ? "default" : "secondary"}
                  className={
                    gmail.is_connected
                      ? "bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200"
                      : ""
                  }
                >
                  {gmail.is_connected ? (
                    <CheckCircle2 className="mr-1 h-3 w-3" />
                  ) : (
                    <XCircle className="mr-1 h-3 w-3" />
                  )}
                  {gmail.is_connected
                    ? t.settings_integrations_connected
                    : t.settings_integrations_not_connected}
                </Badge>
              </div>
              <CardDescription>
                {t.settings_integrations_gmail_description}
              </CardDescription>
            </CardHeader>
            <CardContent>
              {gmail.is_connected && gmail.connected_at && (
                <p className="mb-3 text-xs text-muted-foreground">
                  {t.settings_integrations_connected_at}{" "}
                  {formatConnectedDate(gmail.connected_at, locale)}
                </p>
              )}
              {gmail.is_connected ? (
                <Button
                  variant="outline"
                  size="sm"
                  onClick={handleDisconnectGmail}
                  disabled={isPending && loadingService === "gmail"}
                >
                  {isPending && loadingService === "gmail"
                    ? t.settings_integrations_disconnecting
                    : t.settings_integrations_disconnect}
                </Button>
              ) : (
                <Button
                  size="sm"
                  onClick={handleConnectGmail}
                  disabled={isPending}
                >
                  <ExternalLink className="mr-1 h-3 w-3" />
                  {t.settings_integrations_connect}
                </Button>
              )}
            </CardContent>
          </Card>

          {/* Calendar Card */}
          <Card>
            <CardHeader className="pb-3">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <Calendar className="h-5 w-5 text-muted-foreground" />
                  <CardTitle>{t.settings_integrations_calendar}</CardTitle>
                </div>
                <Badge
                  variant={calendar.is_connected ? "default" : "secondary"}
                  className={
                    calendar.is_connected
                      ? "bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200"
                      : ""
                  }
                >
                  {calendar.is_connected ? (
                    <CheckCircle2 className="mr-1 h-3 w-3" />
                  ) : (
                    <XCircle className="mr-1 h-3 w-3" />
                  )}
                  {calendar.is_connected
                    ? t.settings_integrations_connected
                    : t.settings_integrations_not_connected}
                </Badge>
              </div>
              <CardDescription>
                {t.settings_integrations_calendar_description}
              </CardDescription>
            </CardHeader>
            <CardContent>
              {calendar.is_connected && calendar.connected_at && (
                <p className="mb-3 text-xs text-muted-foreground">
                  {t.settings_integrations_connected_at}{" "}
                  {formatConnectedDate(calendar.connected_at, locale)}
                </p>
              )}
              {!calendar.is_connected && (
                <p className="text-xs text-muted-foreground">
                  {t.settings_integrations_not_connected}
                </p>
              )}
            </CardContent>
          </Card>

          {/* Drive Card */}
          <Card>
            <CardHeader className="pb-3">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <HardDrive className="h-5 w-5 text-muted-foreground" />
                  <CardTitle>{t.settings_integrations_drive}</CardTitle>
                </div>
                <Badge
                  variant={googleDrive.is_connected ? "default" : "secondary"}
                  className={
                    googleDrive.is_connected
                      ? "bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200"
                      : ""
                  }
                >
                  {googleDrive.is_connected ? (
                    <CheckCircle2 className="mr-1 h-3 w-3" />
                  ) : (
                    <XCircle className="mr-1 h-3 w-3" />
                  )}
                  {googleDrive.is_connected
                    ? t.settings_integrations_connected
                    : t.settings_integrations_not_connected}
                </Badge>
              </div>
              <CardDescription>
                {t.settings_integrations_drive_description}
              </CardDescription>
            </CardHeader>
            <CardContent>
              {googleDrive.is_connected && googleDrive.connected_at && (
                <p className="mb-3 text-xs text-muted-foreground">
                  {t.settings_integrations_connected_at}{" "}
                  {formatConnectedDate(googleDrive.connected_at, locale)}
                </p>
              )}
              {googleDrive.is_connected ? (
                <Button
                  variant="outline"
                  size="sm"
                  onClick={handleDisconnectGoogle}
                  disabled={isPending && loadingService === "drive"}
                >
                  {isPending && loadingService === "drive"
                    ? t.settings_integrations_disconnecting
                    : t.settings_integrations_disconnect}
                </Button>
              ) : (
                <Button
                  size="sm"
                  onClick={handleConnectGoogle}
                  disabled={isPending}
                >
                  <ExternalLink className="mr-1 h-3 w-3" />
                  {t.settings_integrations_connect}
                </Button>
              )}
            </CardContent>
          </Card>
        </div>
      </div>

      {/* System Services Section */}
      <div className="space-y-4">
        <h2 className="text-lg font-semibold text-foreground">
          {t.settings_integrations_system_service}
        </h2>
        <p className="text-sm text-muted-foreground">
          {t.settings_integrations_system_service_description}
        </p>

        <div className="grid gap-4 sm:grid-cols-1 md:grid-cols-2">
          {/* Microsoft Graph Card */}
          <Card>
            <CardHeader className="pb-3">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <Send className="h-5 w-5 text-muted-foreground" />
                  <CardTitle>{t.settings_integrations_ms_graph}</CardTitle>
                </div>
                <Badge
                  variant={msGraph.is_configured ? "default" : "secondary"}
                  className={
                    msGraph.is_configured
                      ? "bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200"
                      : ""
                  }
                >
                  {msGraph.is_configured ? (
                    <CheckCircle2 className="mr-1 h-3 w-3" />
                  ) : (
                    <XCircle className="mr-1 h-3 w-3" />
                  )}
                  {msGraph.is_configured
                    ? t.settings_integrations_configured
                    : t.settings_integrations_not_configured}
                </Badge>
              </div>
              <CardDescription>
                {t.settings_integrations_ms_graph_description}
              </CardDescription>
            </CardHeader>
          </Card>

          {/* Azure OpenAI Card */}
          <Card>
            <CardHeader className="pb-3">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <BrainCircuit className="h-5 w-5 text-muted-foreground" />
                  <CardTitle>
                    {t.settings_integrations_azure_openai}
                  </CardTitle>
                </div>
                <Badge
                  variant={azureOpenAi.is_configured ? "default" : "secondary"}
                  className={
                    azureOpenAi.is_configured
                      ? "bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200"
                      : ""
                  }
                >
                  {azureOpenAi.is_configured ? (
                    <CheckCircle2 className="mr-1 h-3 w-3" />
                  ) : (
                    <XCircle className="mr-1 h-3 w-3" />
                  )}
                  {azureOpenAi.is_configured
                    ? t.settings_integrations_configured
                    : t.settings_integrations_not_configured}
                </Badge>
              </div>
              <CardDescription>
                {t.settings_integrations_azure_openai_description}
              </CardDescription>
            </CardHeader>
          </Card>
        </div>
      </div>
    </div>
  );
}
