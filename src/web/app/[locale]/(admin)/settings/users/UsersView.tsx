"use client";

import { useState, useCallback, useTransition, useMemo } from "react";
import { useLocale } from "next-intl";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Link } from "@/i18n/routing";
import { PageHeader } from "@/app/_components/PageHeader";
import { Button } from "@/app/_components/ui/button";
import { Input } from "@/app/_components/ui/input";
import { Select } from "@/app/_components/ui/select";
import { Badge } from "@/app/_components/ui/badge";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from "@/app/_components/ui/dialog";
import { Plus, Search } from "lucide-react";
import { fieldnames, type Locale } from "@/app/[locale]/_shared/app-fieldnames";
import type { PageStatus } from "@/app/[locale]/_shared/app-enums";
import type { PaginatedUsers } from "@/server/Services/userService";
import {
  type UserDto,
  type InviteUserFormData,
  USER_ROLES,
  InviteUserSchema,
  roleLabel,
} from "./shared";
import { getUsersAction, inviteUserAction } from "./actions";

interface UsersViewProps {
  initialUsers: PaginatedUsers;
}

export default function UsersView({ initialUsers }: UsersViewProps) {
  const locale = useLocale() as Locale;
  const t = fieldnames[locale];

  const [users, setUsers] = useState<UserDto[]>(initialUsers.items);
  const [status, setStatus] = useState<PageStatus>("idle");
  const [errorMessage, setErrorMessage] = useState("");
  const [successMessage, setSuccessMessage] = useState("");
  const [isPending, startTransition] = useTransition();

  // Filters
  const [search, setSearch] = useState("");
  const [roleFilter, setRoleFilter] = useState("");
  const [statusFilter, setStatusFilter] = useState("");

  // Invite dialog
  const [showInviteDialog, setShowInviteDialog] = useState(false);

  const inviteForm = useForm<InviteUserFormData>({
    resolver: zodResolver(InviteUserSchema(locale)),
    defaultValues: {
      email: "",
      display_name: "",
      role: "Associate",
    },
  });

  const filteredUsers = useMemo(() => {
    return users.filter((user) => {
      const matchesSearch =
        !search ||
        user.display_name.toLowerCase().includes(search.toLowerCase()) ||
        user.email.toLowerCase().includes(search.toLowerCase());

      const matchesRole = !roleFilter || user.role === roleFilter;

      const matchesStatus =
        !statusFilter ||
        (statusFilter === "active" && user.is_active) ||
        (statusFilter === "inactive" && !user.is_active);

      return matchesSearch && matchesRole && matchesStatus;
    });
  }, [users, search, roleFilter, statusFilter]);

  const refreshUsers = useCallback(async () => {
    const result = await getUsersAction();
    if (result.success && result.data) {
      setUsers(result.data.items);
    }
  }, []);

  const handleInvite = useCallback(
    (data: InviteUserFormData) => {
      setStatus("loading");
      setErrorMessage("");
      setSuccessMessage("");

      startTransition(async () => {
        const result = await inviteUserAction({
          email: data.email,
          display_name: data.display_name,
          role: data.role,
        });

        if (result.success) {
          setStatus("success");
          setSuccessMessage(t.users_invite_success);
          setShowInviteDialog(false);
          inviteForm.reset({ email: "", display_name: "", role: "Associate" });
          await refreshUsers();
        } else {
          setStatus("failed");
          setErrorMessage(result.message || t.users_invite_error);
        }
      });
    },
    [t, startTransition, inviteForm, refreshUsers],
  );

  return (
    <div className="space-y-6 p-6">
      <PageHeader
        title={t.users_title}
        breadcrumbs={[
          { label: t.nav_dashboard, href: "/dashboard" },
          { label: t.nav_settings, href: "/settings" },
          { label: t.users_title },
        ]}
        actions={
          <Button
            size="sm"
            onClick={() => {
              setShowInviteDialog(true);
              setErrorMessage("");
              setSuccessMessage("");
              inviteForm.reset({
                email: "",
                display_name: "",
                role: "Associate",
              });
            }}
            disabled={isPending}
          >
            <Plus className="h-4 w-4" />
            <span className="hidden sm:inline">{t.users_invite}</span>
          </Button>
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

      {/* Filters */}
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder={t.users_search_placeholder}
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="pl-9"
          />
        </div>
        <Select
          value={roleFilter}
          onChange={(e) => setRoleFilter(e.target.value)}
          aria-label={t.users_filter_role}
          className="w-full sm:w-44"
        >
          <option value="">{t.users_all_roles}</option>
          {USER_ROLES.map((role) => (
            <option key={role} value={role}>
              {roleLabel(role, locale)}
            </option>
          ))}
        </Select>
        <Select
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value)}
          aria-label={t.users_filter_status}
          className="w-full sm:w-44"
        >
          <option value="">{t.users_all_statuses}</option>
          <option value="active">{t.users_active}</option>
          <option value="inactive">{t.users_inactive}</option>
        </Select>
      </div>

      {/* User List */}
      <div className="rounded-lg border border-border bg-card">
        {users.length === 0 ? (
          <div className="p-8 text-center">
            <p className="text-sm font-medium text-foreground">
              {t.users_empty_title}
            </p>
            <p className="mt-1 text-sm text-muted-foreground">
              {t.users_empty_message}
            </p>
          </div>
        ) : filteredUsers.length === 0 ? (
          <div className="p-8 text-center text-sm text-muted-foreground">
            {t.users_no_results}
          </div>
        ) : (
          <>
            {/* Desktop Table */}
            <div className="hidden md:block">
              <table className="w-full">
                <thead>
                  <tr className="border-b border-border text-left text-sm font-medium text-muted-foreground">
                    <th className="px-4 py-3">{t.users_name}</th>
                    <th className="px-4 py-3">{t.users_email}</th>
                    <th className="px-4 py-3">{t.users_role}</th>
                    <th className="px-4 py-3">{t.users_status}</th>
                    <th className="px-4 py-3">{t.users_last_login}</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border">
                  {filteredUsers.map((user) => (
                    <tr key={user.user_id} className="hover:bg-muted/50">
                      <td className="px-4 py-3">
                        <Link
                          href={`/settings/users/${user.user_id}`}
                          className="text-sm font-medium text-foreground hover:underline"
                        >
                          {user.display_name}
                        </Link>
                      </td>
                      <td className="px-4 py-3 text-sm text-muted-foreground">
                        {user.email}
                      </td>
                      <td className="px-4 py-3">
                        <Badge variant="secondary" className="text-xs">
                          {roleLabel(user.role, locale)}
                        </Badge>
                      </td>
                      <td className="px-4 py-3">
                        <Badge
                          variant={user.is_active ? "default" : "outline"}
                          className="text-xs"
                        >
                          {user.is_active ? t.users_active : t.users_inactive}
                        </Badge>
                      </td>
                      <td className="px-4 py-3 text-sm text-muted-foreground">
                        {t.users_never}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            {/* Mobile Cards */}
            <div className="divide-y divide-border md:hidden">
              {filteredUsers.map((user) => (
                <Link
                  key={user.user_id}
                  href={`/settings/users/${user.user_id}`}
                  className="flex items-center justify-between p-4 hover:bg-muted/50"
                >
                  <div className="min-w-0 flex-1">
                    <p className="truncate text-sm font-medium text-foreground">
                      {user.display_name}
                    </p>
                    <p className="truncate text-xs text-muted-foreground">
                      {user.email}
                    </p>
                  </div>
                  <div className="ml-3 flex flex-col items-end gap-1">
                    <Badge variant="secondary" className="text-xs">
                      {roleLabel(user.role, locale)}
                    </Badge>
                    <Badge
                      variant={user.is_active ? "default" : "outline"}
                      className="text-xs"
                    >
                      {user.is_active ? t.users_active : t.users_inactive}
                    </Badge>
                  </div>
                </Link>
              ))}
            </div>
          </>
        )}
      </div>

      {/* Invite User Dialog */}
      <Dialog open={showInviteDialog} onOpenChange={setShowInviteDialog}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{t.users_invite_title}</DialogTitle>
            <DialogDescription>
              {t.users_invite_description}
            </DialogDescription>
          </DialogHeader>

          <form
            onSubmit={inviteForm.handleSubmit(handleInvite)}
            className="space-y-4"
          >
            <div className="space-y-1">
              <label
                htmlFor="invite-display-name"
                className="text-sm font-medium text-foreground"
              >
                {t.users_name} *
              </label>
              <Input
                id="invite-display-name"
                {...inviteForm.register("display_name")}
                disabled={isPending}
                maxLength={200}
              />
              {inviteForm.formState.errors.display_name && (
                <p className="text-xs text-red-600">
                  {inviteForm.formState.errors.display_name.message}
                </p>
              )}
            </div>

            <div className="space-y-1">
              <label
                htmlFor="invite-email"
                className="text-sm font-medium text-foreground"
              >
                {t.users_email} *
              </label>
              <Input
                id="invite-email"
                type="email"
                {...inviteForm.register("email")}
                disabled={isPending}
                maxLength={200}
              />
              {inviteForm.formState.errors.email && (
                <p className="text-xs text-red-600">
                  {inviteForm.formState.errors.email.message}
                </p>
              )}
            </div>

            <div className="space-y-1">
              <label
                htmlFor="invite-role"
                className="text-sm font-medium text-foreground"
              >
                {t.users_role} *
              </label>
              <Select
                id="invite-role"
                {...inviteForm.register("role")}
                disabled={isPending}
              >
                {USER_ROLES.map((role) => (
                  <option key={role} value={role}>
                    {roleLabel(role, locale)}
                  </option>
                ))}
              </Select>
              {inviteForm.formState.errors.role && (
                <p className="text-xs text-red-600">
                  {inviteForm.formState.errors.role.message}
                </p>
              )}
            </div>

            <DialogFooter>
              <Button
                type="button"
                variant="outline"
                onClick={() => setShowInviteDialog(false)}
                disabled={isPending}
              >
                {t.users_cancel}
              </Button>
              <Button
                type="submit"
                disabled={isPending || status === "loading"}
              >
                {isPending || status === "loading"
                  ? t.users_invite_sending
                  : t.users_invite}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  );
}
