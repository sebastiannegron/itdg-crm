"use client";

import { useState, useCallback, useMemo, useTransition, useRef } from "react";
import { useLocale } from "next-intl";
import { PageHeader } from "@/app/_components/PageHeader";
import { EmptyState } from "@/app/_components/EmptyState";
import { Badge } from "@/app/_components/ui/badge";
import { Button } from "@/app/_components/ui/button";
import { Input } from "@/app/_components/ui/input";
import { Select } from "@/app/_components/ui/select";
import {
  FileText,
  Search,
  Plus,
  Eye,
  ArrowLeft,
  Type,
} from "lucide-react";
import { fieldnames, type Locale } from "@/app/[locale]/_shared/app-fieldnames";
import type { PageStatus } from "@/app/[locale]/_shared/app-enums";
import {
  type CommunicationTemplateDto,
  TEMPLATE_CATEGORIES,
  MERGE_FIELDS,
  SAMPLE_MERGE_DATA,
} from "./shared";
import {
  fetchTemplates,
  renderTemplatePreview,
  createNewTemplate,
  updateExistingTemplate,
} from "./actions";

type ViewMode = "list" | "editor";

const CATEGORY_LABELS: Record<number, string> = {
  0: "Onboarding",
  1: "Document Request",
  2: "Payment Reminder",
  3: "Tax Season",
  4: "General",
};

const CATEGORY_STYLES: Record<number, string> = {
  0: "bg-blue-100 text-blue-800",
  1: "bg-purple-100 text-purple-800",
  2: "bg-amber-100 text-amber-800",
  3: "bg-green-100 text-green-800",
  4: "bg-gray-100 text-gray-800",
};

function getCategoryLabel(
  category: number,
  t: (typeof fieldnames)[Locale],
): string {
  const labels: Record<number, string> = {
    0: t.templates_category_onboarding,
    1: t.templates_category_document_request,
    2: t.templates_category_payment_reminder,
    3: t.templates_category_tax_season,
    4: t.templates_category_general,
  };
  return labels[category] ?? CATEGORY_LABELS[category] ?? "Unknown";
}

function formatDate(dateStr: string): string {
  try {
    const date = new Date(dateStr);
    return date.toLocaleDateString("en-US", {
      month: "short",
      day: "numeric",
      year: "numeric",
      timeZone: "America/Puerto_Rico",
    });
  } catch {
    return "—";
  }
}

function renderPreviewText(
  text: string,
  mergeData: Record<string, string>,
): string {
  let result = text;
  for (const [key, value] of Object.entries(mergeData)) {
    result = result.replaceAll(`{{${key}}}`, value);
  }
  return result;
}

interface TemplatesViewProps {
  initialTemplates: CommunicationTemplateDto[];
}

