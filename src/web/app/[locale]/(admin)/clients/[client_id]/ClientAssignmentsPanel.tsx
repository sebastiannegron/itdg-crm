"use client";

import { useState, useTransition } from "react";
import { useLocale } from "next-intl";
import { Button } from "@/app/_components/ui/button";
import { Select } from "@/app/_components/ui/select";
import { Users, X } from "lucide-react";
import { fieldnames, type Locale } from "@/app/[locale]/_shared/app-fieldnames";
import type { PageStatus } from "@/app/[locale]/_shared/app-enums";
import type { ClientAssignmentDto } from "./shared";
import { assignClientAction, unassignClientAction } from "./actions";

export interface AssociateOption {
  user_id: string;
  display_name: string;
  email: string;
}

interface ClientAssignmentsPanelProps {
  clientId: string;
  initialAssignments: ClientAssignmentDto[];
  users: AssociateOption[];
}

export default function ClientAssignmentsPanel({
  clientId,
  initialAssignments,
  users,
}: ClientAssignmentsPanelProps) {
  const locale = useLocale() as Locale;
  const t = fieldnames[locale];

  const [assignments, setAssignments] =
    useState<ClientAssignmentDto[]>(initialAssignments);
  const [selectedUserId, setSelectedUserId] = useState("");
  const [status, setStatus] = useState<PageStatus>("idle");
  const [message, setMessage] = useState("");
  const [isPending, startTransition] = useTransition();

  const assignedUserIds = new Set(assignments.map((a) => a.user_id));
  const availableUsers = users.filter((u) => !assignedUserIds.has(u.user_id));

  function handleAssign() {
    if (!selectedUserId) return;

    setStatus("loading");
    setMessage("");

    startTransition(async () => {
      const result = await assignClientAction(clientId, selectedUserId);

      if (result.success) {
        const assignedUser = users.find((u) => u.user_id === selectedUserId);
        if (assignedUser) {
          setAssignments((prev) => [
            ...prev,
            {
              user_id: assignedUser.user_id,
              display_name: assignedUser.display_name,
              email: assignedUser.email,
              assigned_at: new Date().toISOString(),
            },
          ]);
        }
        setSelectedUserId("");
        setStatus("success");
        setMessage(t.clients_assignments_assign_success);
      } else {
        setStatus("failed");
        setMessage(result.message || t.clients_assignments_assign_error);
      }
    });
  }

  function handleRemove(userId: string) {
    setStatus("loading");
    setMessage("");

    startTransition(async () => {
      const result = await unassignClientAction(clientId, userId);

      if (result.success) {
        setAssignments((prev) => prev.filter((a) => a.user_id !== userId));
        setStatus("success");
        setMessage(t.clients_assignments_remove_success);
      } else {
        setStatus("failed");
        setMessage(result.message || t.clients_assignments_remove_error);
      }
    });
  }

  return (
    <div className="rounded-lg border border-border bg-card p-4 space-y-4">
      <div className="flex items-center gap-2">
        <Users className="h-4 w-4 text-muted-foreground" />
        <h3 className="text-sm font-semibold text-foreground">
          {t.clients_assignments_title}
        </h3>
      </div>

      {status === "success" && message && (
        <div className="rounded-md border border-green-200 bg-green-50 p-2 text-xs text-green-800">
          {message}
        </div>
      )}

      {status === "failed" && message && (
        <div className="rounded-md border border-red-200 bg-red-50 p-2 text-xs text-red-800">
          {message}
        </div>
      )}

      {/* Assign form */}
      {availableUsers.length > 0 && (
        <div className="flex items-end gap-2">
          <div className="flex-1">
            <Select
              value={selectedUserId}
              onChange={(e) => setSelectedUserId(e.target.value)}
              disabled={isPending}
              aria-label={t.clients_assignments_select}
            >
              <option value="">{t.clients_assignments_select}</option>
              {availableUsers.map((user) => (
                <option key={user.user_id} value={user.user_id}>
                  {user.display_name} ({user.email})
                </option>
              ))}
            </Select>
          </div>
          <Button
            type="button"
            size="sm"
            onClick={handleAssign}
            disabled={!selectedUserId || isPending}
          >
            {isPending && status === "loading"
              ? t.clients_assignments_assigning
              : t.clients_assignments_assign}
          </Button>
        </div>
      )}

      {/* Assigned list */}
      {assignments.length === 0 ? (
        <p className="text-xs text-muted-foreground">
          {t.clients_assignments_empty}
        </p>
      ) : (
        <ul className="divide-y divide-border" role="list">
          {assignments.map((assignment) => (
            <li
              key={assignment.user_id}
              className="flex items-center justify-between py-2"
            >
              <div>
                <p className="text-sm font-medium text-foreground">
                  {assignment.display_name}
                </p>
                <p className="text-xs text-muted-foreground">
                  {assignment.email}
                </p>
              </div>
              <Button
                type="button"
                variant="ghost"
                size="sm"
                onClick={() => handleRemove(assignment.user_id)}
                disabled={isPending}
                aria-label={`${t.clients_assignments_remove} ${assignment.display_name}`}
              >
                <X className="h-4 w-4" />
              </Button>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
