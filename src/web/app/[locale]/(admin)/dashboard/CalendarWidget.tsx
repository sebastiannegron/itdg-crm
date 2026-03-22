"use client";

import { useState, useCallback, useTransition, useMemo } from "react";
import { useLocale } from "next-intl";
import { Calendar, ChevronLeft, ChevronRight, Clock } from "lucide-react";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/app/_components/ui/card";
import { Button } from "@/app/_components/ui/button";
import { fieldnames, type Locale } from "@/app/[locale]/_shared/app-fieldnames";
import type {
  DashboardCalendarDto,
  DashboardCalendarEventDto,
} from "./shared";
import { getDashboardCalendarAction } from "./actions";

interface CalendarWidgetProps {
  initialCalendar: DashboardCalendarDto | null;
}

function formatTime(dateStr: string | null): string {
  if (!dateStr) return "";
  const date = new Date(dateStr);
  return date.toLocaleTimeString("en-US", {
    hour: "numeric",
    minute: "2-digit",
    timeZone: "America/Puerto_Rico",
  });
}

function formatDate(date: Date): string {
  return date.toLocaleDateString("en-US", {
    weekday: "short",
    month: "short",
    day: "numeric",
    timeZone: "America/Puerto_Rico",
  });
}

function isSameDay(dateStr: string | null, targetDate: Date): boolean {
  if (!dateStr) return false;
  const date = new Date(dateStr);
  const prFormatter = new Intl.DateTimeFormat("en-US", {
    timeZone: "America/Puerto_Rico",
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
  });
  const eventParts = prFormatter.formatToParts(date);
  const targetParts = prFormatter.formatToParts(targetDate);

  const getVal = (parts: Intl.DateTimeFormatPart[], type: string) =>
    parts.find((p) => p.type === type)?.value;

  return (
    getVal(eventParts, "year") === getVal(targetParts, "year") &&
    getVal(eventParts, "month") === getVal(targetParts, "month") &&
    getVal(eventParts, "day") === getVal(targetParts, "day")
  );
}

function getWeekStart(date: Date): Date {
  const d = new Date(date);
  const day = d.getDay();
  d.setDate(d.getDate() - day);
  d.setHours(0, 0, 0, 0);
  return d;
}

function getWeekDays(weekStart: Date): Date[] {
  return Array.from({ length: 7 }, (_, i) => {
    const d = new Date(weekStart);
    d.setDate(d.getDate() + i);
    return d;
  });
}

function isAllDay(event: DashboardCalendarEventDto): boolean {
  if (!event.start || !event.end) return false;
  const start = new Date(event.start);
  const end = new Date(event.end);
  const diffMs = end.getTime() - start.getTime();
  return diffMs >= 24 * 60 * 60 * 1000 - 1000;
}

