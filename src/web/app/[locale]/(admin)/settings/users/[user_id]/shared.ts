import { z } from "zod";
import { fieldnames, type Locale } from "@/app/[locale]/_shared/app-fieldnames";
import type { UserDto } from "@/server/Services/userService";
import { USER_ROLES, roleLabel } from "../shared";

export type { UserDto };
export { USER_ROLES, roleLabel };
export type { Locale };

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
