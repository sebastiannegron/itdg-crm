"use client";

import { useState, useCallback, useTransition } from "react";
import { useLocale } from "next-intl";
import { useRouter } from "@/i18n/routing";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { PageHeader } from "@/app/_components/PageHeader";
import { EmptyState } from "@/app/_components/EmptyState";
import { TierBadge } from "@/app/_components/TierBadge";
import { StatusBadge } from "@/app/_components/StatusBadge";
import { Button } from "@/app/_components/ui/button";
import { Input } from "@/app/_components/ui/input";
import { Select } from "@/app/_components/ui/select";
import {
  ArrowLeft,
  Mail,
  FileText,
  CheckSquare,
  UserX,
  Phone,
  MapPin,
  Building2,
  StickyNote,
} from "lucide-react";
import { fieldnames, type Locale } from "@/app/[locale]/_shared/app-fieldnames";
import type { PageStatus } from "@/app/[locale]/_shared/app-enums";
import {
  type ClientDto,
  type ClientAssignmentDto,
  type UpdateClientFormData,
  type DetailTab,
  DETAIL_TABS,
  CLIENT_STATUSES,
  UpdateClientSchema,
} from "./shared";
import { updateClientAction } from "./actions";
import ClientAssignmentsPanel, {
  type AssociateOption,
} from "./ClientAssignmentsPanel";
import ClientDocumentsTab from "./ClientDocumentsTab";
import ClientEmailsTab from "./ClientEmailsTab";
import ClientTimeline from "./ClientTimeline";