export default function CalendarWidget({
  initialCalendar,
}: CalendarWidgetProps) {
  const locale = useLocale() as Locale;
  const t = fieldnames[locale];

  const [calendar, setCalendar] = useState<DashboardCalendarDto | null>(
    initialCalendar,
  );
  const [currentDate, setCurrentDate] = useState<Date>(new Date());
  const [isPending, startTransition] = useTransition();

  const weekStart = useMemo(() => getWeekStart(currentDate), [currentDate]);
  const weekDays = useMemo(() => getWeekDays(weekStart), [weekStart]);

  const todayStr = useMemo(() => {
    const now = new Date();
    return new Intl.DateTimeFormat("en-US", {
      timeZone: "America/Puerto_Rico",
      year: "numeric",
      month: "2-digit",
      day: "2-digit",
    }).format(now);
  }, []);

  const goToToday = useCallback(() => {
    setCurrentDate(new Date());
  }, []);

  const goToPrevWeek = useCallback(() => {
    setCurrentDate((prev) => {
      const d = new Date(prev);
      d.setDate(d.getDate() - 7);
      return d;
    });
  }, []);

  const goToNextWeek = useCallback(() => {
    setCurrentDate((prev) => {
      const d = new Date(prev);
      d.setDate(d.getDate() + 7);
      return d;
    });
  }, []);

  const fetchCalendar = useCallback(
    (start: Date, end: Date) => {
      startTransition(async () => {
        const result = await getDashboardCalendarAction(
          start.toISOString(),
          end.toISOString(),
        );
        if (result.success && result.data) {
          setCalendar(result.data);
        }
      });
    },
    [startTransition],
  );

  const handlePrevWeek = useCallback(() => {
    goToPrevWeek();
    const newStart = new Date(weekStart);
    newStart.setDate(newStart.getDate() - 7);
    const newEnd = new Date(newStart);
    newEnd.setDate(newEnd.getDate() + 7);
    fetchCalendar(newStart, newEnd);
  }, [goToPrevWeek, weekStart, fetchCalendar]);

  const handleNextWeek = useCallback(() => {
    goToNextWeek();
    const newStart = new Date(weekStart);
    newStart.setDate(newStart.getDate() + 7);
    const newEnd = new Date(newStart);
    newEnd.setDate(newEnd.getDate() + 7);
    fetchCalendar(newStart, newEnd);
  }, [goToNextWeek, weekStart, fetchCalendar]);

  const handleToday = useCallback(() => {
    goToToday();
    const today = new Date();
    const start = getWeekStart(today);
    const end = new Date(start);
    end.setDate(end.getDate() + 7);
    fetchCalendar(start, end);
  }, [goToToday, fetchCalendar]);

  const events = calendar?.events ?? [];
  const teamMembers = calendar?.team_members ?? [];

  const isToday = (date: Date): boolean => {
    const formatter = new Intl.DateTimeFormat("en-US", {
      timeZone: "America/Puerto_Rico",
      year: "numeric",
      month: "2-digit",
      day: "2-digit",
    });
    return formatter.format(date) === todayStr;
  };

  const getEventsForDay = (day: Date): DashboardCalendarEventDto[] =>
    events.filter((e) => isSameDay(e.start, day));

  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-4">
        <CardTitle className="flex items-center gap-2 text-base font-semibold">
          <Calendar className="h-4 w-4" />
          {t.dashboard_calendar_title}
        </CardTitle>
        <div className="flex items-center gap-1">
          <Button
            variant="outline"
            size="sm"
            onClick={handlePrevWeek}
            disabled={isPending}
            aria-label={t.dashboard_calendar_previous}
            className="h-7 w-7 p-0"
          >
            <ChevronLeft className="h-4 w-4" />
          </Button>
          <Button
            variant="outline"
            size="sm"
            onClick={handleToday}
            disabled={isPending}
            className="h-7 px-2 text-xs"
          >
            {t.dashboard_calendar_today}
          </Button>
          <Button
            variant="outline"
            size="sm"
            onClick={handleNextWeek}
            disabled={isPending}
            aria-label={t.dashboard_calendar_next}
            className="h-7 w-7 p-0"
          >
            <ChevronRight className="h-4 w-4" />
          </Button>
        </div>
      </CardHeader>
      <CardContent>
        {/* Team member legend */}
        {teamMembers.length > 0 && (
          <div className="mb-4 flex flex-wrap gap-3" data-testid="team-legend">
            {teamMembers.map((member) => (
              <div key={member.name} className="flex items-center gap-1.5">
                <span
                  className="h-2.5 w-2.5 rounded-full"
                  style={{ backgroundColor: member.color }}
                />
                <span className="text-xs text-muted-foreground">
                  {member.name}
                </span>
              </div>
            ))}
          </div>
        )}

        {/* Desktop: Week grid view */}
        <div className="hidden md:block">
          <div className="grid grid-cols-7 gap-px rounded-lg border border-border bg-border">
            {weekDays.map((day) => {
              const dayEvents = getEventsForDay(day);
              const today = isToday(day);
              return (
                <div
                  key={day.toISOString()}
                  className={`min-h-[120px] p-2 first:rounded-tl-lg last:rounded-tr-lg ${
                    today ? "bg-blue-50/50" : "bg-background"
                  }`}
                >
                  <div
                    className={`mb-1 text-xs font-medium ${
                      today
                        ? "text-blue-600"
                        : "text-muted-foreground"
                    }`}
                  >
                    {formatDate(day)}
                  </div>
                  <div className="space-y-1">
                    {dayEvents.map((event) => (
                      <div
                        key={event.id}
                        className="rounded px-1.5 py-0.5 text-xs text-white"
                        style={{ backgroundColor: event.team_member_color }}
                        title={`${event.summary ?? ""} — ${event.team_member_name}`}
                      >
                        <div className="truncate font-medium">
                          {event.summary ?? ""}
                        </div>
                        {!isAllDay(event) && (
                          <div className="truncate opacity-90">
                            {formatTime(event.start)}
                          </div>
                        )}
                      </div>
                    ))}
                  </div>
                </div>
              );
            })}
          </div>
        </div>

        {/* Mobile: Agenda view */}
        <div className="block md:hidden">
          {events.length === 0 && teamMembers.length === 0 ? (
            <p className="text-sm text-muted-foreground">
              {t.dashboard_calendar_not_connected}
            </p>
          ) : events.length === 0 ? (
            <p className="text-sm text-muted-foreground">
              {t.dashboard_calendar_no_events}
            </p>
          ) : (
            <div className="space-y-2">
              {events.map((event) => (
                <div
                  key={event.id}
                  className="flex items-start gap-3 rounded-lg border border-border p-3"
                >
                  <div
                    className="mt-1 h-2.5 w-2.5 shrink-0 rounded-full"
                    style={{ backgroundColor: event.team_member_color }}
                  />
                  <div className="min-w-0 flex-1">
                    <p className="truncate text-sm font-medium text-foreground">
                      {event.summary ?? ""}
                    </p>
                    <div className="flex items-center gap-1.5 text-xs text-muted-foreground">
                      <Clock className="h-3 w-3" />
                      {isAllDay(event) ? (
                        <span>{t.dashboard_calendar_all_day}</span>
                      ) : (
                        <span>
                          {formatTime(event.start)} – {formatTime(event.end)}
                        </span>
                      )}
                    </div>
                    <p className="text-xs text-muted-foreground">
                      {event.team_member_name}
                    </p>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Desktop: Empty state (shown inside the grid if no events) */}
        <div className="hidden md:block">
          {events.length === 0 && teamMembers.length === 0 && (
            <p className="mt-2 text-sm text-muted-foreground">
              {t.dashboard_calendar_not_connected}
            </p>
          )}
          {events.length === 0 && teamMembers.length > 0 && (
            <p className="mt-2 text-sm text-muted-foreground">
              {t.dashboard_calendar_no_events}
            </p>
          )}
        </div>
      </CardContent>
    </Card>
  );
}
