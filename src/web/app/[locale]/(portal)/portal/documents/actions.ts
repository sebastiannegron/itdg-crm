"use server";

import { Span, SpanStatusCode, trace } from "@opentelemetry/api";
import {
  getPortalDocuments,
  uploadPortalDocument as uploadDocument,
  type GetPortalDocumentsParams,
  type PaginatedPortalDocuments,
} from "@/server/Services/portalDocumentService";
import {
  getDocumentDownload,
  type DocumentDownloadDto,
} from "@/server/Services/documentService";
import {
  getDocumentCategories,
  type DocumentCategoryDto,
} from "@/server/Services/documentCategoryService";

const tracer = trace.getTracer("web");

export interface ActionResult<T = undefined> {
  success: boolean;
  message: string;
  data?: T;
}

export async function fetchPortalDocuments(
  params: GetPortalDocumentsParams = {},
): Promise<ActionResult<PaginatedPortalDocuments>> {
  return tracer.startActiveSpan(
    "Fetch Portal Documents",
    async (span: Span) => {
      try {
        const documents = await getPortalDocuments(params);
        span.setStatus({ code: SpanStatusCode.OK });
        return {
          success: true,
          message: "Documents fetched successfully",
          data: documents,
        };
      } catch (err: unknown) {
        const error = err instanceof Error ? err : new Error(String(err));
        span.recordException(error);
        span.setStatus({
          code: SpanStatusCode.ERROR,
          message: error.message,
        });
        return {
          success: false,
          message: error.message,
        };
      } finally {
        span.end();
      }
    },
  );
}

export async function fetchPortalDocumentDownload(
  documentId: string,
): Promise<ActionResult<DocumentDownloadDto>> {
  return tracer.startActiveSpan(
    "Fetch Portal Document Download",
    async (span: Span) => {
      try {
        const download = await getDocumentDownload(documentId);
        span.setStatus({ code: SpanStatusCode.OK });
        return {
          success: true,
          message: "Document download fetched successfully",
          data: download,
        };
      } catch (err: unknown) {
        const error = err instanceof Error ? err : new Error(String(err));
        span.recordException(error);
        span.setStatus({
          code: SpanStatusCode.ERROR,
          message: error.message,
        });
        return {
          success: false,
          message: error.message,
        };
      } finally {
        span.end();
      }
    },
  );
}

export async function uploadPortalDocumentAction(
  formData: FormData,
): Promise<ActionResult> {
  return tracer.startActiveSpan(
    "Upload Portal Document",
    async (span: Span) => {
      try {
        const file = formData.get("file") as File | null;
        const categoryId = formData.get("category_id") as string | null;

        if (!file || file.size === 0) {
          return { success: false, message: "File is required" };
        }
        if (!categoryId || categoryId.trim().length === 0) {
          return { success: false, message: "Category is required" };
        }

        await uploadDocument(file, categoryId.trim());
        span.setStatus({ code: SpanStatusCode.OK });
        return {
          success: true,
          message: "Document uploaded successfully",
        };
      } catch (err: unknown) {
        const error = err instanceof Error ? err : new Error(String(err));
        span.recordException(error);
        span.setStatus({
          code: SpanStatusCode.ERROR,
          message: error.message,
        });
        return {
          success: false,
          message: error.message,
        };
      } finally {
        span.end();
      }
    },
  );
}

export async function fetchDocumentCategoriesAction(): Promise<
  ActionResult<DocumentCategoryDto[]>
> {
  return tracer.startActiveSpan(
    "Fetch Document Categories",
    async (span: Span) => {
      try {
        const categories = await getDocumentCategories();
        span.setStatus({ code: SpanStatusCode.OK });
        return {
          success: true,
          message: "Categories fetched successfully",
          data: categories,
        };
      } catch (err: unknown) {
        const error = err instanceof Error ? err : new Error(String(err));
        span.recordException(error);
        span.setStatus({
          code: SpanStatusCode.ERROR,
          message: error.message,
        });
        return {
          success: false,
          message: error.message,
        };
      } finally {
        span.end();
      }
    },
  );
}
