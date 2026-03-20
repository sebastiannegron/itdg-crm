"use client";

import { useState, useCallback, useTransition } from "react";
import { useLocale } from "next-intl";
import {
  Users,
  CheckSquare,
  Calendar,
  AlertTriangle,
  RefreshCw,
} from "lucide-react";
import { PageHeader } from "@/app/_components/PageHeader";
import { StatusBadge } from "@/app/_components/StatusBadge";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/app/_components/ui/card";
import { Button } from "@/app/_components/ui/button";
import { fieldnames, type Locale } from "@/app/[locale]/_shared/app-fieldnames";
import type { PageStatus } from "@/app/[locale]/_shared/app-enums";
import type {
  DashboardSummaryDto,
  TaskItem,
  EscalationItem,
  DeadlineItem,
} from "./shared";
import { getDashboardSummaryAction } from "./actions";

interface DashboardViewProps {
  initialSummary: DashboardSummaryDto | null;
  initialTasks: TaskItem[];
  initialEscalations: EscalationItem[];
  initialDeadlines: DeadlineItem[];
}

export default function DashboardView({
  initialSummary,
  initialTasks,
  initialEscalations,
  initialDeadlines,
}: DashboardViewProps) {
  const locale = useLocale() as Locale;
  const t = fieldnames[locale];

  const [summary, setSummary] = useState<DashboardSummaryDto | null>(
    initialSummary,
  );
  const [tasks] = useState<TaskItem[]>(initialTasks);
  const [escalations] = useState<EscalationItem[]>(initialEscalations);
  const [deadlines] = useState<DeadlineItem[]>(initialDeadlines);
  const [status, setStatus] = useState<PageStatus>("idle");
  const [errorMessage, setErrorMessage] = useState("");
  const [isPending, startTransition] = useTransition();

  const handleRefresh = useCallback(() => {
    setStatus("loading");
    setErrorMessage("");

    startTransition(async () => {
      const result = await getDashboardSummaryAction();

      if (result.success && result.data) {
        setSummary(result.data);
        setStatus("success");
      } else {
        setStatus("failed");
        setErrorMessage(result.message || t.dashboard_refresh_error);
      }
    });
  }, [t, startTransition]);

  const statCards = [
    {
      label: t.dashboard_total_clients,
      subtitle: t.dashboard_total_clients_subtitle,
      value: summary?.total_clients ?? 0,
      accentColor: "bg-blue-500",
      icon: Users,
    },
    {
      label: t.dashboard_open_tasks,
      subtitle: t.dashboard_open_tasks_subtitle,
      value: summary?.pending_tasks_count ?? 0,
      accentColor: "bg-orange-500",
      icon: CheckSquare,
    },
    {
      label: t.dashboard_upcoming_deadlines,
      subtitle: t.dashboard_upcoming_deadlines_subtitle,
      value: summary?.upcoming_deadlines_count ?? 0,
      accentColor: "bg-green-500",
      icon: Calendar,
    },
    {
      label: t.dashboard_escalated,
      subtitle: t.dashboard_escalated_subtitle,
      value: summary?.recent_escalations_count ?? 0,
      accentColor: "bg-red-500",
      icon: AlertTriangle,
    },
  ];

  const priorityDotColor: Record<string, string> = {
    High: "bg-red-500",
    Medium: "bg-amber-500",
    Low: "bg-gray-400",
  };

  return (
    <div className="space-y-6 p-6">
      <PageHeader
        title={t.dashboard_title}
        actions={
          <Button
            variant="outline"
            size="sm"
            onClick={handleRefresh}
            disabled={isPending || status === "loading"}
            aria-label="Refresh dashboard"
          >
            <RefreshCw
              className={`h-4 w-4 ${isPending || status === "loading" ? "animate-spin" : ""}`}
            />
          </Button>
        }
      />

      {errorMessage && (
        <div className="rounded-md border border-red-200 bg-red-50 p-3 text-sm text-red-800">
          {errorMessage}
        </div>
      )}

      {/* Stat Cards Row — 2x2 on mobile, 4 cols on desktop */}
      <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
        {statCards.map((card) => (
          <Card key={card.label} className="relative overflow-hidden">
            <div
              className={`absolute left-0 top-0 h-full w-[3px] ${card.accentColor}`}
            />
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2 pl-5">
              <CardTitle className="text-xs font-medium text-muted-foreground">
                {card.label}
              </CardTitle>
              <card.icon className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent className="pl-5">
              <div className="text-2xl font-bold">{card.value}</div>
              <p className="text-xs text-muted-foreground">{card.subtitle}</p>
            </CardContent>
          </Card>
        ))}
      </div>

      {/* Main Content — responsive grid: 1 col mobile, 2 tablet, 3-4 desktop */}
      <div className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-3">
        {/* Pending Tasks — spans 2 cols on desktop */}
        <Card className="lg:col-span-2">
          <CardHeader className="flex flex-row items-center justify-between space-y-0">
            <CardTitle className="text-base font-semibold">
              {t.dashboard_pending_tasks}
            </CardTitle>
            <span className="cursor-pointer text-xs font-medium text-orange-600 hover:text-orange-700">
              {t.dashboard_view_all}
            </span>
          </CardHeader>
          <CardContent>
            {tasks.length === 0 ? (
              <p className="text-sm text-muted-foreground">
                {t.dashboard_no_tasks}
              </p>
            ) : (
              <div className="space-y-3">
                {tasks.map((task) => (
                  <div
                    key={task.id}
                    className="flex items-center justify-between rounded-lg border border-border p-3"
                  >
                    <div className="flex items-center gap-3">
                      <span
                        className={`h-2 w-2 rounded-full ${priorityDotColor[task.priority] ?? "bg-gray-400"}`}
                        title={task.priority}
                      />
                      <div>
                        <p className="text-sm font-medium text-foreground">
                          {task.title}
                        </p>
                        <p className="text-xs text-muted-foreground">
                          {t.dashboard_due}: {task.due_date}
                        </p>
                      </div>
                    </div>
                    <StatusBadge status={task.status} />
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>

        {/* Right Column — Escalations + Deadlines */}
        <div className="space-y-6">
          {/* Escalated Issues Panel */}
          <Card>
            <CardHeader className="space-y-0">
              <CardTitle className="text-base font-semibold text-red-700">
                {t.dashboard_escalated_issues}
              </CardTitle>
            </CardHeader>
            <CardContent>
              {escalations.length === 0 ? (
                <p className="text-sm text-muted-foreground">
                  {t.dashboard_no_escalations}
                </p>
              ) : (
                <div className="space-y-2">
                  {escalations.map((item) => (
                    <div
                      key={item.id}
                      className="rounded-lg border border-[#FECACA] bg-[#FEF2F2] p-3"
                    >
                      <p className="text-sm font-medium text-red-800">
                        {item.title}
                      </p>
                      <p className="text-xs text-red-600">
                        {item.client_name}
                      </p>
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>

          {/* Upcoming Deadlines Panel */}
          <Card>
            <CardHeader className="space-y-0">
              <CardTitle className="text-base font-semibold">
                {t.dashboard_upcoming_deadlines_panel}
              </CardTitle>
            </CardHeader>
            <CardContent>
              {deadlines.length === 0 ? (
                <p className="text-sm text-muted-foreground">
                  {t.dashboard_no_deadlines}
                </p>
              ) : (
                <div className="space-y-2">
                  {deadlines.map((item) => (
                    <div
                      key={item.id}
                      className="flex items-center gap-3 rounded-lg border border-orange-200 bg-[#FFF8F5] p-3"
                    >
                      <span className="inline-flex items-center rounded-md bg-orange-100 px-2 py-1 text-xs font-semibold text-orange-700">
                        {item.due_date}
                      </span>
                      <p className="text-sm font-medium text-foreground">
                        {item.title}
                      </p>
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
