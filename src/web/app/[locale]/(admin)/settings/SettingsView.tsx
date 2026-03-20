"use client";

import { useState, useCallback, useTransition } from "react";
import { useLocale } from "next-intl";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { PageHeader } from "@/app/_components/PageHeader";
import { Button } from "@/app/_components/ui/button";
import { Input } from "@/app/_components/ui/input";
import { Plus, Pencil, X } from "lucide-react";
import { fieldnames, type Locale } from "@/app/[locale]/_shared/app-fieldnames";
import type { PageStatus } from "@/app/[locale]/_shared/app-enums";
import {
  type ClientTierDto,
  type TierFormData,
  TierSchema,
} from "./shared";
import {
  createTierAction,
  updateTierAction,
  getTiersAction,
} from "./actions";

interface SettingsViewProps {
  initialTiers: ClientTierDto[];
}

export default function SettingsView({ initialTiers }: SettingsViewProps) {
  const locale = useLocale() as Locale;
  const t = fieldnames[locale];

  const [tiers, setTiers] = useState<ClientTierDto[]>(initialTiers);
  const [status, setStatus] = useState<PageStatus>("idle");
  const [errorMessage, setErrorMessage] = useState("");
  const [successMessage, setSuccessMessage] = useState("");
  const [editingTierId, setEditingTierId] = useState<string | null>(null);
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [isPending, startTransition] = useTransition();

  const createForm = useForm<TierFormData>({
    resolver: zodResolver(TierSchema(locale)),
    defaultValues: {
      name: "",
      sort_order: tiers.length + 1,
    },
  });

  const editForm = useForm<TierFormData>({
    resolver: zodResolver(TierSchema(locale)),
  });

  const refreshTiers = useCallback(async () => {
    const result = await getTiersAction();
    if (result.success && result.data) {
      setTiers(result.data);
    }
  }, []);

  const handleCreate = useCallback(
    (data: TierFormData) => {
      setStatus("loading");
      setErrorMessage("");
      setSuccessMessage("");

      startTransition(async () => {
        const result = await createTierAction({
          name: data.name,
          sort_order: data.sort_order,
        });

        if (result.success) {
          setStatus("success");
          setSuccessMessage(t.settings_tier_create_success);
          setShowCreateForm(false);
          createForm.reset({ name: "", sort_order: tiers.length + 2 });
          await refreshTiers();
        } else {
          setStatus("failed");
          setErrorMessage(result.message || t.settings_tier_create_error);
        }
      });
    },
    [t, startTransition, createForm, tiers.length, refreshTiers],
  );

  const handleStartEdit = useCallback(
    (tier: ClientTierDto) => {
      setEditingTierId(tier.tier_id);
      editForm.reset({
        name: tier.name,
        sort_order: tier.sort_order,
      });
      setErrorMessage("");
      setSuccessMessage("");
    },
    [editForm],
  );

  const handleCancelEdit = useCallback(() => {
    setEditingTierId(null);
    setErrorMessage("");
  }, []);

  const handleUpdate = useCallback(
    (data: TierFormData) => {
      if (!editingTierId) return;

      setStatus("loading");
      setErrorMessage("");
      setSuccessMessage("");

      startTransition(async () => {
        const result = await updateTierAction(editingTierId, {
          name: data.name,
          sort_order: data.sort_order,
        });

        if (result.success) {
          setStatus("success");
          setSuccessMessage(t.settings_tier_save_success);
          setEditingTierId(null);
          await refreshTiers();
        } else {
          setStatus("failed");
          setErrorMessage(result.message || t.settings_tier_save_error);
        }
      });
    },
    [editingTierId, t, startTransition, refreshTiers],
  );

  return (
    <div className="space-y-6 p-6">
      <PageHeader
        title={t.nav_settings}
        breadcrumbs={[
          { label: t.nav_dashboard, href: "/dashboard" },
          { label: t.nav_settings },
        ]}
      />

      {errorMessage && (
        <div className="rounded-md border border-red-200 bg-red-50 p-3 text-sm text-red-800">
          {errorMessage}
        </div>
      )}

      {successMessage && status === "success" && (
        <div className="rounded-md border border-green-200 bg-green-50 p-3 text-sm text-green-800">
          {successMessage}
        </div>
      )}

      {/* Tier Management Section */}
      <div className="space-y-4">
        <div className="flex items-center justify-between">
          <h2 className="text-lg font-semibold text-foreground">
            {t.settings_tiers_title}
          </h2>
          {!showCreateForm && (
            <Button
              size="sm"
              onClick={() => {
                setShowCreateForm(true);
                setErrorMessage("");
                setSuccessMessage("");
                createForm.reset({ name: "", sort_order: tiers.length + 1 });
              }}
              disabled={isPending}
              aria-label={t.settings_tier_create}
            >
              <Plus className="h-4 w-4" />
              <span className="hidden sm:inline">
                {t.settings_tier_create}
              </span>
            </Button>
          )}
        </div>

        {/* Create Tier Form */}
        {showCreateForm && (
          <form
            onSubmit={createForm.handleSubmit(handleCreate)}
            className="space-y-3 rounded-lg border border-border bg-card p-4"
          >
            <h3 className="text-sm font-medium text-foreground">
              {t.settings_tier_create}
            </h3>
            <div className="grid gap-3 sm:grid-cols-2">
              <div className="space-y-1">
                <label
                  htmlFor="create-tier-name"
                  className="text-sm font-medium text-foreground"
                >
                  {t.settings_tier_name} *
                </label>
                <Input
                  id="create-tier-name"
                  {...createForm.register("name")}
                  disabled={isPending}
                  maxLength={100}
                />
                {createForm.formState.errors.name && (
                  <p className="text-xs text-red-600">
                    {createForm.formState.errors.name.message}
                  </p>
                )}
              </div>
              <div className="space-y-1">
                <label
                  htmlFor="create-tier-sort"
                  className="text-sm font-medium text-foreground"
                >
                  {t.settings_tier_sort_order}
                </label>
                <Input
                  id="create-tier-sort"
                  type="number"
                  min={0}
                  {...createForm.register("sort_order", { valueAsNumber: true })}
                  disabled={isPending}
                />
                {createForm.formState.errors.sort_order && (
                  <p className="text-xs text-red-600">
                    {createForm.formState.errors.sort_order.message}
                  </p>
                )}
              </div>
            </div>
            <div className="flex items-center gap-2">
              <Button
                type="submit"
                size="sm"
                disabled={isPending || status === "loading"}
              >
                {isPending || status === "loading"
                  ? t.settings_tier_creating
                  : t.settings_tier_create}
              </Button>
              <Button
                type="button"
                variant="outline"
                size="sm"
                onClick={() => setShowCreateForm(false)}
                disabled={isPending}
              >
                {t.settings_cancel}
              </Button>
            </div>
          </form>
        )}

        {/* Tier List */}
        <div className="rounded-lg border border-border bg-card">
          {tiers.length === 0 ? (
            <div className="p-6 text-center text-sm text-muted-foreground">
              {t.settings_tiers_empty}
            </div>
          ) : (
            <div className="divide-y divide-border">
              {tiers.map((tier) => (
                <div key={tier.tier_id}>
                  {editingTierId === tier.tier_id ? (
                    <form
                      onSubmit={editForm.handleSubmit(handleUpdate)}
                      className="flex flex-col gap-3 p-4 sm:flex-row sm:items-end"
                    >
                      <div className="flex-1 space-y-1">
                        <label
                          htmlFor={`edit-tier-name-${tier.tier_id}`}
                          className="text-sm font-medium text-foreground"
                        >
                          {t.settings_tier_name}
                        </label>
                        <Input
                          id={`edit-tier-name-${tier.tier_id}`}
                          {...editForm.register("name")}
                          disabled={isPending}
                          maxLength={100}
                        />
                        {editForm.formState.errors.name && (
                          <p className="text-xs text-red-600">
                            {editForm.formState.errors.name.message}
                          </p>
                        )}
                      </div>
                      <div className="w-full space-y-1 sm:w-32">
                        <label
                          htmlFor={`edit-tier-sort-${tier.tier_id}`}
                          className="text-sm font-medium text-foreground"
                        >
                          {t.settings_tier_sort_order}
                        </label>
                        <Input
                          id={`edit-tier-sort-${tier.tier_id}`}
                          type="number"
                          min={0}
                          {...editForm.register("sort_order", { valueAsNumber: true })}
                          disabled={isPending}
                        />
                      </div>
                      <div className="flex items-center gap-2">
                        <Button
                          type="submit"
                          size="sm"
                          disabled={isPending || status === "loading"}
                        >
                          {isPending || status === "loading"
                            ? t.settings_tier_saving
                            : t.settings_tier_save}
                        </Button>
                        <Button
                          type="button"
                          variant="outline"
                          size="sm"
                          onClick={handleCancelEdit}
                          disabled={isPending}
                        >
                          <X className="h-4 w-4" />
                        </Button>
                      </div>
                    </form>
                  ) : (
                    <div className="flex items-center justify-between p-4">
                      <div className="flex items-center gap-4">
                        <span className="inline-flex h-8 w-8 items-center justify-center rounded-full bg-muted text-sm font-medium text-muted-foreground">
                          {tier.sort_order}
                        </span>
                        <span className="text-sm font-medium text-foreground">
                          {tier.name}
                        </span>
                      </div>
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => handleStartEdit(tier)}
                        disabled={isPending || editingTierId !== null}
                        aria-label={`${t.settings_tier_edit} ${tier.name}`}
                      >
                        <Pencil className="h-4 w-4" />
                        <span className="hidden sm:inline">
                          {t.settings_tier_edit}
                        </span>
                      </Button>
                    </div>
                  )}
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
