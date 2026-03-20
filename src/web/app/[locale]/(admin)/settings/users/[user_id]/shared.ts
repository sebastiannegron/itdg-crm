import { z } from "zod";
import { fieldnames, type Locale } from "@/app/[locale]/_shared/app-fieldnames";
import type { UserDto } from "@/server/Services/userService";

export type { UserDto };

export const USER_ROLES = ["Administrator", "Associate", "ClientPortal"] as const;
export type UserRole = (typeof USER_ROLES)[number];

export function roleLabel(role: string, locale: Locale): string {
  const t = fieldnames[locale];
  switch (role) {
    case "Administrator":
      return t.users_role_administrator;
    case "Associate":
      return t.users_role_associate;
    case "ClientPortal":
      return t.users_role_client_portal;
    default:
      return role;
  }
}

function safeText(locale: Locale) {
  const t = fieldnames[locale];
  return {
    required: t.required_error,
  };
}

export function UpdateUserSchema(locale: Locale) {
  const msg = safeText(locale);

  return z.object({
    role: z.enum(USER_ROLES, { message: msg.required }),
    is_active: z.boolean(),
  });
}

export type UpdateUserFormData = z.infer<ReturnType<typeof UpdateUserSchema>>;
