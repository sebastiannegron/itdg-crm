import { z } from "zod";
import type {
  ClientDto,
  CreateClientParams,
  UpdateClientParams,
} from "@/server/Services/clientService";
import { codeRegex, urlRegex } from "@/app/[locale]/_shared/app-enums";
import { fieldnames, type Locale } from "@/app/[locale]/_shared/app-fieldnames";

export type { ClientDto, CreateClientParams, UpdateClientParams };

export const CLIENT_STATUSES = ["Active", "Inactive", "Suspended"] as const;
export type ClientStatus = (typeof CLIENT_STATUSES)[number];

export const DETAIL_TABS = [
  "overview",
  "documents",
  "communications",
  "tasks",
] as const;
export type DetailTab = (typeof DETAIL_TABS)[number];

function safeText(locale: Locale) {
  const t = fieldnames[locale];
  return {
    required: t.required_error,
    invalidInput: t.clients_invalid_input,
    invalidEmail: t.email_invalid_error,
    invalidPhone: t.clients_phone_invalid,
  };
}

export function UpdateClientSchema(locale: Locale) {
  const msg = safeText(locale);

  return z.object({
    name: z
      .string()
      .min(1, msg.required)
      .max(200)
      .refine(
        (val) => !codeRegex.test(val) && !urlRegex.test(val),
        msg.invalidInput,
      ),
    contact_email: z
      .string()
      .max(200)
      .email(msg.invalidEmail)
      .optional()
      .or(z.literal("")),
    phone: z
      .string()
      .max(50)
      .refine(
        (val) => !val || !codeRegex.test(val),
        msg.invalidPhone,
      )
      .optional()
      .or(z.literal("")),
    address: z
      .string()
      .max(500)
      .refine(
        (val) => !val || (!codeRegex.test(val) && !urlRegex.test(val)),
        msg.invalidInput,
      )
      .optional()
      .or(z.literal("")),
    status: z.enum(CLIENT_STATUSES),
    industry_tag: z
      .string()
      .max(100)
      .refine(
        (val) => !val || (!codeRegex.test(val) && !urlRegex.test(val)),
        msg.invalidInput,
      )
      .optional()
      .or(z.literal("")),
    notes: z
      .string()
      .max(2000)
      .refine(
        (val) => !val || !codeRegex.test(val),
        msg.invalidInput,
      )
      .optional()
      .or(z.literal("")),
  });
}

export type UpdateClientFormData = z.infer<ReturnType<typeof UpdateClientSchema>>;