function parseTierNumber(tierName: string | null): 1 | 2 | 3 | null {
  if (!tierName) return null;
  const match = tierName.match(/(\d)/);
  if (match) {
    const num = parseInt(match[1], 10);
    if (num >= 1 && num <= 3) return num as 1 | 2 | 3;
  }
  return null;
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

interface ClientDetailViewProps {
  client: ClientDto | null;
  assignments?: ClientAssignmentDto[];
  users?: AssociateOption[];
}

export default function ClientDetailView({
  client,
  assignments = [],
  users = [],
}: ClientDetailViewProps) {
  const locale = useLocale() as Locale;
  const t = fieldnames[locale];
  const router = useRouter();

  const [activeTab, setActiveTab] = useState<DetailTab>("overview");
  const [isEditing, setIsEditing] = useState(false);
  const [status, setStatus] = useState<PageStatus>("idle");
  const [errorMessage, setErrorMessage] = useState("");
  const [isPending, startTransition] = useTransition();

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<UpdateClientFormData>({
    resolver: zodResolver(UpdateClientSchema(locale)),
    defaultValues: {
      name: client?.name ?? "",
      contact_email: client?.contact_email ?? "",
      phone: client?.phone ?? "",
      address: client?.address ?? "",
      status: (client?.status as UpdateClientFormData["status"]) ?? "Active",
      industry_tag: client?.industry_tag ?? "",
      notes: client?.notes ?? "",
    },
  });

  const handleBack = useCallback(() => {
    router.push("/clients");
  }, [router]);

  const handleCancelEdit = useCallback(() => {
    setIsEditing(false);
    setErrorMessage("");
    reset();
  }, [reset]);

  const handleSave = useCallback(
    (data: UpdateClientFormData) => {
      if (!client) return;

      setStatus("loading");
      setErrorMessage("");

      startTransition(async () => {
        const result = await updateClientAction(client.client_id, {
          name: data.name,
          contact_email: data.contact_email || undefined,
          phone: data.phone || undefined,
          address: data.address || undefined,
          status: data.status,
          industry_tag: data.industry_tag || undefined,
          notes: data.notes || undefined,
        });

        if (result.success) {
          setStatus("success");
          setIsEditing(false);
        } else {
          setStatus("failed");
          setErrorMessage(result.message || t.clients_save_error);
        }
      });
    },
    [client, t, router, startTransition],
  );

  if (!client) {
    return (
      <div className="space-y-6 p-6">
        <PageHeader
          title={t.nav_clients}
          breadcrumbs={[
            { label: t.nav_dashboard, href: "/dashboard" },
            { label: t.nav_clients, href: "/clients" },
          ]}
        />
        <EmptyState
          icon={UserX}
          title={t.clients_empty_title}
          message={t.clients_empty_message}
        />
      </div>
    );
  }

  const tier = parseTierNumber(client.tier_name);

  const tabLabels: Record<DetailTab, string> = {
    overview: t.clients_tab_overview,
    documents: t.clients_tab_documents,
    communications: t.clients_tab_communications,
    tasks: t.clients_tab_tasks,
  };

  return (
    <div className="space-y-6 p-6">
      {/* Header */}
      <PageHeader
        title={client.name}
        breadcrumbs={[
          { label: t.nav_dashboard, href: "/dashboard" },
          { label: t.nav_clients, href: "/clients" },
          { label: client.name },
        ]}
        actions={
          <div className="flex items-center gap-2">
            <Button variant="outline" size="sm" onClick={handleBack}>
              <ArrowLeft className="h-4 w-4" />
              <span className="hidden sm:inline">{t.clients_back_to_list}</span>
            </Button>
            {!isEditing && activeTab === "overview" && (
              <Button size="sm" onClick={() => setIsEditing(true)}>
                {t.clients_edit_client}
              </Button>
            )}
          </div>
        }
      />

      {/* Client Header Info */}
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between rounded-lg border border-border bg-card p-4">
        <div className="flex items-center gap-3">
          <h2 className="text-lg font-semibold text-foreground">{client.name}</h2>
          {tier && <TierBadge tier={tier} />}
        </div>
        <StatusBadge status={client.status} />
      </div>

      {/* Action Buttons */}
      <div className="flex flex-col gap-2 sm:flex-row sm:items-center">
        <Button variant="outline" size="sm">
          <Mail className="h-4 w-4" />
          {t.clients_view_emails}
        </Button>
        <Button variant="outline" size="sm">
          <FileText className="h-4 w-4" />
          {t.clients_documents}
        </Button>
        <Button size="sm">
          <CheckSquare className="h-4 w-4" />
          {t.clients_tasks}
        </Button>
      </div>

      {/* Tabs */}
      <div className="border-b border-border">
        <nav className="-mb-px flex gap-4 overflow-x-auto" aria-label="Tabs">
          {DETAIL_TABS.map((tab) => (
            <button
              key={tab}
              type="button"
              onClick={() => setActiveTab(tab)}
              className={`whitespace-nowrap border-b-2 px-1 py-2 text-sm font-medium transition-colors ${
                activeTab === tab
                  ? "border-primary text-primary"
                  : "border-transparent text-muted-foreground hover:border-border hover:text-foreground"
              }`}
              aria-current={activeTab === tab ? "page" : undefined}
            >
              {tabLabels[tab]}
            </button>
          ))}
        </nav>
      </div>

      {/* Tab Content */}
      {activeTab === "overview" && (
        <>
          {errorMessage && (
            <div className="rounded-md border border-red-200 bg-red-50 p-3 text-sm text-red-800">
              {errorMessage}
            </div>
          )}

          {status === "success" && (
            <div className="rounded-md border border-green-200 bg-green-50 p-3 text-sm text-green-800">
              {t.clients_save_success}
            </div>
          )}

          {isEditing ? (
            <form
              onSubmit={handleSubmit(handleSave)}
              className="space-y-4 rounded-lg border border-border bg-card p-4"
            >
              <div className="grid gap-4 sm:grid-cols-2">
                <div className="space-y-2">
                  <label
                    htmlFor="client-name"
                    className="text-sm font-medium text-foreground"
                  >
                    {t.clients_name} *
                  </label>
                  <Input
                    id="client-name"
                    {...register("name")}
                    disabled={isPending}
                    maxLength={200}
                  />
                  {errors.name && (
                    <p className="text-xs text-red-600">{errors.name.message}</p>
                  )}
                </div>

                <div className="space-y-2">
                  <label
                    htmlFor="client-email"
                    className="text-sm font-medium text-foreground"
                  >
                    {t.clients_email}
                  </label>
                  <Input
                    id="client-email"
                    type="email"
                    {...register("contact_email")}
                    disabled={isPending}
                    maxLength={200}
                  />
                  {errors.contact_email && (
                    <p className="text-xs text-red-600">
                      {errors.contact_email.message}
                    </p>
                  )}
                </div>

                <div className="space-y-2">
                  <label
                    htmlFor="client-phone"
                    className="text-sm font-medium text-foreground"
                  >
                    {t.clients_phone}
                  </label>
                  <Input
                    id="client-phone"
                    {...register("phone")}
                    disabled={isPending}
                    maxLength={50}
                  />
                  {errors.phone && (
                    <p className="text-xs text-red-600">{errors.phone.message}</p>
                  )}
                </div>

                <div className="space-y-2">
                  <label
                    htmlFor="client-status"
                    className="text-sm font-medium text-foreground"
                  >
                    {t.clients_status} *
                  </label>
                  <Select
                    id="client-status"
                    {...register("status")}
                    disabled={isPending}
                  >
                    {CLIENT_STATUSES.map((s) => (
                      <option key={s} value={s}>
                        {s}
                      </option>
                    ))}
                  </Select>
                  {errors.status && (
                    <p className="text-xs text-red-600">{errors.status.message}</p>
                  )}
                </div>

                <div className="space-y-2">
                  <label
                    htmlFor="client-industry"
                    className="text-sm font-medium text-foreground"
                  >
                    {t.clients_industry}
                  </label>
                  <Input
                    id="client-industry"
                    {...register("industry_tag")}
                    disabled={isPending}
                    maxLength={100}
                  />
                  {errors.industry_tag && (
                    <p className="text-xs text-red-600">
                      {errors.industry_tag.message}
                    </p>
                  )}
                </div>

                <div className="space-y-2 sm:col-span-2">
                  <label
                    htmlFor="client-address"
                    className="text-sm font-medium text-foreground"
                  >
                    {t.clients_address}
                  </label>
                  <Input
                    id="client-address"
                    {...register("address")}
                    disabled={isPending}
                    maxLength={500}
                  />
                  {errors.address && (
                    <p className="text-xs text-red-600">{errors.address.message}</p>
                  )}
                </div>

                <div className="space-y-2 sm:col-span-2">
                  <label
                    htmlFor="client-notes"
                    className="text-sm font-medium text-foreground"
                  >
                    {t.clients_notes}
                  </label>
                  <textarea
                    id="client-notes"
                    {...register("notes")}
                    disabled={isPending}
                    rows={3}
                    maxLength={2000}
                    className="flex w-full rounded-md border border-border bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                  />
                  {errors.notes && (
                    <p className="text-xs text-red-600">{errors.notes.message}</p>
                  )}
                </div>
              </div>

              <div className="flex items-center gap-2 pt-2">
                <Button
                  type="submit"
                  size="sm"
                  disabled={isPending || status === "loading"}
                >
                  {isPending || status === "loading"
                    ? t.clients_saving
                    : t.clients_save}
                </Button>
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  onClick={handleCancelEdit}
                  disabled={isPending}
                >
                  {t.clients_cancel}
                </Button>
              </div>
            </form>
          ) : (
            <div className="rounded-lg border border-border bg-card p-4">
              <div className="grid gap-4 sm:grid-cols-2">
                <InfoField
                  icon={<Mail className="h-4 w-4" />}
                  label={t.clients_email}
                  value={client.contact_email}
                />
                <InfoField
                  icon={<Phone className="h-4 w-4" />}
                  label={t.clients_phone}
                  value={client.phone}
                />
                <InfoField
                  icon={<Building2 className="h-4 w-4" />}
                  label={t.clients_industry}
                  value={client.industry_tag}
                />
                <InfoField
                  icon={<MapPin className="h-4 w-4" />}
                  label={t.clients_address}
                  value={client.address}
                />
                <InfoField
                  label={t.clients_last_activity}
                  value={formatDate(client.updated_at)}
                />
                {client.notes && (
                  <div className="sm:col-span-2">
                    <InfoField
                      icon={<StickyNote className="h-4 w-4" />}
                      label={t.clients_notes}
                      value={client.notes}
                    />
                  </div>
                )}
              </div>
            </div>
          )}
        </>
      )}

      {activeTab === "overview" && !isEditing && client && (
        <ClientAssignmentsPanel
          clientId={client.client_id}
          initialAssignments={assignments}
          users={users}
        />
      )}

      {activeTab === "overview" && !isEditing && client && (
        <ClientTimeline clientId={client.client_id} />
      )}

      {activeTab === "documents" && client && (
        <ClientDocumentsTab clientId={client.client_id} />
      )}

      {activeTab === "communications" && client && (
        <ClientEmailsTab
          clientId={client.client_id}
          clientEmail={client.contact_email}
          clientName={client.name}
        />
      )}

      {activeTab === "tasks" && (
        <EmptyState
          icon={CheckSquare}
          title={t.clients_tab_tasks}
          message={t.clients_placeholder_tasks}
        />
      )}
    </div>
  );
}

function InfoField({
  icon,
  label,
  value,
}: {
  icon?: React.ReactNode;
  label: string;
  value: string | null;
}) {
  return (
    <div className="space-y-1">
      <div className="flex items-center gap-1.5 text-xs font-medium text-muted-foreground">
        {icon}
        {label}
      </div>
      <p className="text-sm text-foreground">{value ?? "—"}</p>
    </div>
  );
}
