"use server";

import { Span, SpanStatusCode, trace } from "@opentelemetry/api";
import {
  getClientDocuments,
  type GetClientDocumentsParams,
  type PaginatedDocuments,
  getDocumentDownload,
  type DocumentDownloadDto,
  searchDocuments,
  type SearchDocumentsParams,
  type PaginatedSearchResults,
} from "@/server/Services/documentService";
import {
  getClients,
  type ClientDto,
  type PaginatedResult,
} from "@/server/Services/clientService";
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

export async function fetchClientDocuments(
  params: GetClientDocumentsParams,
): Promise<ActionResult<PaginatedDocuments>> {
  return tracer.startActiveSpan(
    "Fetch Client Documents",
    async (span: Span) => {
      try {
        const documents = await getClientDocuments(params);
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

export async function fetchDocumentDownload(
  documentId: string,
): Promise<ActionResult<DocumentDownloadDto>> {
  return tracer.startActiveSpan(
    "Fetch Document Download",
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

export async function fetchClientsForFilter(): Promise<
  ActionResult<ClientDto[]>
> {
  return tracer.startActiveSpan(
    "Fetch Clients For Filter",
    async (span: Span) => {
      try {
        const result: PaginatedResult<ClientDto> = await getClients({
          page: 1,
          pageSize: 100,
        });
        span.setStatus({ code: SpanStatusCode.OK });
        return {
          success: true,
          message: "Clients fetched successfully",
          data: result.items,
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

export async function fetchDocumentCategories(): Promise<
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

export async function fetchSearchDocuments(
  params: SearchDocumentsParams,
): Promise<ActionResult<PaginatedSearchResults>> {
  return tracer.startActiveSpan(
    "Search Documents",
    async (span: Span) => {
      try {
        const results = await searchDocuments(params);
        span.setStatus({ code: SpanStatusCode.OK });
        return {
          success: true,
          message: "Search completed successfully",
          data: results,
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
