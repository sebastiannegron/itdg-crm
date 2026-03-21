"use server";

import { Span, SpanStatusCode, trace } from "@opentelemetry/api";
import {
  getTiers as getTiersService,
  createTier as createTierService,
  updateTier as updateTierService,
  type ClientTierDto,
  type CreateTierParams,
  type UpdateTierParams,
} from "@/server/Services/tierService";
import {
  getDocumentCategories as getDocumentCategoriesService,
  createDocumentCategory as createDocumentCategoryService,
  updateDocumentCategory as updateDocumentCategoryService,
  deleteDocumentCategory as deleteDocumentCategoryService,
  reorderDocumentCategories as reorderDocumentCategoriesService,
  type DocumentCategoryDto,
  type CreateDocumentCategoryParams,
  type UpdateDocumentCategoryParams,
  type ReorderDocumentCategoriesParams,
} from "@/server/Services/documentCategoryService";
import {
  getGoogleConnectionStatus as getGoogleConnectionStatusService,
  disconnectGoogle as disconnectGoogleService,
  type GoogleConnectionStatusDto,
} from "@/server/Services/integrationService";

const tracer = trace.getTracer("web");

export interface ActionResult<T = undefined> {
  success: boolean;
  message: string;
  data?: T;
}

export async function getTiersAction(): Promise<
  ActionResult<ClientTierDto[]>
> {
  return tracer.startActiveSpan("Get Tiers", async (span: Span) => {
    try {
      const tiers = await getTiersService();
      span.setStatus({ code: SpanStatusCode.OK });
      return {
        success: true,
        message: "Tiers fetched successfully",
        data: tiers,
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
  });
}

export async function createTierAction(
  params: CreateTierParams,
): Promise<ActionResult> {
  return tracer.startActiveSpan("Create Tier", async (span: Span) => {
    try {
      await createTierService(params);
      span.setStatus({ code: SpanStatusCode.OK });
      return {
        success: true,
        message: "Tier created successfully",
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
  });
}

export async function updateTierAction(
  tierId: string,
  params: UpdateTierParams,
): Promise<ActionResult> {
  return tracer.startActiveSpan("Update Tier", async (span: Span) => {
    try {
      await updateTierService(tierId, params);
      span.setStatus({ code: SpanStatusCode.OK });
      return {
        success: true,
        message: "Tier updated successfully",
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
  });
}

export async function getDocumentCategoriesAction(): Promise<
  ActionResult<DocumentCategoryDto[]>
> {
  return tracer.startActiveSpan(
    "Get Document Categories",
    async (span: Span) => {
      try {
        const categories = await getDocumentCategoriesService();
        span.setStatus({ code: SpanStatusCode.OK });
        return {
          success: true,
          message: "Document categories fetched successfully",
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

export async function createDocumentCategoryAction(
  params: CreateDocumentCategoryParams,
): Promise<ActionResult> {
  return tracer.startActiveSpan(
    "Create Document Category",
    async (span: Span) => {
      try {
        await createDocumentCategoryService(params);
        span.setStatus({ code: SpanStatusCode.OK });
        return {
          success: true,
          message: "Category created successfully",
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

export async function updateDocumentCategoryAction(
  categoryId: string,
  params: UpdateDocumentCategoryParams,
): Promise<ActionResult> {
  return tracer.startActiveSpan(
    "Update Document Category",
    async (span: Span) => {
      try {
        await updateDocumentCategoryService(categoryId, params);
        span.setStatus({ code: SpanStatusCode.OK });
        return {
          success: true,
          message: "Category updated successfully",
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

export async function deleteDocumentCategoryAction(
  categoryId: string,
): Promise<ActionResult> {
  return tracer.startActiveSpan(
    "Delete Document Category",
    async (span: Span) => {
      try {
        await deleteDocumentCategoryService(categoryId);
        span.setStatus({ code: SpanStatusCode.OK });
        return {
          success: true,
          message: "Category deleted successfully",
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

export async function reorderDocumentCategoriesAction(
  params: ReorderDocumentCategoriesParams,
): Promise<ActionResult> {
  return tracer.startActiveSpan(
    "Reorder Document Categories",
    async (span: Span) => {
      try {
        await reorderDocumentCategoriesService(params);
        span.setStatus({ code: SpanStatusCode.OK });
        return {
          success: true,
          message: "Categories reordered successfully",
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

export async function getGoogleConnectionStatusAction(): Promise<
  ActionResult<GoogleConnectionStatusDto>
> {
  return tracer.startActiveSpan(
    "Get Google Connection Status",
    async (span: Span) => {
      try {
        const status = await getGoogleConnectionStatusService();
        span.setStatus({ code: SpanStatusCode.OK });
        return {
          success: true,
          message: "Google connection status fetched successfully",
          data: status,
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

export async function disconnectGoogleAction(): Promise<ActionResult> {
  return tracer.startActiveSpan(
    "Disconnect Google",
    async (span: Span) => {
      try {
        await disconnectGoogleService();
        span.setStatus({ code: SpanStatusCode.OK });
        return {
          success: true,
          message: "Google Drive disconnected successfully",
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
