"use client";

import { useState, useCallback, useTransition } from "react";
import { useLocale } from "next-intl";
import { MessageSquare, ArrowLeft, Send, Mail, MailOpen } from "lucide-react";
import { Button } from "@/app/_components/ui/button";
import { Input } from "@/app/_components/ui/input";
import { Badge } from "@/app/_components/ui/badge";
import { EmptyState } from "@/app/_components/EmptyState";
import { fieldnames, type Locale } from "@/app/[locale]/_shared/app-fieldnames";
import { codeRegex, urlRegex, type PageStatus } from "@/app/[locale]/_shared/app-enums";
import type { MessageDto, MessageDirection } from "./shared";
import {
  sendPortalMessage,
  markPortalMessageAsRead,
  fetchPortalMessages,
} from "./actions";

function formatDate(dateStr: string, locale: Locale): string {
  try {
    const date = new Date(dateStr);
    return date.toLocaleDateString(locale === "es-pr" ? "es-PR" : "en-US", {
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

function formatDateShort(dateStr: string): string {
  try {
    const date = new Date(dateStr);
    return date.toLocaleDateString("en-US", {
      month: "short",
      day: "numeric",
      timeZone: "America/Puerto_Rico",
    });
  } catch {
    return "—";
  }
}

interface MessagesViewProps {
  initialMessages: MessageDto[];
}

export default function MessagesView({ initialMessages }: MessagesViewProps) {
  const locale = useLocale() as Locale;
  const t = fieldnames[locale];

  const [messages, setMessages] = useState<MessageDto[]>(initialMessages);
  const [selectedMessage, setSelectedMessage] = useState<MessageDto | null>(
    null,
  );
  const [showCompose, setShowCompose] = useState(false);
  const [replyBody, setReplyBody] = useState("");
  const [composeSubject, setComposeSubject] = useState("");
  const [composeBody, setComposeBody] = useState("");
  const [status, setStatus] = useState<PageStatus>("idle");
  const [errorMessage, setErrorMessage] = useState("");
  const [isPending, startTransition] = useTransition();

  const hasInputInjection = useCallback((value: string): boolean => {
    return codeRegex.test(value) || urlRegex.test(value);
  }, []);

  const handleSelectMessage = useCallback(
    (message: MessageDto) => {
      setSelectedMessage(message);
      setShowCompose(false);
      setReplyBody("");
      setErrorMessage("");
      setStatus("idle");

      if (!message.is_read) {
        startTransition(async () => {
          await markPortalMessageAsRead(message.id);
          setMessages((prev) =>
            prev.map((m) =>
              m.id === message.id ? { ...m, is_read: true } : m,
            ),
          );
        });
      }
    },
    [startTransition],
  );

  const handleBackToInbox = useCallback(() => {
    setSelectedMessage(null);
    setShowCompose(false);
    setReplyBody("");
    setComposeSubject("");
    setComposeBody("");
    setErrorMessage("");
    setStatus("idle");
  }, []);

  const handleReply = useCallback(async () => {
    if (!selectedMessage || !replyBody.trim()) {
      setErrorMessage(t.messages_body_required);
      return;
    }

    if (hasInputInjection(replyBody)) {
      setErrorMessage(t.messages_sent_error);
      return;
    }

    setStatus("loading");
    setErrorMessage("");

    const formData = new FormData();
    formData.append("subject", `Re: ${selectedMessage.subject}`);
    formData.append("body", replyBody.trim());

    const result = await sendPortalMessage(formData);

    if (result.success) {
      setStatus("success");
      setReplyBody("");
      const refreshResult = await fetchPortalMessages();
      if (refreshResult.success && refreshResult.data) {
        setMessages(refreshResult.data);
      }
    } else {
      setStatus("failed");
      setErrorMessage(result.message || t.messages_sent_error);
    }
  }, [selectedMessage, replyBody, t, hasInputInjection]);

  const handleCompose = useCallback(async () => {
    if (!composeSubject.trim()) {
      setErrorMessage(t.messages_subject_required);
      return;
    }
    if (!composeBody.trim()) {
      setErrorMessage(t.messages_body_required);
      return;
    }

    if (
      hasInputInjection(composeSubject) ||
      hasInputInjection(composeBody)
    ) {
      setErrorMessage(t.messages_sent_error);
      return;
    }

    setStatus("loading");
    setErrorMessage("");

    const formData = new FormData();
    formData.append("subject", composeSubject.trim());
    formData.append("body", composeBody.trim());

    const result = await sendPortalMessage(formData);

    if (result.success) {
      setStatus("success");
      setComposeSubject("");
      setComposeBody("");
      setShowCompose(false);
      const refreshResult = await fetchPortalMessages();
      if (refreshResult.success && refreshResult.data) {
        setMessages(refreshResult.data);
      }
    } else {
      setStatus("failed");
      setErrorMessage(result.message || t.messages_sent_error);
    }
  }, [composeSubject, composeBody, t, hasInputInjection]);

  const handleShowCompose = useCallback(() => {
    setSelectedMessage(null);
    setShowCompose(true);
    setComposeSubject("");
    setComposeBody("");
    setErrorMessage("");
    setStatus("idle");
  }, []);

  // Compose view
  if (showCompose) {
    return (
      <div className="space-y-4">
        <div className="flex items-center gap-3">
          <button
            type="button"
            onClick={handleBackToInbox}
            className="rounded-md p-1.5 text-muted-foreground hover:bg-secondary hover:text-foreground transition-colors"
            aria-label={t.messages_back_to_inbox}
          >
            <ArrowLeft className="h-5 w-5" />
          </button>
          <h1 className="text-lg font-semibold text-foreground">
            {t.messages_new}
          </h1>
        </div>

        <div className="rounded-lg border border-border bg-card p-4 space-y-4">
          <div>
            <label
              htmlFor="compose-subject"
              className="block text-sm font-medium text-foreground mb-1"
            >
              {t.messages_subject}
            </label>
            <Input
              id="compose-subject"
              value={composeSubject}
              onChange={(e) => setComposeSubject(e.target.value)}
              placeholder={t.messages_new_subject_placeholder}
              maxLength={500}
              disabled={status === "loading"}
            />
          </div>

          <div>
            <label
              htmlFor="compose-body"
              className="block text-sm font-medium text-foreground mb-1"
            >
              {t.messages_body}
            </label>
            <textarea
              id="compose-body"
              value={composeBody}
              onChange={(e) => setComposeBody(e.target.value)}
              placeholder={t.messages_new_body_placeholder}
              maxLength={4000}
              disabled={status === "loading"}
              rows={6}
              className="flex w-full rounded-md border border-border bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
            />
          </div>

          {errorMessage && (
            <p className="text-sm text-destructive" role="alert">
              {errorMessage}
            </p>
          )}

          {status === "success" && (
            <p className="text-sm text-green-600" role="status">
              {t.messages_sent_success}
            </p>
          )}

          <div className="flex justify-end">
            <Button
              onClick={handleCompose}
              disabled={
                status === "loading" ||
                !composeSubject.trim() ||
                !composeBody.trim()
              }
              size="sm"
            >
              <Send className="h-4 w-4" />
              {status === "loading" ? t.messages_sending : t.messages_send}
            </Button>
          </div>
        </div>
      </div>
    );
  }

  // Detail view
  if (selectedMessage) {
    const direction = selectedMessage.direction as MessageDirection;
    return (
      <div className="space-y-4">
        <div className="flex items-center gap-3">
          <button
            type="button"
            onClick={handleBackToInbox}
            className="rounded-md p-1.5 text-muted-foreground hover:bg-secondary hover:text-foreground transition-colors"
            aria-label={t.messages_back_to_inbox}
          >
            <ArrowLeft className="h-5 w-5" />
          </button>
          <h1 className="min-w-0 flex-1 truncate text-lg font-semibold text-foreground">
            {selectedMessage.subject}
          </h1>
        </div>

        <div className="rounded-lg border border-border bg-card p-4 sm:p-6">
          <div className="flex flex-col gap-1 border-b border-border pb-4 sm:flex-row sm:items-center sm:justify-between">
            <span className="text-sm font-medium text-foreground">
              {direction === "Outbound"
                ? t.messages_from_you
                : t.messages_from_advisor}
            </span>
            <span className="text-sm text-muted-foreground">
              {formatDate(selectedMessage.created_at, locale)}
            </span>
          </div>

          <div className="prose prose-sm mt-4 max-w-none whitespace-pre-wrap text-sm text-foreground">
            {selectedMessage.body}
          </div>
        </div>

        {/* Reply form */}
        <div className="rounded-lg border border-border bg-card p-4 space-y-3">
          <label
            htmlFor="reply-body"
            className="block text-sm font-medium text-foreground"
          >
            {t.messages_reply}
          </label>
          <textarea
            id="reply-body"
            value={replyBody}
            onChange={(e) => setReplyBody(e.target.value)}
            placeholder={t.messages_reply_placeholder}
            maxLength={4000}
            disabled={status === "loading"}
            rows={4}
            className="flex w-full rounded-md border border-border bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
          />

          {errorMessage && (
            <p className="text-sm text-destructive" role="alert">
              {errorMessage}
            </p>
          )}

          {status === "success" && (
            <p className="text-sm text-green-600" role="status">
              {t.messages_sent_success}
            </p>
          )}

          <div className="flex justify-end">
            <Button
              onClick={handleReply}
              disabled={status === "loading" || !replyBody.trim()}
              size="sm"
            >
              <Send className="h-4 w-4" />
              {status === "loading" ? t.messages_sending : t.messages_send}
            </Button>
          </div>
        </div>
      </div>
    );
  }

  // Inbox list view
  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-lg font-semibold text-foreground">
          {t.messages_inbox}
        </h1>
        <Button onClick={handleShowCompose} size="sm">
          <Send className="h-4 w-4" />
          {t.messages_new}
        </Button>
      </div>

      {messages.length === 0 ? (
        <EmptyState
          icon={MessageSquare}
          title={t.messages_empty_title}
          message={t.messages_empty_message}
        />
      ) : (
        <div className="divide-y divide-border rounded-lg border border-border bg-card">
          {messages.map((message) => {
            const direction = message.direction as MessageDirection;
            return (
              <button
                key={message.id}
                type="button"
                onClick={() => handleSelectMessage(message)}
                className={`flex w-full items-start gap-3 p-4 text-left transition-colors hover:bg-accent ${
                  !message.is_read ? "bg-primary/5" : ""
                }`}
              >
                <div className="mt-0.5 shrink-0">
                  {message.is_read ? (
                    <MailOpen className="h-4 w-4 text-muted-foreground" />
                  ) : (
                    <Mail className="h-4 w-4 text-primary" />
                  )}
                </div>
                <div className="min-w-0 flex-1">
                  <div className="flex items-start justify-between gap-2">
                    <span
                      className={`truncate text-sm ${
                        !message.is_read
                          ? "font-semibold text-foreground"
                          : "font-medium text-foreground"
                      }`}
                    >
                      {message.subject}
                    </span>
                    <span className="shrink-0 text-xs text-muted-foreground">
                      {formatDateShort(message.created_at)}
                    </span>
                  </div>
                  <div className="mt-0.5 flex items-center gap-2">
                    <span className="truncate text-xs text-muted-foreground">
                      {direction === "Outbound"
                        ? t.messages_from_you
                        : t.messages_from_advisor}
                    </span>
                    {!message.is_read && (
                      <Badge
                        variant="default"
                        className="shrink-0 text-[10px] px-1.5 py-0"
                      >
                        {t.messages_unread}
                      </Badge>
                    )}
                  </div>
                </div>
              </button>
            );
          })}
        </div>
      )}
    </div>
  );
}
