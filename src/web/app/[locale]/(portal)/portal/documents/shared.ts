import type {
  PortalDocumentDto,
  PaginatedPortalDocuments,
  GetPortalDocumentsParams,
} from "@/server/Services/portalDocumentService";
import type { DocumentCategoryDto } from "@/server/Services/documentCategoryService";

export type {
  PortalDocumentDto,
  PaginatedPortalDocuments,
  GetPortalDocumentsParams,
  DocumentCategoryDto,
};

export const CURRENT_YEAR = new Date().getFullYear();

export function getYearOptions(): number[] {
  const years: number[] = [];
  for (let y = CURRENT_YEAR; y >= CURRENT_YEAR - 5; y--) {
    years.push(y);
  }
  return years;
}

export function formatFileSize(bytes: number): string {
  if (bytes === 0) return "0 B";
  const units = ["B", "KB", "MB", "GB"];
  const i = Math.floor(Math.log(bytes) / Math.log(1024));
  const size = bytes / Math.pow(1024, i);
  return `${size.toFixed(i === 0 ? 0 : 1)} ${units[i]}`;
}

export function formatDate(isoDate: string, locale: string = "en-US"): string {
  const dateLocale = locale === "es-pr" ? "es-PR" : "en-US";
  const date = new Date(isoDate);
  return date.toLocaleDateString(dateLocale, {
    month: "short",
    day: "numeric",
    year: "numeric",
    timeZone: "America/Puerto_Rico",
  });
}

export function getMimeTypeIcon(mimeType: string): string {
  if (mimeType.includes("pdf")) return "pdf";
  if (mimeType.includes("image")) return "image";
  if (mimeType.includes("spreadsheet") || mimeType.includes("excel"))
    return "spreadsheet";
  if (
    mimeType.includes("document") ||
    mimeType.includes("word") ||
    mimeType.includes("msword")
  )
    return "document";
  return "file";
}
