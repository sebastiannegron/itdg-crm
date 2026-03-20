"use client";

import { useState, useCallback, useTransition } from "react";
import { useLocale } from "next-intl";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Link } from "@/i18n/routing";
import { PageHeader } from "@/app/_components/PageHeader";
import { Button } from "@/app/_components/ui/button";
import { Select } from "@/app/_components/ui/select";
import { Badge } from "@/app/_components/ui/badge";
import { ArrowLeft } from "lucide-react";
import { fieldnames, type Locale } from "@/app/[locale]/_shared/app-fieldnames";
import type { PageStatus } from "@/app/[locale]/_shared/app-enums";
import {
  type UserDto,
  type UpdateUserFormData,
  USER_ROLES,
  UpdateUserSchema,
  roleLabel,
} from "./shared";
import { updateUserAction, getUserAction } from "./actions";

interface UserDetailViewProps {
  user: UserDto | null;
}

export default function UserDetailView({ user: initialUser }: UserDetailViewProps) {
  const locale = useLocale() as Locale;
  const t = fieldnames[locale];

  const [user, setUser] = useState<UserDto | null>(initialUser);
  const [isEditing, setIsEditing] = useState(false);
  const [status, setStatus] = useState<PageStatus>("idle");
  const [errorMessage, setErrorMessage] = useState("");
  const [successMessage, setSuccessMessage] = useState("");
  const [isPending, startTransition] = useTransition();

  const editForm = useForm<UpdateUserFormData>({
    resolver: zodResolver(UpdateUserSchema(locale)),
    defaultValues: {
      role: (user?.role as UpdateUserFormData["role"]) ?? "Associate",
      is_active: user?.is_active ?? true,
    },
  });

  const handleStartEdit = useCallback(() => {
    if (!user) return;
    editForm.reset({
      role: user.role as UpdateUserFormData["role"],
      is_active: user.is_active,
    });
    setIsEditing(true);
    setErrorMessage("");
    setSuccessMessage("");
  }, [user, editForm]);

  const handleCancelEdit = useCallback(() => {
    setIsEditing(false);
    setErrorMessage("");
  }, []);

  const handleUpdate = useCallback(
    (data: UpdateUserFormData) => {
      if (!user) return;

      setStatus("loading");
      setErrorMessage("");
      setSuccessMessage("");

      startTransition(async () => {
        const result = await updateUserAction(user.user_id, {
          role: data.role,
          is_active: data.is_active,
        });

        if (result.success) {
          setStatus("success");
          setSuccessMessage(t.users_save_success);
          setIsEditing(false);

          const refreshResult = await getUserAction(user.user_id);
          if (refreshResult.success && refreshResult.data) {
            setUser(refreshResult.data);
          }
        } else {
          setStatus("failed");
          setErrorMessage(result.message || t.users_save_error);
        }
      });
    },
    [user, t, startTransition],
  );

  if (!user) {
    return (
      <div className="space-y-6 p-6">
        <PageHeader
          title={t.users_edit_user}
          breadcrumbs={[
            { label: t.nav_dashboard, href: "/dashboard" },
            { label: t.nav_settings, href: "/settings" },
            { label: t.users_title, href: "/settings/users" },
            { label: t.users_edit_user },
          ]}
        />
        <div className="rounded-lg border border-border bg-card p-8 text-center text-sm text-muted-foreground">
          User not found.
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6 p-6">
      <PageHeader
        title={user.display_name}
        breadcrumbs={[
          { label: t.nav_dashboard, href: "/dashboard" },
          { label: t.nav_settings, href: "/settings" },
          { label: t.users_title, href: "/settings/users" },
          { label: user.display_name },
        ]}
        actions={
          <Link href="/settings/users">
            <Button variant="outline" size="sm">
              <ArrowLeft className="h-4 w-4" />
              <span className="hidden sm:inline">{t.users_back_to_list}</span>
            </Button>
          </Link>
        }
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

      <div className="rounded-lg border border-border bg-card">
        {/* User Details */}
        <div className="space-y-4 p-6">
          <div className="grid gap-4 sm:grid-cols-2">
            <div className="space-y-1">
              <p className="text-sm font-medium text-muted-foreground">
                {t.users_name}
              </p>
              <p className="text-sm text-foreground">{user.display_name}</p>
            </div>
            <div className="space-y-1">
              <p className="text-sm font-medium text-muted-foreground">
                {t.users_email}
              </p>
              <p className="text-sm text-foreground">{user.email}</p>
            </div>
          </div>

          {isEditing ? (
            <form
              onSubmit={editForm.handleSubmit(handleUpdate)}
              className="space-y-4 border-t border-border pt-4"
            >
              <div className="grid gap-4 sm:grid-cols-2">
                <div className="space-y-1">
                  <label
                    htmlFor="edit-user-role"
                    className="text-sm font-medium text-foreground"
                  >
                    {t.users_role}
                  </label>
                  <Select
                    id="edit-user-role"
                    {...editForm.register("role")}
                    disabled={isPending}
                  >
                    {USER_ROLES.map((role) => (
                      <option key={role} value={role}>
                        {roleLabel(role, locale)}
                      </option>
                    ))}
                  </Select>
                  {editForm.formState.errors.role && (
                    <p className="text-xs text-red-600">
                      {editForm.formState.errors.role.message}
                    </p>
                  )}
                </div>
                <div className="space-y-1">
                  <label
                    htmlFor="edit-user-status"
                    className="text-sm font-medium text-foreground"
                  >
                    {t.users_status}
                  </label>
                  <Controller
                    name="is_active"
                    control={editForm.control}
                    render={({ field }) => (
                      <Select
                        id="edit-user-status"
                        value={field.value ? "true" : "false"}
                        onChange={(e) =>
                          field.onChange(e.target.value === "true")
                        }
                        disabled={isPending}
                      >
                        <option value="true">{t.users_active}</option>
                        <option value="false">{t.users_inactive}</option>
                      </Select>
                    )}
                  />
                </div>
              </div>
              <div className="flex items-center gap-2">
                <Button
                  type="submit"
                  size="sm"
                  disabled={isPending || status === "loading"}
                >
                  {isPending || status === "loading"
                    ? t.users_saving
                    : t.users_save}
                </Button>
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  onClick={handleCancelEdit}
                  disabled={isPending}
                >
                  {t.users_cancel}
                </Button>
              </div>
            </form>
          ) : (
            <div className="space-y-4 border-t border-border pt-4">
              <div className="grid gap-4 sm:grid-cols-2">
                <div className="space-y-1">
                  <p className="text-sm font-medium text-muted-foreground">
                    {t.users_role}
                  </p>
                  <Badge variant="secondary" className="text-xs">
                    {roleLabel(user.role, locale)}
                  </Badge>
                </div>
                <div className="space-y-1">
                  <p className="text-sm font-medium text-muted-foreground">
                    {t.users_status}
                  </p>
                  <Badge
                    variant={user.is_active ? "default" : "outline"}
                    className="text-xs"
                  >
                    {user.is_active ? t.users_active : t.users_inactive}
                  </Badge>
                </div>
              </div>
              <div className="grid gap-4 sm:grid-cols-2">
                <div className="space-y-1">
                  <p className="text-sm font-medium text-muted-foreground">
                    {t.users_last_login}
                  </p>
                  <p className="text-sm text-foreground">{t.users_never}</p>
                </div>
              </div>
              <Button
                size="sm"
                onClick={handleStartEdit}
                disabled={isPending}
              >
                {t.users_edit_user}
              </Button>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
