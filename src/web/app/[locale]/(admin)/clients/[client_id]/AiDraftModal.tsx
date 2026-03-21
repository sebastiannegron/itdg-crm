"use client";

import { useState, useCallback, useTransition } from "react";
import { useLocale } from "next-intl";
import { Button } from "@/app/_components/ui/button";
import { Input } from "@/app/_components/ui/input";
import { fieldnames, type Locale } from "@/app/[locale]/_shared/app-fieldnames";
import { codeRegex, urlRegex } from "@/app/[locale]/_shared/app-enums";
import { generateAiDraftAction } from "./actions";
import type { ActionResult } from "./actions";
import type { DraftEmailResponse } from "@/server/Services/aiService";

type DraftStep = "input" | "preview";

interface AiDraftModalProps {
  clientName: string;
  onUseDraft: (draft: string) => void;
  onClose: () => void;
}

export default function AiDraftModal({
  clientName,
  onUseDraft,
  onClose,
}: AiDraftModalProps) {
  const locale = useLocale() as Locale;
  const t = fieldnames[locale];

  const [step, setStep] = useState<DraftStep>("input");
  const [topic, setTopic] = useState("");
  const [additionalContext, setAdditionalContext] = useState("");
  const [draft, setDraft] = useState("");
  const [editedDraft, setEditedDraft] = useState("");
  const [errorMessage, setErrorMessage] = useState("");
  const [isPending, startTransition] = useTransition();

  const validateInput = useCallback(
    (value: string): string | null => {
      if (!value.trim()) return t.ai_draft_topic_required;
      if (codeRegex.test(value) || urlRegex.test(value))
        return t.ai_draft_invalid_input;
      return null;
    },
    [t],
  );

  const handleGenerate = useCallback(() => {
    const topicError = validateInput(topic);
    if (topicError) {
      setErrorMessage(topicError);
      return;
    }

    if (
      additionalContext.trim() &&
      (codeRegex.test(additionalContext) || urlRegex.test(additionalContext))
    ) {
      setErrorMessage(t.ai_draft_invalid_input);
      return;
    }

    setErrorMessage("");

    startTransition(async () => {
      const language = locale === "es-pr" ? "es" : "en";
      const result: ActionResult<DraftEmailResponse> =
        await generateAiDraftAction({
          client_name: clientName,
          topic: topic.trim(),
          language,
          additional_context: additionalContext.trim() || undefined,
        });

      if (result.success && result.data) {
        setDraft(result.data.draft);
        setEditedDraft(result.data.draft);
        setStep("preview");
        setErrorMessage("");
      } else {
        setErrorMessage(result.message || t.ai_draft_error);
      }
    });
  }, [topic, additionalContext, clientName, locale, t, validateInput]);

  const handleRegenerate = useCallback(() => {
    setStep("input");
    setDraft("");
    setEditedDraft("");
  }, []);

  const handleUseDraft = useCallback(() => {
    onUseDraft(editedDraft);
  }, [editedDraft, onUseDraft]);

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
      <div className="mx-4 w-full max-w-lg rounded-lg bg-white shadow-xl">
        {/* Header */}
        <div className="flex items-center justify-between border-b border-border px-5 py-4">
          <h2 className="text-base font-semibold text-foreground">
            {t.ai_draft_title}
          </h2>
          <button
            type="button"
            onClick={onClose}
            className="text-muted-foreground hover:text-foreground"
            aria-label="Close"
          >
            ✕
          </button>
        </div>

        {/* Warning banner */}
        <div className="mx-5 mt-4 rounded-md border border-[#FDE68A] bg-[#FFFBEB] px-4 py-2.5">
          <p className="text-xs text-amber-800">{t.ai_draft_warning}</p>
        </div>

        {/* Content */}
        <div className="px-5 py-4">
          {step === "input" && (
            <div className="space-y-4">
              {/* Topic */}
              <div>
                <label className="mb-1.5 block text-sm font-medium text-foreground">
                  {t.ai_draft_topic_label}
                </label>
                <textarea
                  value={topic}
                  onChange={(e) => setTopic(e.target.value)}
                  placeholder={t.ai_draft_topic_placeholder}
                  rows={3}
                  maxLength={500}
                  className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring"
                />
              </div>

              {/* Additional Context */}
              <div>
                <label className="mb-1.5 block text-sm font-medium text-foreground">
                  {t.ai_draft_context_label}
                </label>
                <Input
                  value={additionalContext}
                  onChange={(e) => setAdditionalContext(e.target.value)}
                  placeholder={t.ai_draft_context_placeholder}
                  maxLength={2000}
                />
              </div>

              {errorMessage && (
                <div className="rounded-md border border-red-200 bg-red-50 p-3 text-sm text-red-800">
                  {errorMessage}
                </div>
              )}

              <div className="flex justify-end gap-2">
                <Button variant="outline" size="sm" onClick={onClose}>
                  {t.ai_draft_discard}
                </Button>
                <Button
                  size="sm"
                  onClick={handleGenerate}
                  disabled={isPending}
                  className="bg-[#1a2744] text-white hover:bg-[#1a2744]/90"
                >
                  {isPending ? t.ai_draft_generating : t.ai_draft_generate}
                </Button>
              </div>
            </div>
          )}

          {step === "preview" && (
            <div className="space-y-4">
              {/* AI Draft Output */}
              <div className="rounded-md border border-[#BBF7D0] bg-[#F0FDF4] p-4">
                <p className="mb-2 text-xs font-semibold text-green-800">
                  {t.ai_draft_result_title}
                </p>
                <textarea
                  value={editedDraft}
                  onChange={(e) => setEditedDraft(e.target.value)}
                  rows={10}
                  className="w-full rounded-md border border-border bg-white px-3 py-2 text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-ring"
                />
              </div>

              {errorMessage && (
                <div className="rounded-md border border-red-200 bg-red-50 p-3 text-sm text-red-800">
                  {errorMessage}
                </div>
              )}

              <div className="flex justify-end gap-2">
                <Button variant="outline" size="sm" onClick={onClose}>
                  {t.ai_draft_discard}
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={handleRegenerate}
                >
                  {t.ai_draft_regenerate}
                </Button>
                <Button
                  size="sm"
                  onClick={handleUseDraft}
                  className="bg-orange-500 text-white hover:bg-orange-600"
                >
                  {t.ai_draft_use}
                </Button>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
