"use client";

import { useState, useCallback, useTransition } from "react";
import { useLocale } from "next-intl";
import { PageHeader } from "@/app/_components/PageHeader";
import { EmptyState } from "@/app/_components/EmptyState";
import { Badge } from "@/app/_components/ui/badge";
import { Button } from "@/app/_components/ui/button";
import { Input } from "@/app/_components/ui/input";
import { Select } from "@/app/_components/ui/select";
import {
  Send,
  Search,
  ArrowLeft,
  ArrowRight,
  Eye,
  Check,
  Mail,
  MessageSquare,
  FileText,
} from "lucide-react";
import {
  fieldnames,
  type Locale,
} from "@/app/[locale]/_shared/app-fieldnames";
import type { PageStatus } from "@/app/[locale]/_shared/app-enums";
import type {
  CommunicationTemplateDto,
  RenderedTemplateDto,
  SendStep,
  ClientOption,
} from "./shared";
import { TEMPLATE_CATEGORIES, MERGE_FIELDS, SAMPLE_MERGE_DATA } from "../templates/shared";
import {
  fetchActiveTemplates,
  fetchClients,
  previewTemplate,
  sendMessage,
} from "./actions";

const CATEGORY_STYLES: Record<number, string> = {
  0: "bg-blue-100 text-blue-800",
  1: "bg-purple-100 text-purple-800",
  2: "bg-amber-100 text-amber-800",
  3: "bg-green-100 text-green-800",
  4: "bg-gray-100 text-gray-800",
};

interface SendTemplateViewProps {
  initialTemplates: CommunicationTemplateDto[];
  initialClients: ClientOption[];
}

