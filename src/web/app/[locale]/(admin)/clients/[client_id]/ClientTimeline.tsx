"use client";

import { useState, useCallback, useEffect, useTransition } from "react";
import { useLocale } from "next-intl";
import { FileText, Mail, MessageSquare, Clock } from "lucide-react";
import { Button } from "@/app/_components/ui/button";
import { fieldnames, type Locale } from "@/app/[locale]/_shared/app-fieldnames";
import { fetchClientTimelineAction } from "./actions";
import type { TimelineItemDto } from "@/server/Services/timelineService";

const PAGE_SIZE = 10;

function getTimelineIcon(type: string) {
  switch (type) {
    case "document":
      return <FileText className="h-4 w-4 text-blue-600" />;
    case "email":
      return <Mail className="h-4 w-4 text-orange-600" />;
    case "message":
      return <MessageSquare className="h-4 w-4 text-green-600" />;
    default:
      return <Clock className="h-4 w-4 text-muted-foreground" />;
  }
}

function getTypeLabel(type: string, t: (typeof fieldnames)[Locale]): string {
  switch (type) {
    case "document":
      return t.clients_timeline_document;
    case "email":
      return t.clients_timeline_email;
    case "message":
      return t.clients_timeline_message;
    default:
      return type;
  }
}

function formatTimestamp(dateStr: string): string {
  try {
    const date = new Date(dateStr);
    return date.toLocaleDateString("en-US", {
      month: "short",
      day: "numeric",
      year: "numeric",
      hour: "numeric",
      minute: "2-digit",
      timeZone: "America/Puerto_Rico",
    });
  } catch {
    return "—";
  }
}

interface ClientTimelineProps {
  clientId: string;
}

export default function ClientTimeline({ clientId }: ClientTimelineProps) {
  const locale = useLocale() as Locale;
  const t = fieldnames[locale];

  const [items, setItems] = useState<TimelineItemDto[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [currentPage, setCurrentPage] = useState(1);
  const [isInitialLoad, setIsInitialLoad] = useState(true);
  const [errorMessage, setErrorMessage] = useState("");
  const [isPending, startTransition] = useTransition();

  const loadTimeline = useCallback(
    (page: number, append: boolean) => {
      setErrorMessage("");
      startTransition(async () => {
        const result = await fetchClientTimelineAction({
          clientId,
          page,
          pageSize: PAGE_SIZE,
        });

        if (result.success && result.data) {
          setItems((prev) =>
            append ? [...prev, ...result.data!.items] : result.data!.items,
          );
          setTotalCount(result.data.total_count);
          setCurrentPage(result.data.page);
        } else {
          setErrorMessage(result.message || t.clients_timeline_load_error);
        }

        setIsInitialLoad(false);
      });
    },
    [clientId, t, startTransition],
  );

  useEffect(() => {
    loadTimeline(1, false);
  }, [loadTimeline]);

  const handleLoadMore = useCallback(() => {
    loadTimeline(currentPage + 1, true);
  }, [loadTimeline, currentPage]);

  const hasMore = items.length < totalCount;

  if (isInitialLoad && isPending) {
    return (
      <div className="rounded-lg border border-border bg-card p-4">
        <h3 className="mb-3 text-sm font-semibold text-foreground">
          {t.clients_timeline_title}
        </h3>
        <p className="text-sm text-muted-foreground">
          {t.clients_timeline_loading}
        </p>
      </div>
    );
  }

  return (
    <div className="rounded-lg border border-border bg-card p-4">
      <h3 className="mb-3 text-sm font-semibold text-foreground">
        {t.clients_timeline_title}
      </h3>

      {errorMessage && (
        <div className="mb-3 rounded-md border border-red-200 bg-red-50 p-3 text-sm text-red-800">
          {errorMessage}
        </div>
      )}

      {items.length === 0 && !isPending ? (
        <p className="text-sm text-muted-foreground">
          {t.clients_timeline_empty}
        </p>
      ) : (
        <div className="space-y-0">
          {items.map((item) => (
            <div
              key={item.id}
              className="flex gap-3 border-b border-border py-3 last:border-b-0"
            >
              {/* Icon */}
              <div className="mt-0.5 flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-muted">
                {getTimelineIcon(item.type)}
              </div>

              {/* Content — compact card on mobile */}
              <div className="min-w-0 flex-1">
                <div className="flex flex-col gap-1 sm:flex-row sm:items-center sm:justify-between">
                  <div className="min-w-0">
                    <span className="text-xs font-medium text-muted-foreground">
                      {getTypeLabel(item.type, t)}
                    </span>
                    <p className="truncate text-sm text-foreground">
                      {item.description}
                    </p>
                  </div>
                  <div className="flex shrink-0 items-center gap-2 text-xs text-muted-foreground">
                    {item.actor && (
                      <span className="truncate max-w-[150px]">
                        {item.actor}
                      </span>
                    )}
                    <span className="whitespace-nowrap">
                      {formatTimestamp(item.timestamp)}
                    </span>
                  </div>
                </div>
              </div>
            </div>
          ))}

          {hasMore && (
            <div className="pt-3 text-center">
              <Button
                variant="outline"
                size="sm"
                onClick={handleLoadMore}
                disabled={isPending}
              >
                {isPending
                  ? t.clients_timeline_loading
                  : t.clients_timeline_load_more}
              </Button>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
