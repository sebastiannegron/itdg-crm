import type { CommunicationTemplateDto, RenderedTemplateDto } from "@/server/Services/templateService";

export type { CommunicationTemplateDto, RenderedTemplateDto };

export const TEMPLATE_CATEGORIES = [
  { value: 0, label: "Onboarding" },
  { value: 1, label: "Document Request" },
  { value: 2, label: "Payment Reminder" },
  { value: 3, label: "Tax Season" },
  { value: 4, label: "General" },
] as const;

export type TemplateCategory = (typeof TEMPLATE_CATEGORIES)[number]["value"];

export const TEMPLATE_LANGUAGES = ["en", "es"] as const;
export type TemplateLanguage = (typeof TEMPLATE_LANGUAGES)[number];

export const MERGE_FIELDS = [
  { key: "client_name", label: "Client Name" },
  { key: "client_email", label: "Client Email" },
  { key: "client_phone", label: "Client Phone" },
  { key: "advisor_name", label: "Advisor Name" },
  { key: "advisor_email", label: "Advisor Email" },
  { key: "company_name", label: "Company Name" },
  { key: "due_date", label: "Due Date" },
  { key: "amount", label: "Amount" },
  { key: "document_name", label: "Document Name" },
] as const;

export const SAMPLE_MERGE_DATA: Record<string, string> = {
  client_name: "John Doe",
  client_email: "john.doe@example.com",
  client_phone: "787-555-0100",
  advisor_name: "María García",
  advisor_email: "maria@randasoc.com",
  company_name: "R&A Tax Consultants",
  due_date: "April 15, 2026",
  amount: "$1,250.00",
  document_name: "W-2 Form",
};

export interface TemplateFilters {
  search: string;
  category: string;
  status: string;
}
