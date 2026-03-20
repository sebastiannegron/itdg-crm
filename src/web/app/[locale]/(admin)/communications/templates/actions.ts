"use server";

import { Span, SpanStatusCode, trace } from "@opentelemetry/api";
import {
  getTemplates,
  renderTemplate,
  createTemplate,
  updateTemplate,
  retireTemplate,
  type CommunicationTemplateDto,
  type RenderedTemplateDto,
  type CreateTemplateParams,
  type UpdateTemplateParams,
} from "@/server/Services/templateService";

const tracer = trace.getTracer("web");

export interface ActionResult<T = undefined> {
  success: boolean;
  message: string;
  data?: T;
}

export async function fetchTemplates(): Promise<
  ActionResult<CommunicationTemplateDto[]>
> {
  return tracer.startActiveSpan("Fetch Templates", async (span: Span) => {
    try {
      const templates = await getTemplates();
      span.setStatus({ code: SpanStatusCode.OK });
      return {
        success: true,
        message: "Templates fetched successfully",
        data: templates,
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

export async function renderTemplatePreview(
  templateId: string,
  mergeFields: Record<string, string>,
): Promise<ActionResult<RenderedTemplateDto>> {
  return tracer.startActiveSpan(
    "Render Template Preview",
    async (span: Span) => {
      try {
        const rendered = await renderTemplate(templateId, mergeFields);
        span.setStatus({ code: SpanStatusCode.OK });
        return {
          success: true,
          message: "Template rendered successfully",
          data: rendered,
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

export async function createNewTemplate(
  params: CreateTemplateParams,
): Promise<ActionResult> {
  return tracer.startActiveSpan("Create Template", async (span: Span) => {
    try {
      await createTemplate(params);
      span.setStatus({ code: SpanStatusCode.OK });
      return {
        success: true,
        message: "Template created successfully",
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

export async function updateExistingTemplate(
  id: string,
  params: UpdateTemplateParams,
): Promise<ActionResult> {
  return tracer.startActiveSpan("Update Template", async (span: Span) => {
    try {
      await updateTemplate(id, params);
      span.setStatus({ code: SpanStatusCode.OK });
      return {
        success: true,
        message: "Template updated successfully",
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

export async function retireExistingTemplate(
  id: string,
): Promise<ActionResult> {
  return tracer.startActiveSpan("Retire Template", async (span: Span) => {
    try {
      await retireTemplate(id);
      span.setStatus({ code: SpanStatusCode.OK });
      return {
        success: true,
        message: "Template retired successfully",
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