export default function SendTemplateView({
  initialTemplates,
  initialClients,
}: SendTemplateViewProps) {
  const locale = useLocale() as Locale;
  const t = fieldnames[locale];
  const [isPending, startTransition] = useTransition();

  // Step state
  const [step, setStep] = useState<SendStep>("select");

  // Template selection
  const [templates] = useState<CommunicationTemplateDto[]>(initialTemplates);
  const [clients] = useState<ClientOption[]>(initialClients);
  const [search, setSearch] = useState("");
  const [categoryFilter, setCategoryFilter] = useState("all");
  const [selectedTemplate, setSelectedTemplate] =
    useState<CommunicationTemplateDto | null>(null);
  const [selectedClient, setSelectedClient] = useState<ClientOption | null>(
    null,
  );

  // Merge fields
  const [mergeFieldValues, setMergeFieldValues] = useState<
    Record<string, string>
  >({});

  // Delivery options
  const [sendViaPortal, setSendViaPortal] = useState(true);
  const [sendViaEmail, setSendViaEmail] = useState(false);

  // Preview
  const [preview, setPreview] = useState<RenderedTemplateDto | null>(null);

  // Status
  const [status, setStatus] = useState<PageStatus>("idle");
  const [errorMessage, setErrorMessage] = useState("");
  const [successMessage, setSuccessMessage] = useState("");

  // Filtered templates
  const filteredTemplates = templates.filter((tmpl) => {
    const matchesSearch =
      !search ||
      tmpl.name.toLowerCase().includes(search.toLowerCase()) ||
      tmpl.subject_template.toLowerCase().includes(search.toLowerCase());
    const matchesCategory =
      categoryFilter === "all" ||
      tmpl.category === Number(categoryFilter);
    return matchesSearch && matchesCategory;
  });

  function getCategoryLabel(category: number): string {
    const labels: Record<number, string> = {
      0: t.templates_category_onboarding,
      1: t.templates_category_document_request,
      2: t.templates_category_payment_reminder,
      3: t.templates_category_tax_season,
      4: t.templates_category_general,
    };
    return labels[category] ?? String(category);
  }

  const handleSelectTemplate = useCallback(
    (tmpl: CommunicationTemplateDto) => {
      setSelectedTemplate(tmpl);
      setErrorMessage("");
    },
    [],
  );

  const handleMergeFieldChange = useCallback(
    (key: string, value: string) => {
      setMergeFieldValues((prev) => ({ ...prev, [key]: value }));
    },
    [],
  );

  const handlePreview = useCallback(() => {
    if (!selectedTemplate || !selectedClient) return;

    startTransition(async () => {
      setStatus("loading");
      setErrorMessage("");

      const fieldsToSend = { ...mergeFieldValues };
      if (!fieldsToSend.client_name && selectedClient.name) {
        fieldsToSend.client_name = selectedClient.name;
      }
      if (!fieldsToSend.client_email && selectedClient.contact_email) {
        fieldsToSend.client_email = selectedClient.contact_email;
      }

      const result = await previewTemplate(selectedTemplate.id, fieldsToSend);
      if (result.success && result.data) {
        setPreview(result.data);
        setStep("preview");
        setStatus("idle");
      } else {
        setErrorMessage(result.message);
        setStatus("failed");
      }
    });
  }, [selectedTemplate, selectedClient, mergeFieldValues, startTransition]);

  const handleSend = useCallback(() => {
    if (!selectedTemplate || !selectedClient) return;

    startTransition(async () => {
      setStatus("loading");
      setErrorMessage("");

      const fieldsToSend = { ...mergeFieldValues };
      if (!fieldsToSend.client_name && selectedClient.name) {
        fieldsToSend.client_name = selectedClient.name;
      }
      if (!fieldsToSend.client_email && selectedClient.contact_email) {
        fieldsToSend.client_email = selectedClient.contact_email;
      }

      const result = await sendMessage({
        template_id: selectedTemplate.id,
        client_id: selectedClient.client_id,
        merge_fields: fieldsToSend,
        send_via_portal: sendViaPortal,
        send_via_email: sendViaEmail,
        recipient_email: sendViaEmail
          ? selectedClient.contact_email ?? undefined
          : undefined,
      });

      if (result.success) {
        setStatus("success");
        setSuccessMessage(t.send_template_sent_success);
        setStep("confirm");
      } else {
        setErrorMessage(result.message);
        setStatus("failed");
      }
    });
  }, [
    selectedTemplate,
    selectedClient,
    mergeFieldValues,
    sendViaPortal,
    sendViaEmail,
    startTransition,
    t,
  ]);

  const handleReset = useCallback(() => {
    setStep("select");
    setSelectedTemplate(null);
    setSelectedClient(null);
    setMergeFieldValues({});
    setPreview(null);
    setSendViaPortal(true);
    setSendViaEmail(false);
    setStatus("idle");
    setErrorMessage("");
    setSuccessMessage("");
  }, []);

  const canProceedToPreview =
    selectedTemplate && selectedClient && (sendViaPortal || sendViaEmail);

  // --- RENDER ---

  if (step === "confirm" && status === "success") {
    return (
      <div className="space-y-6">
        <PageHeader
          title={t.send_template_title}
          breadcrumbs={[
            { label: t.nav_communications, href: "/communications/templates" },
            { label: t.send_template_title },
          ]}
        />
        <div className="flex flex-col items-center justify-center py-16 space-y-4">
          <div className="flex h-16 w-16 items-center justify-center rounded-full bg-green-100">
            <Check className="h-8 w-8 text-green-600" />
          </div>
          <h2 className="text-xl font-semibold">{t.send_template_sent_success}</h2>
          <p className="text-sm text-muted-foreground">
            {t.send_template_sent_description}
          </p>
          <Button onClick={handleReset} variant="outline">
            {t.send_template_send_another}
          </Button>
        </div>
      </div>
    );
  }

  if (step === "preview" && preview) {
    return (
      <div className="space-y-6">
        <PageHeader
          title={t.send_template_title}
          breadcrumbs={[
            { label: t.nav_communications, href: "/communications/templates" },
            { label: t.send_template_title },
          ]}
          actions={
            <Button
              onClick={() => setStep("select")}
              variant="outline"
              size="sm"
            >
              <ArrowLeft className="mr-1.5 h-4 w-4" />
              {t.send_template_back}
            </Button>
          }
        />

        <div className="rounded-lg border bg-card p-6 space-y-4">
          <h2 className="text-lg font-semibold">{t.send_template_preview_title}</h2>

          <div className="space-y-2">
            <label className="text-sm font-medium text-muted-foreground">
              {t.send_template_to}
            </label>
            <p className="text-sm">{selectedClient?.name}</p>
          </div>

          <div className="space-y-2">
            <label className="text-sm font-medium text-muted-foreground">
              {t.templates_subject}
            </label>
            <p className="text-sm font-medium">{preview.subject}</p>
          </div>

          <div className="space-y-2">
            <label className="text-sm font-medium text-muted-foreground">
              {t.templates_body}
            </label>
            <div className="rounded-md border bg-muted/30 p-4 text-sm whitespace-pre-wrap">
              {preview.body}
            </div>
          </div>

          <div className="space-y-2">
            <label className="text-sm font-medium text-muted-foreground">
              {t.send_template_channels}
            </label>
            <div className="flex gap-2">
              {sendViaPortal && (
                <Badge className="bg-blue-100 text-blue-800">
                  <MessageSquare className="mr-1 h-3 w-3" />
                  {t.send_template_portal}
                </Badge>
              )}
              {sendViaEmail && (
                <Badge className="bg-green-100 text-green-800">
                  <Mail className="mr-1 h-3 w-3" />
                  {t.send_template_email}
                </Badge>
              )}
            </div>
          </div>

          {errorMessage && (
            <p className="text-sm text-destructive">{errorMessage}</p>
          )}

          <div className="flex justify-end gap-2 pt-4">
            <Button
              onClick={() => setStep("select")}
              variant="outline"
              disabled={isPending}
            >
              {t.send_template_back}
            </Button>
            <Button onClick={handleSend} disabled={isPending}>
              <Send className="mr-1.5 h-4 w-4" />
              {isPending ? t.send_template_sending : t.send_template_confirm_send}
            </Button>
          </div>
        </div>
      </div>
    );
  }

  // Step: select template + client
  return (
    <div className="space-y-6">
      <PageHeader
        title={t.send_template_title}
        breadcrumbs={[
          { label: t.nav_communications, href: "/communications/templates" },
          { label: t.send_template_title },
        ]}
      />

      {/* Client selector */}
      <div className="rounded-lg border bg-card p-6 space-y-4">
        <h2 className="text-lg font-semibold">{t.send_template_select_client}</h2>
        <Select
          value={selectedClient?.client_id ?? ""}
          onChange={(e) => {
            const client = clients.find(
              (c) => c.client_id === e.target.value,
            );
            setSelectedClient(client ?? null);
          }}
        >
          <option value="">{t.send_template_choose_client}</option>
          {clients.map((client) => (
            <option key={client.client_id} value={client.client_id}>
              {client.name}
              {client.contact_email ? ` (${client.contact_email})` : ""}
            </option>
          ))}
        </Select>
      </div>

      {/* Template selector */}
      <div className="rounded-lg border bg-card p-6 space-y-4">
        <h2 className="text-lg font-semibold">{t.send_template_select_template}</h2>

        {/* Filters */}
        <div className="flex flex-col gap-3 sm:flex-row">
          <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              type="text"
              placeholder={t.templates_search_placeholder}
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="pl-9"
            />
          </div>
          <Select
            value={categoryFilter}
            onChange={(e) => setCategoryFilter(e.target.value)}
          >
            <option value="all">{t.templates_all_categories}</option>
            {TEMPLATE_CATEGORIES.map((cat) => (
              <option key={cat.value} value={cat.value}>
                {getCategoryLabel(cat.value)}
              </option>
            ))}
          </Select>
        </div>

        {/* Template cards */}
        {filteredTemplates.length === 0 ? (
          <EmptyState
            icon={FileText}
            title={t.templates_empty_title}
            message={t.templates_no_results}
          />
        ) : (
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
            {filteredTemplates.map((tmpl) => (
              <button
                key={tmpl.id}
                type="button"
                onClick={() => handleSelectTemplate(tmpl)}
                className={`rounded-lg border p-4 text-left transition-colors hover:bg-muted/50 ${
                  selectedTemplate?.id === tmpl.id
                    ? "border-primary ring-2 ring-primary/20"
                    : "border-border"
                }`}
              >
                <div className="flex items-center justify-between mb-2">
                  <span className="text-sm font-medium truncate">
                    {tmpl.name}
                  </span>
                  <Badge
                    className={
                      CATEGORY_STYLES[tmpl.category] ?? "bg-gray-100 text-gray-800"
                    }
                  >
                    {getCategoryLabel(tmpl.category)}
                  </Badge>
                </div>
                <p className="text-xs text-muted-foreground truncate">
                  {tmpl.subject_template}
                </p>
              </button>
            ))}
          </div>
        )}
      </div>

      {/* Merge fields */}
      {selectedTemplate && (
        <div className="rounded-lg border bg-card p-6 space-y-4">
          <h2 className="text-lg font-semibold">{t.send_template_merge_fields}</h2>
          <p className="text-sm text-muted-foreground">
            {t.send_template_merge_fields_description}
          </p>
          <div className="grid gap-3 sm:grid-cols-2">
            {MERGE_FIELDS.map((field) => (
              <div key={field.key} className="space-y-1">
                <label className="text-sm font-medium">{field.label}</label>
                <Input
                  type="text"
                  placeholder={SAMPLE_MERGE_DATA[field.key] ?? ""}
                  value={mergeFieldValues[field.key] ?? ""}
                  onChange={(e) =>
                    handleMergeFieldChange(field.key, e.target.value)
                  }
                />
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Delivery options */}
      {selectedTemplate && selectedClient && (
        <div className="rounded-lg border bg-card p-6 space-y-4">
          <h2 className="text-lg font-semibold">{t.send_template_channels}</h2>
          <div className="space-y-3">
            <label className="flex items-center gap-3 cursor-pointer">
              <input
                type="checkbox"
                checked={sendViaPortal}
                onChange={(e) => setSendViaPortal(e.target.checked)}
                className="h-4 w-4 rounded border-gray-300"
              />
              <MessageSquare className="h-4 w-4 text-muted-foreground" />
              <span className="text-sm">{t.send_template_portal}</span>
            </label>
            <label className="flex items-center gap-3 cursor-pointer">
              <input
                type="checkbox"
                checked={sendViaEmail}
                onChange={(e) => setSendViaEmail(e.target.checked)}
                className="h-4 w-4 rounded border-gray-300"
              />
              <Mail className="h-4 w-4 text-muted-foreground" />
              <span className="text-sm">
                {t.send_template_email}
                {selectedClient?.contact_email
                  ? ` (${selectedClient.contact_email})`
                  : ""}
              </span>
            </label>
          </div>
        </div>
      )}

      {/* Error message */}
      {errorMessage && (
        <p className="text-sm text-destructive">{errorMessage}</p>
      )}

      {/* Actions */}
      <div className="flex justify-end gap-2">
        <Button
          onClick={handlePreview}
          disabled={!canProceedToPreview || isPending}
        >
          <Eye className="mr-1.5 h-4 w-4" />
          {isPending ? t.send_template_loading : t.send_template_preview}
          <ArrowRight className="ml-1.5 h-4 w-4" />
        </Button>
      </div>
    </div>
  );
}
