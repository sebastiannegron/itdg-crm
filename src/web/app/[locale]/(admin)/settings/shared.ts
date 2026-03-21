import { z } from "zod";
import { codeRegex, urlRegex } from "@/app/[locale]/_shared/app-enums";
import { fieldnames, type Locale } from "@/app/[locale]/_shared/app-fieldnames";
import type { ClientTierDto } from "@/server/Services/tierService";
import type { DocumentCategoryDto } from "@/server/Services/documentCategoryService";
import type { GoogleConnectionStatusDto } from "@/server/Services/integrationService";
import type { GmailConnectionStatusDto } from "@/server/Services/integrationService";

export type { ClientTierDto, DocumentCategoryDto, GoogleConnectionStatusDto, GmailConnectionStatusDto };

function safeText(locale: Locale) {
  const t = fieldnames[locale];
  return {
    required: t.required_error,
    invalidInput: t.settings_invalid_input,
  };
}

export function TierSchema(locale: Locale) {
  const msg = safeText(locale);

  return z.object({
    name: z
      .string()
      .min(1, msg.required)
      .max(100)
      .refine(
        (val) => !codeRegex.test(val) && !urlRegex.test(val),
        msg.invalidInput,
      ),
    sort_order: z.number().int().min(0),
  });
}

export type TierFormData = z.infer<ReturnType<typeof TierSchema>>;

export function DocumentCategorySchema(locale: Locale) {
  const msg = safeText(locale);

  return z.object({
    name: z
      .string()
      .min(1, msg.required)
      .max(100)
      .refine(
        (val) => !codeRegex.test(val) && !urlRegex.test(val),
        msg.invalidInput,
      ),
    naming_convention: z
      .string()
      .max(200)
      .refine(
        (val) => !codeRegex.test(val) && !urlRegex.test(val),
        msg.invalidInput,
      )
      .optional()
      .or(z.literal("")),
    sort_order: z.number().int().min(0),
  });
}

export type DocumentCategoryFormData = z.infer<
  ReturnType<typeof DocumentCategorySchema>
>;
