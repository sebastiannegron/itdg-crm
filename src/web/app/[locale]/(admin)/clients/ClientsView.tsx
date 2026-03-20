"use client";

import { useState, useCallback, useMemo } from "react";
import { useLocale } from "next-intl";
import { useRouter } from "@/i18n/routing";
import { DataTable, type DataTableColumn } from "@/app/_components/DataTable";
import { PageHeader } from "@/app/_components/PageHeader";
import { TierBadge } from "@/app/_components/TierBadge";
import { StatusBadge } from "@/app/_components/StatusBadge";
import { EmptyState } from "@/app/_components/EmptyState";
import { Select } from "@/app/_components/ui/select";
import { ChevronRight, Users } from "lucide-react";
import { fieldnames, type Locale } from "@/app/[locale]/_shared/app-fieldnames";
import {
  type ClientDto,
  type PaginatedResult,
  CLIENT_STATUSES,
} from "./shared";

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

interface ClientsViewProps {
  initialData: PaginatedResult<ClientDto>;
}

export default function ClientsView({ initialData }: ClientsViewProps) {
  const locale = useLocale() as Locale;
  const t = fieldnames[locale];
  const router = useRouter();

  const [data] = useState(initialData);
  const [statusFilter, setStatusFilter] = useState("");
  const [tierFilter, setTierFilter] = useState("");

  const filteredItems = useMemo(() => {
    let items = data.items;
    if (statusFilter) {
      items = items.filter((c) => c.status === statusFilter);
    }
    if (tierFilter) {
      items = items.filter((c) => {
        const tier = parseTierNumber(c.tier_name);
        return tier !== null && String(tier) === tierFilter;
      });
    }
    return items;
  }, [data.items, statusFilter, tierFilter]);

  const handleStatusChange = useCallback(
    (e: React.ChangeEvent<HTMLSelectElement>) => {
      setStatusFilter(e.target.value);
    },
    [],
  );

  const handleTierChange = useCallback(
    (e: React.ChangeEvent<HTMLSelectElement>) => {
      setTierFilter(e.target.value);
    },
    [],
  );

  const handleRowClick = useCallback(
    (client: ClientDto) => {
      router.push(`/clients/${client.client_id}`);
    },
    [router],
  );

  const columns: DataTableColumn<ClientDto>[] = useMemo(
    () => [
      {
        key: "name",
        header: t.clients_name,
        sortable: true,
        render: (row) => (
          <div>
            <div className="font-medium text-foreground">{row.name}</div>
            {row.contact_email && (
              <div className="text-xs text-muted-foreground">
                {row.contact_email}
              </div>
            )}
          </div>
        ),
      },
      {
        key: "tier_name",
        header: t.clients_tier,
        sortable: true,
        render: (row) => {
          const tier = parseTierNumber(row.tier_name);
          return tier ? <TierBadge tier={tier} /> : <span className="text-muted-foreground">—</span>;
        },
        accessorFn: (row) => row.tier_name ?? "",
      },
      {
        key: "status",
        header: t.clients_status,
        sortable: true,
        render: (row) => <StatusBadge status={row.status} />,
      },
      {
        key: "industry_tag",
        header: t.clients_industry,
        sortable: true,
        render: (row) => (
          <span className="text-sm">
            {row.industry_tag ?? "—"}
          </span>
        ),
        accessorFn: (row) => row.industry_tag ?? "",
      },
      {
        key: "updated_at",
        header: t.clients_last_activity,
        sortable: true,
        render: (row) => (
          <span className="text-sm text-muted-foreground">
            {formatDate(row.updated_at)}
          </span>
        ),
        accessorFn: (row) => new Date(row.updated_at).getTime(),
      },
      {
        key: "actions",
        header: "",
        render: (row) => (
          <button
            type="button"
            onClick={() => handleRowClick(row)}
            className="p-1 text-muted-foreground hover:text-foreground transition-colors"
            aria-label={`${t.clients_view_details} ${row.name}`}
          >
            <ChevronRight className="h-4 w-4" />
          </button>
        ),
      },
    ],
    [t, handleRowClick],
  );

  const renderCard = useCallback(
    (client: ClientDto) => {
      const tier = parseTierNumber(client.tier_name);
      return (
        <button
          type="button"
          onClick={() => handleRowClick(client)}
          className="w-full rounded-md border border-border bg-card p-4 text-left transition-colors hover:bg-accent"
        >
          <div className="flex items-start justify-between gap-2">
            <div className="min-w-0 flex-1">
              <div className="flex items-center gap-2">
                <span className="truncate font-medium text-foreground">
                  {client.name}
                </span>
                {tier && <TierBadge tier={tier} />}
              </div>
              {(client.contact_email || client.industry_tag) && (
                <div className="mt-1 text-xs text-muted-foreground">
                  {client.contact_email ?? client.industry_tag}
                </div>
              )}
            </div>
            <div className="flex flex-col items-end gap-1">
              <StatusBadge status={client.status} />
            </div>
          </div>
        </button>
      );
    },
    [handleRowClick],
  );

  return (
    <div className="space-y-6 p-6">
      <PageHeader
        title={t.nav_clients}
        breadcrumbs={[
          { label: t.nav_dashboard, href: "/dashboard" },
          { label: t.nav_clients },
        ]}
      />

      <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
        <Select
          value={statusFilter}
          onChange={handleStatusChange}
          className="h-9 w-full sm:w-40"
          aria-label={t.clients_filter_status}
        >
          <option value="">{t.clients_all_statuses}</option>
          {CLIENT_STATUSES.map((s) => (
            <option key={s} value={s}>
              {s}
            </option>
          ))}
        </Select>

        <Select
          value={tierFilter}
          onChange={handleTierChange}
          className="h-9 w-full sm:w-40"
          aria-label={t.clients_filter_tier}
        >
          <option value="">{t.clients_all_tiers}</option>
          <option value="1">Tier 1</option>
          <option value="2">Tier 2</option>
          <option value="3">Tier 3</option>
        </Select>
      </div>

      {filteredItems.length === 0 && !statusFilter && !tierFilter ? (
        <EmptyState
          icon={Users}
          title={t.clients_empty_title}
          message={t.clients_empty_message}
        />
      ) : (
        <DataTable
          columns={columns}
          data={filteredItems}
          pageSize={20}
          searchable
          searchPlaceholder={t.clients_search_placeholder}
          renderCard={renderCard}
          emptyMessage={t.clients_no_results}
        />
      )}
    </div>
  );
}
