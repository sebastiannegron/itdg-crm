import { z } from "zod";
import { codeRegex, urlRegex } from "@/app/[locale]/_shared/app-enums";
import { fieldnames, type Locale } from "@/app/[locale]/_shared/app-fieldnames";
import type { UserDto } from "@/server/Services/userService";

export type { UserDto };

export const USER_ROLES = ["Administrator", "Associate", "ClientPortal"] as const;
export type UserRole = (typeof USER_ROLES)[number];

export const USER_STATUSES = ["active", "inactive"] as const;
export type UserStatus = (typeof USER_STATUSES)[number];

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
    invalidInput: t.users_invalid_input,
    invalidEmail: t.email_invalid_error,
  };
}

export function InviteUserSchema(locale: Locale) {
  const msg = safeText(locale);

  return z.object({
    email: z
      .string()
      .min(1, msg.required)
      .max(200)
      .email(msg.invalidEmail),
    display_name: z
      .string()
      .min(2, msg.required)
      .max(200)
      .refine(
        (val) => !codeRegex.test(val) && !urlRegex.test(val),
        msg.invalidInput,
      ),
    role: z.enum(USER_ROLES),
  });
}

export type InviteUserFormData = z.infer<ReturnType<typeof InviteUserSchema>>;