export default function TemplatesView({
  initialTemplates,
}: TemplatesViewProps) {
  const locale = useLocale() as Locale;
  const t = fieldnames[locale];

  const [templates, setTemplates] =
    useState<CommunicationTemplateDto[]>(initialTemplates);
  const [viewMode, setViewMode] = useState<ViewMode>("list");
  const [selectedTemplate, setSelectedTemplate] =
    useState<CommunicationTemplateDto | null>(null);
  const [search, setSearch] = useState("");
  const [categoryFilter, setCategoryFilter] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [status, setStatus] = useState<PageStatus>("idle");
  const [errorMessage, setErrorMessage] = useState("");
  const [isPending, startTransition] = useTransition();

  // Editor state
  const [editorName, setEditorName] = useState("");
  const [editorCategory, setEditorCategory] = useState<number>(4);
  const [editorLanguage, setEditorLanguage] = useState("en");
  const [editorSubject, setEditorSubject] = useState("");
  const [editorBody, setEditorBody] = useState("");
  const [isNewTemplate, setIsNewTemplate] = useState(false);

  // Preview state
  const [previewSubject, setPreviewSubject] = useState("");
  const [previewBody, setPreviewBody] = useState("");
  const [showPreview, setShowPreview] = useState(false);

  const bodyRef = useRef<HTMLTextAreaElement>(null);

  const filteredTemplates = useMemo(() => {
    let items = templates;
    if (search.trim()) {
      const term = search.toLowerCase();
      items = items.filter(
        (tpl) =>
          tpl.name.toLowerCase().includes(term) ||
          tpl.subject_template.toLowerCase().includes(term),
      );
    }
    if (categoryFilter) {
      items = items.filter(
        (tpl) => String(tpl.category) === categoryFilter,
      );
    }
    if (statusFilter) {
      const isActive = statusFilter === "active";
      items = items.filter((tpl) => tpl.is_active === isActive);
    }
    return items;
  }, [templates, search, categoryFilter, statusFilter]);

  const handleSelectTemplate = useCallback(
    (template: CommunicationTemplateDto) => {
      setSelectedTemplate(template);
      setEditorName(template.name);
      setEditorCategory(template.category);
      setEditorLanguage(template.language);
      setEditorSubject(template.subject_template);
      setEditorBody(template.body_template);
      setIsNewTemplate(false);
      setShowPreview(false);
      setErrorMessage("");
      setViewMode("editor");
    },
    [],
  );

  const handleNewTemplate = useCallback(() => {
    setSelectedTemplate(null);
    setEditorName("");
    setEditorCategory(4);
    setEditorLanguage("en");
    setEditorSubject("");
    setEditorBody("");
    setIsNewTemplate(true);
    setShowPreview(false);
    setErrorMessage("");
    setViewMode("editor");
  }, []);

  const handleBackToList = useCallback(() => {
    setViewMode("list");
    setSelectedTemplate(null);
    setShowPreview(false);
    setErrorMessage("");
  }, []);

  const handleInsertMergeField = useCallback(
    (fieldKey: string) => {
      const textarea = bodyRef.current;
      if (!textarea) return;

      const start = textarea.selectionStart;
      const end = textarea.selectionEnd;
      const mergeTag = `{{${fieldKey}}}`;
      const newBody =
        editorBody.substring(0, start) +
        mergeTag +
        editorBody.substring(end);

      setEditorBody(newBody);

      requestAnimationFrame(() => {
        textarea.focus();
        const cursorPos = start + mergeTag.length;
        textarea.setSelectionRange(cursorPos, cursorPos);
      });
    },
    [editorBody],
  );

  const handlePreview = useCallback(() => {
    const rendered = renderPreviewText(editorSubject, SAMPLE_MERGE_DATA);
    const renderedBody = renderPreviewText(editorBody, SAMPLE_MERGE_DATA);
    setPreviewSubject(rendered);
    setPreviewBody(renderedBody);
    setShowPreview(true);
  }, [editorSubject, editorBody]);

  const handleSave = useCallback(() => {
    if (!editorName.trim() || !editorSubject.trim() || !editorBody.trim()) {
      setErrorMessage(t.required_error);
      return;
    }

    setStatus("loading");
    setErrorMessage("");

    const params = {
      category: editorCategory,
      name: editorName.trim(),
      subject_template: editorSubject.trim(),
      body_template: editorBody.trim(),
      language: editorLanguage,
    };

    startTransition(async () => {
      const result = isNewTemplate
        ? await createNewTemplate(params)
        : await updateExistingTemplate(selectedTemplate!.id, params);

      if (result.success) {
        setStatus("success");
        const refreshResult = await fetchTemplates();
        if (refreshResult.success && refreshResult.data) {
          setTemplates(refreshResult.data);
        }
        setViewMode("list");
        setSelectedTemplate(null);
      } else {
        setStatus("failed");
        setErrorMessage(result.message || t.templates_save_error);
      }
    });
  }, [
    editorName,
    editorSubject,
    editorBody,
    editorCategory,
    editorLanguage,
    isNewTemplate,
    selectedTemplate,
    t,
    startTransition,
  ]);

  const handleCategoryChange = useCallback(
    (e: React.ChangeEvent<HTMLSelectElement>) => {
      setCategoryFilter(e.target.value);
    },
    [],
  );

  const handleStatusFilterChange = useCallback(
    (e: React.ChangeEvent<HTMLSelectElement>) => {
      setStatusFilter(e.target.value);
    },
    [],
  );

  if (viewMode === "editor") {
    return (
      <div className="space-y-6 p-6">
        <PageHeader
          title={t.templates_editor}
          breadcrumbs={[
            { label: t.nav_dashboard, href: "/dashboard" },
            { label: t.nav_communications },
            { label: t.templates_title, href: "/communications/templates" },
            {
              label: isNewTemplate
                ? t.templates_create
                : selectedTemplate?.name ?? "",
            },
          ]}
          actions={
            <div className="flex items-center gap-2">
              <Button
                variant="outline"
                size="sm"
                onClick={handleBackToList}
              >
                <ArrowLeft className="h-4 w-4" />
                {t.templates_back_to_list}
              </Button>
              <Button
                variant="outline"
                size="sm"
                onClick={handlePreview}
              >
                <Eye className="h-4 w-4" />
                {t.templates_preview}
              </Button>
              <Button
                size="sm"
                onClick={handleSave}
                disabled={isPending || status === "loading"}
              >
                {isPending || status === "loading"
                  ? t.templates_saving
                  : t.templates_save}
              </Button>
            </div>
          }
        />

        {errorMessage && (
          <div className="rounded-md border border-red-200 bg-red-50 p-3 text-sm text-red-800">
            {errorMessage}
          </div>
        )}

        {status === "success" && (
          <div className="rounded-md border border-green-200 bg-green-50 p-3 text-sm text-green-800">
            {t.templates_save_success}
          </div>
        )}

        <div className="grid gap-6 lg:grid-cols-2">
          {/* Editor Panel */}
          <div className="space-y-4">
            <div className="space-y-2">
              <label
                htmlFor="template-name"
                className="text-sm font-medium text-foreground"
              >
                {t.templates_name}
              </label>
              <Input
                id="template-name"
                value={editorName}
                onChange={(e) => setEditorName(e.target.value)}
                maxLength={200}
                disabled={isPending}
              />
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <label
                  htmlFor="template-category"
                  className="text-sm font-medium text-foreground"
                >
                  {t.templates_category}
                </label>
                <Select
                  id="template-category"
                  value={String(editorCategory)}
                  onChange={(e) =>
                    setEditorCategory(Number(e.target.value))
                  }
                  disabled={isPending}
                >
                  {TEMPLATE_CATEGORIES.map((cat) => (
                    <option key={cat.value} value={cat.value}>
                      {getCategoryLabel(cat.value, t)}
                    </option>
                  ))}
                </Select>
              </div>

              <div className="space-y-2">
                <label
                  htmlFor="template-language"
                  className="text-sm font-medium text-foreground"
                >
                  {t.templates_language}
                </label>
                <Select
                  id="template-language"
                  value={editorLanguage}
                  onChange={(e) => setEditorLanguage(e.target.value)}
                  disabled={isPending}
                >
                  <option value="en">English</option>
                  <option value="es">Español</option>
                </Select>
              </div>
            </div>

            <div className="space-y-2">
              <label
                htmlFor="template-subject"
                className="text-sm font-medium text-foreground"
              >
                {t.templates_subject}
              </label>
              <Input
                id="template-subject"
                value={editorSubject}
                onChange={(e) => setEditorSubject(e.target.value)}
                maxLength={500}
                disabled={isPending}
              />
            </div>

            <div className="space-y-2">
              <div className="flex items-center justify-between">
                <label
                  htmlFor="template-body"
                  className="text-sm font-medium text-foreground"
                >
                  {t.templates_body}
                </label>
              </div>

              {/* Merge Field Toolbar */}
              <div className="rounded-md border border-border bg-muted/50 p-2">
                <div className="mb-1.5 flex items-center gap-1.5 text-xs font-medium text-muted-foreground">
                  <Type className="h-3.5 w-3.5" />
                  {t.templates_merge_fields}
                </div>
                <div className="flex flex-wrap gap-1">
                  {MERGE_FIELDS.map((field) => (
                    <button
                      key={field.key}
                      type="button"
                      onClick={() => handleInsertMergeField(field.key)}
                      disabled={isPending}
                      className="inline-flex items-center rounded border border-border bg-background px-2 py-0.5 text-xs font-medium text-foreground transition-colors hover:bg-accent disabled:opacity-50"
                      title={`${t.templates_insert_field}: {{${field.key}}}`}
                    >
                      {field.label}
                    </button>
                  ))}
                </div>
              </div>

              <textarea
                ref={bodyRef}
                id="template-body"
                value={editorBody}
                onChange={(e) => setEditorBody(e.target.value)}
                disabled={isPending}
                rows={12}
                className="flex w-full rounded-md border border-border bg-background px-3 py-2 text-sm font-mono ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
              />
            </div>
          </div>

          {/* Preview Panel */}
          {showPreview && (
            <div className="space-y-4">
              <div className="rounded-md border border-border">
                <div className="border-b border-border bg-muted/50 px-4 py-2.5">
                  <h3 className="text-sm font-semibold text-foreground">
                    {t.templates_preview}
                  </h3>
                  <p className="text-xs text-muted-foreground">
                    {t.templates_preview_note}
                  </p>
                </div>
                <div className="p-4 space-y-3">
                  <div>
                    <span className="text-xs font-medium text-muted-foreground">
                      {t.templates_subject}:
                    </span>
                    <p className="text-sm font-medium text-foreground mt-0.5">
                      {previewSubject}
                    </p>
                  </div>
                  <div className="border-t border-border pt-3">
                    <span className="text-xs font-medium text-muted-foreground">
                      {t.templates_body}:
                    </span>
                    <div className="mt-1 whitespace-pre-wrap text-sm text-foreground">
                      {previewBody}
                    </div>
                  </div>
                </div>
              </div>
            </div>
          )}
        </div>
      </div>
    );
  }

  // List View
  return (
    <div className="space-y-6 p-6">
      <PageHeader
        title={t.templates_title}
        breadcrumbs={[
          { label: t.nav_dashboard, href: "/dashboard" },
          { label: t.nav_communications },
          { label: t.templates_title },
        ]}
        actions={
          <Button size="sm" onClick={handleNewTemplate}>
            <Plus className="h-4 w-4" />
            {t.templates_create}
          </Button>
        }
      />

      <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
        <div className="relative flex-1 max-w-sm">
          <Search
            className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground"
            aria-hidden="true"
          />
          <Input
            type="text"
            placeholder={t.templates_search_placeholder}
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="pl-9"
            aria-label={t.templates_search_placeholder}
          />
        </div>

        <Select
          value={categoryFilter}
          onChange={handleCategoryChange}
          className="h-9 w-full sm:w-48"
          aria-label={t.templates_filter_category}
        >
          <option value="">{t.templates_all_categories}</option>
          {TEMPLATE_CATEGORIES.map((cat) => (
            <option key={cat.value} value={cat.value}>
              {getCategoryLabel(cat.value, t)}
            </option>
          ))}
        </Select>

        <Select
          value={statusFilter}
          onChange={handleStatusFilterChange}
          className="h-9 w-full sm:w-40"
          aria-label={t.templates_filter_status}
        >
          <option value="">{t.templates_all_statuses}</option>
          <option value="active">{t.templates_active}</option>
          <option value="inactive">{t.templates_inactive}</option>
        </Select>
      </div>

      {filteredTemplates.length === 0 &&
      !search &&
      !categoryFilter &&
      !statusFilter ? (
        <EmptyState
          icon={FileText}
          title={t.templates_empty_title}
          message={t.templates_empty_message}
        />
      ) : filteredTemplates.length === 0 ? (
        <p className="py-8 text-center text-sm text-muted-foreground">
          {t.templates_no_results}
        </p>
      ) : (
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
          {filteredTemplates.map((template) => (
            <button
              key={template.id}
              type="button"
              onClick={() => handleSelectTemplate(template)}
              className={`w-full rounded-lg border p-4 text-left transition-colors hover:bg-accent ${
                selectedTemplate?.id === template.id
                  ? "border-[#E85320] bg-[#FFF8F5]"
                  : "border-border bg-card"
              }`}
            >
              <div className="flex items-start justify-between gap-2">
                <div className="min-w-0 flex-1">
                  <h3 className="truncate text-sm font-semibold text-foreground">
                    {template.name}
                  </h3>
                  <p className="mt-0.5 truncate text-xs text-muted-foreground">
                    {template.subject_template}
                  </p>
                </div>
                <Badge
                  className={`shrink-0 text-[10px] ${CATEGORY_STYLES[template.category] ?? ""}`}
                >
                  {getCategoryLabel(template.category, t)}
                </Badge>
              </div>

              <div className="mt-3 flex items-center gap-3 text-xs text-muted-foreground">
                <span className="uppercase">{template.language}</span>
                <span>v{template.version}</span>
                <span
                  className={`inline-flex items-center rounded-full px-1.5 py-0.5 text-[10px] font-semibold ${
                    template.is_active
                      ? "bg-green-100 text-green-800"
                      : "bg-gray-100 text-gray-600"
                  }`}
                >
                  {template.is_active
                    ? t.templates_active
                    : t.templates_inactive}
                </span>
                <span className="ml-auto">
                  {formatDate(template.updated_at)}
                </span>
              </div>
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
