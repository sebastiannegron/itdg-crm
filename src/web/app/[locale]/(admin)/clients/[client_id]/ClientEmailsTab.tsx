"use client";

import { useState, useEffect, useCallback, useTransition } from "react";
import { useLocale } from "next-intl";
import { Input } from "@/app/_components/ui/input";
import { Button } from "@/app/_components/ui/button";
import {
  Search,
  Paperclip,
  Mail,
  ChevronLeft,
  ChevronRight,
} from "lucide-react";
import { fieldnames, type Locale } from "@/app/[locale]/_shared/app-fieldnames";
import type { EmailMirrorDto } from "@/server/Services/emailMirrorService";
import { fetchClientEmailsAction } from "./actions";
import type { ActionResult } from "./actions";
import type { PaginatedEmails } from "@/server/Services/emailMirrorService";
import AiDraftModal from "./AiDraftModal";

const PAGE_SIZE = 20;

function formatEmailDate(dateStr: string): string {
  try {
    const date = new Date(dateStr);
    const now = new Date();
    const isToday = date.toDateString() === now.toDateString();

    if (isToday) {
      return date.toLocaleTimeString("en-US", {
        hour: "numeric",
        minute: "2-digit",
        hour12: true,
        timeZone: "America/Puerto_Rico",
      });
    }

    return date.toLocaleDateString("en-US", {
      month: "short",
      day: "numeric",
      timeZone: "America/Puerto_Rico",
    });
  } catch {
    return "—";
  }
}

function formatFullDate(dateStr: string): string {
  try {
    const date = new Date(dateStr);
    return date.toLocaleDateString("en-US", {
      month: "short",
      day: "numeric",
      year: "numeric",
      hour: "numeric",
      minute: "2-digit",
      hour12: true,
      timeZone: "America/Puerto_Rico",
    });
  } catch {
    return "—";
  }
}

function extractName(emailStr: string): string {
  const match = emailStr.match(/^(.+?)\s*<.*>$/);
  if (match) return match[1].trim();
  return emailStr.split("@")[0];
}

function truncateSubject(subject: string, maxLength: number = 40): string {
  if (subject.length <= maxLength) return subject;
  return subject.substring(0, maxLength) + "…";
}

interface ClientEmailsTabProps {
  clientId: string;
  clientEmail?: string | null;
  clientName?: string | null;
}

