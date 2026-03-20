"use client";

import { useState, useCallback, useTransition } from "react";
import { useLocale } from "next-intl";
import { useRouter } from "@/i18n/routing";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { PageHeader } from "@/app/_components/PageHeader";
import { Button } from "@/app/_components/ui/button";
import { Input } from "@/app/_components/ui/input";
import { Select } from "@/app/_components/ui/select";
import { ArrowLeft } from "lucide-react";
import { fieldnames, type Locale } from "@/app/[locale]/_shared/app-fieldnames";
import type { PageStatus } from "@/app/[locale]/_shared/app-enums";
import {
  type CreateClientFormData,
  CLIENT_STATUSES,
  CreateClientSchema,
} from "./shared";
import { createClientAction } from "./actions";

export default function ClientForm() {
  const locale = useLocale() as Locale;
  const t = fieldnames[locale];
  const router = useRouter();

  const [status, setStatus] = useState<PageStatus>("idle");
  const [errorMessage, setErrorMessage] = useState("");
  const [isPending, startTransition] = useTransition();

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<CreateClientFormData>({
    resolver: zodResolver(CreateClientSchema(locale)),
    defaultValues: {
      name: "",
      contact_email: "",
      phone: "",
      address: "",
      status: "Active",
      industry_tag: "",
      notes: "",
    },
  });

  const handleBack = useCallback(() => {
    router.push("/clients");
  }, [router]);

  const handleCreate = useCallback(
    (data: CreateClientFormData) => {
      setStatus("loading");
      setErrorMessage("");

      startTransition(async () => {
        const result = await createClientAction({
          name: data.name,
          contact_email: data.contact_email || undefined,
          phone: data.phone || undefined,
          address: data.address || undefined,
          status: data.status,
          industry_tag: data.industry_tag || undefined,
          notes: data.notes || undefined,
        });

        if (result.success && result.data) {
          setStatus("success");
          router.push(`/clients/${result.data.client_id}`);
        } else {
          setStatus("failed");
          setErrorMessage(result.message || t.clients_create_error);
        }
      });
    },
    [t, router, startTransition],
  );

  return (
    <div className="space-y-6 p-6">
      <PageHeader
        title={t.clients_new_client}
        breadcrumbs={[
          { label: t.nav_dashboard, href: "/dashboard" },
          { label: t.nav_clients, href: "/clients" },
          { label: t.clients_new_client },
        ]}
        actions={
          <Button variant="outline" size="sm" onClick={handleBack}>
            <ArrowLeft className="h-4 w-4" />
            <span className="hidden sm:inline">{t.clients_back_to_list}</span>
          </Button>
        }
      />

      {errorMessage && (
        <div className="rounded-md border border-red-200 bg-red-50 p-3 text-sm text-red-800">
          {errorMessage}
        </div>
      )}

      {status === "success" && (
        <div className="rounded-md border border-green-200 bg-green-50 p-3 text-sm text-green-800">
          {t.clients_create_success}
        </div>
      )}

      <form
        onSubmit={handleSubmit(handleCreate)}
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
              ? t.clients_creating
              : t.clients_create}
          </Button>
          <Button
            type="button"
            variant="outline"
            size="sm"
            onClick={handleBack}
            disabled={isPending}
          >
            {t.clients_back_to_list}
          </Button>
        </div>
      </form>
    </div>
  );
}