export default function ClientEmailsTab({
  clientId,
  clientEmail,
  clientName,
}: ClientEmailsTabProps) {
  const locale = useLocale() as Locale;
  const t = fieldnames[locale];

  const [emails, setEmails] = useState<EmailMirrorDto[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [currentPage, setCurrentPage] = useState(1);
  const [searchQuery, setSearchQuery] = useState("");
  const [selectedEmail, setSelectedEmail] = useState<EmailMirrorDto | null>(
    null,
  );
  const [threadEmails, setThreadEmails] = useState<EmailMirrorDto[]>([]);
  const [hasLoaded, setHasLoaded] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");
  const [isPending, startTransition] = useTransition();
  const [showMobileThread, setShowMobileThread] = useState(false);
  const [showAiDraft, setShowAiDraft] = useState(false);

  const loadEmails = useCallback(
    (page: number, search: string) => {
      startTransition(async () => {
        const result: ActionResult<PaginatedEmails> =
          await fetchClientEmailsAction({
            clientId,
            page,
            pageSize: PAGE_SIZE,
            search: search || undefined,
          });

        if (result.success && result.data) {
          setEmails(result.data.items);
          setTotalCount(result.data.total_count);
          setErrorMessage("");
        } else {
          setErrorMessage(result.message || t.clients_emails_load_error);
        }
        setHasLoaded(true);
      });
    },
    [clientId, t],
  );

  useEffect(() => {
    loadEmails(1, "");
  }, [loadEmails]);

  useEffect(() => {
    if (selectedEmail) {
      const threadId = selectedEmail.gmail_thread_id;
      const thread = emails.filter((e) => e.gmail_thread_id === threadId);
      thread.sort(
        (a, b) =>
          new Date(a.received_at).getTime() -
          new Date(b.received_at).getTime(),
      );
      setThreadEmails(thread);
    } else {
      setThreadEmails([]);
    }
  }, [selectedEmail, emails]);

  const handleSearch = useCallback(() => {
    setCurrentPage(1);
    setSelectedEmail(null);
    setShowMobileThread(false);
    loadEmails(1, searchQuery);
  }, [searchQuery, loadEmails]);

  const handleSearchKeyDown = useCallback(
    (e: React.KeyboardEvent) => {
      if (e.key === "Enter") {
        handleSearch();
      }
    },
    [handleSearch],
  );

  const handlePageChange = useCallback(
    (newPage: number) => {
      setCurrentPage(newPage);
      setSelectedEmail(null);
      setShowMobileThread(false);
      loadEmails(newPage, searchQuery);
    },
    [searchQuery, loadEmails],
  );

  const handleSelectEmail = useCallback((email: EmailMirrorDto) => {
    setSelectedEmail(email);
    setShowMobileThread(true);
  }, []);

  const handleBackToList = useCallback(() => {
    setShowMobileThread(false);
  }, []);

  const totalPages = Math.ceil(totalCount / PAGE_SIZE);

  const isFromClient = useCallback(
    (email: EmailMirrorDto): boolean => {
      if (!clientEmail) return false;
      return email.from.toLowerCase().includes(clientEmail.toLowerCase());
    },
    [clientEmail],
  );

  if (!hasLoaded) {
    return (
      <div className="flex items-center justify-center py-12 text-sm text-muted-foreground">
        {t.clients_emails_loading}
      </div>
    );
  }

  return (
    <div className="space-y-4">
      {/* Search Bar */}
      <div className="flex items-center gap-2">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder={t.clients_emails_search}
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            onKeyDown={handleSearchKeyDown}
            className="pl-9"
          />
        </div>
        <Button variant="outline" size="sm" onClick={handleSearch}>
          <Search className="h-4 w-4" />
        </Button>
        <Button
          size="sm"
          onClick={() => setShowAiDraft(true)}
          className="bg-[#1a2744] text-white hover:bg-[#1a2744]/90"
        >
          {t.ai_draft_button}
        </Button>
      </div>

      {/* Email count and pagination info */}
      {totalCount > 0 && (
        <div className="text-xs text-muted-foreground">
          {totalCount} {t.clients_emails_count}
        </div>
      )}

      {errorMessage && (
        <div className="rounded-md border border-red-200 bg-red-50 p-3 text-sm text-red-800">
          {errorMessage}
        </div>
      )}

      {/* Email Panel Layout */}
      {emails.length === 0 && !errorMessage ? (
        <div className="flex flex-col items-center justify-center rounded-lg border border-border bg-card py-12">
          <Mail className="mb-3 h-10 w-10 text-muted-foreground" />
          <p className="text-sm text-muted-foreground">
            {t.clients_emails_empty}
          </p>
        </div>
      ) : (
        <div className="flex rounded-lg border border-border bg-card overflow-hidden min-h-[500px]">
          {/* Left Panel — Email List */}
          <div
            className={`w-full border-r border-border md:block md:w-[220px] md:min-w-[220px] ${
              showMobileThread ? "hidden" : "block"
            }`}
          >
            <div className="divide-y divide-border">
              {emails.map((email) => {
                const isSelected =
                  selectedEmail?.email_id === email.email_id;
                return (
                  <button
                    key={email.email_id}
                    type="button"
                    onClick={() => handleSelectEmail(email)}
                    className={`flex w-full flex-col gap-1 px-3 py-2.5 text-left transition-colors ${
                      isSelected
                        ? "border-l-2 border-l-orange-500 bg-[#FFF8F5]"
                        : "border-l-2 border-l-transparent hover:bg-muted/50"
                    }`}
                  >
                    <div className="flex items-center justify-between gap-1">
                      <span className="truncate text-xs font-medium text-foreground">
                        {extractName(email.from)}
                      </span>
                      <span className="shrink-0 text-[10px] text-muted-foreground">
                        {formatEmailDate(email.received_at)}
                      </span>
                    </div>
                    <span className="truncate text-xs text-muted-foreground">
                      {truncateSubject(email.subject)}
                    </span>
                    {email.has_attachments && (
                      <Paperclip className="h-3 w-3 text-muted-foreground" />
                    )}
                  </button>
                );
              })}
            </div>

            {/* Pagination */}
            {totalPages > 1 && (
              <div className="flex items-center justify-between border-t border-border px-3 py-2">
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => handlePageChange(currentPage - 1)}
                  disabled={currentPage <= 1 || isPending}
                  className="h-7 w-7 p-0"
                >
                  <ChevronLeft className="h-4 w-4" />
                </Button>
                <span className="text-[10px] text-muted-foreground">
                  {currentPage} / {totalPages}
                </span>
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => handlePageChange(currentPage + 1)}
                  disabled={currentPage >= totalPages || isPending}
                  className="h-7 w-7 p-0"
                >
                  <ChevronRight className="h-4 w-4" />
                </Button>
              </div>
            )}
          </div>

          {/* Right Panel — Thread View */}
          <div
            className={`flex-1 ${
              showMobileThread ? "block" : "hidden md:block"
            }`}
          >
            {selectedEmail ? (
              <div className="flex h-full flex-col">
                {/* Thread Header */}
                <div className="flex items-center justify-between border-b border-border px-4 py-3">
                  <div className="flex items-center gap-2">
                    <button
                      type="button"
                      onClick={handleBackToList}
                      className="md:hidden"
                    >
                      <ChevronLeft className="h-5 w-5 text-muted-foreground" />
                    </button>
                    <div>
                      <h3 className="text-sm font-semibold text-foreground">
                        {selectedEmail.subject}
                      </h3>
                      <p className="text-[10px] text-muted-foreground">
                        {t.clients_emails_thread} · {threadEmails.length}{" "}
                        {t.clients_emails_count}
                      </p>
                    </div>
                  </div>
                  <div className="flex items-center gap-2">
                    <Button
                      variant="outline"
                      size="sm"
                      className="border-red-300 text-red-600 hover:bg-red-50"
                    >
                      {t.clients_emails_escalate}
                    </Button>
                    <Button
                      size="sm"
                      className="bg-orange-500 text-white hover:bg-orange-600"
                    >
                      {t.clients_emails_reply}
                    </Button>
                  </div>
                </div>

                {/* Thread Messages */}
                <div className="flex-1 space-y-3 overflow-y-auto p-4">
                  {threadEmails.map((email) => {
                    const fromClient = isFromClient(email);
                    return (
                      <div
                        key={email.email_id}
                        className={`flex ${
                          fromClient ? "justify-start" : "justify-end"
                        }`}
                      >
                        <div
                          className={`max-w-[80%] rounded-lg px-4 py-3 ${
                            fromClient
                              ? "bg-white border border-border"
                              : "bg-[#1e293b] text-white"
                          }`}
                        >
                          <div className="mb-1 flex items-center gap-2">
                            <span
                              className={`text-xs font-semibold ${
                                fromClient
                                  ? "text-orange-500"
                                  : "text-[#93C5FD]"
                              }`}
                            >
                              {extractName(email.from)}
                            </span>
                            <span
                              className={`text-[10px] ${
                                fromClient
                                  ? "text-muted-foreground"
                                  : "text-gray-400"
                              }`}
                            >
                              {formatFullDate(email.received_at)}
                            </span>
                          </div>
                          {email.has_attachments && (
                            <div
                              className={`mb-1 flex items-center gap-1 text-[10px] ${
                                fromClient
                                  ? "text-muted-foreground"
                                  : "text-gray-400"
                              }`}
                            >
                              <Paperclip className="h-3 w-3" />
                              {t.clients_emails_attachments}
                            </div>
                          )}
                          <p
                            className={`text-sm whitespace-pre-wrap ${
                              fromClient ? "text-foreground" : "text-white"
                            }`}
                          >
                            {email.body_preview || t.clients_emails_no_preview}
                          </p>
                        </div>
                      </div>
                    );
                  })}
                </div>
              </div>
            ) : (
              <div className="flex h-full items-center justify-center">
                <div className="text-center">
                  <Mail className="mx-auto mb-3 h-10 w-10 text-muted-foreground" />
                  <p className="text-sm text-muted-foreground">
                    {t.clients_emails_select}
                  </p>
                </div>
              </div>
            )}
          </div>
        </div>
      )}

      {/* AI Draft Modal */}
      {showAiDraft && (
        <AiDraftModal
          clientName={clientName || "Client"}
          onUseDraft={(draft) => {
            setShowAiDraft(false);
            // Draft text is available for the user to copy/use
            void draft;
          }}
          onClose={() => setShowAiDraft(false)}
        />
      )}
    </div>
  );
}
