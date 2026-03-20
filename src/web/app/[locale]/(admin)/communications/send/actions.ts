"use server";

import { Span, SpanStatusCode, trace } from "@opentelemetry/api";
import {
  getTemplates,
  renderTemplate,
  type CommunicationTemplateDto,
  type RenderedTemplateDto,
} from "@/server/Services/templateService";
import {
  sendTemplateMessage,
  type SendTemplateMessageParams,
} from "@/server/Services/messageService";
import { getClients } from "@/server/Services/clientService";

const tracer = trace.getTracer("web");

export interface ActionResult<T = undefined> {
  success: boolean;
  message: string;
  data?: T;
}

export async function fetchActiveTemplates(): Promise<
  ActionResult<CommunicationTemplateDto[]>
> {
  return tracer.startActiveSpan(
    "Fetch Active Templates",
    async (span: Span) => {
      try {
        const templates = await getTemplates();
        const active = templates.filter((t) => t.is_active);
        span.setStatus({ code: SpanStatusCode.OK });
        return {
          success: true,
          message: "Templates fetched successfully",
          data: active,
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

export async function fetchClients(): Promise<
  ActionResult<{ client_id: string; name: string; contact_email: string | null }[]>
> {
  return tracer.startActiveSpan("Fetch Clients For Send", async (span: Span) => {
    try {
      const result = await getClients();
      const clients = result.items.map((c) => ({
        client_id: c.client_id,
        name: c.name,
        contact_email: c.contact_email,
      }));
      span.setStatus({ code: SpanStatusCode.OK });
      return {
        success: true,
        message: "Clients fetched successfully",
        data: clients,
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

export async function previewTemplate(
  templateId: string,
  mergeFields: Record<string, string>,
): Promise<ActionResult<RenderedTemplateDto>> {
  return tracer.startActiveSpan(
    "Preview Template Message",
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

export async function sendMessage(
  params: SendTemplateMessageParams,
): Promise<ActionResult> {
  return tracer.startActiveSpan(
    "Send Template Message",
    async (span: Span) => {
      try {
        await sendTemplateMessage(params);
        span.setStatus({ code: SpanStatusCode.OK });
        return {
          success: true,
          message: "Message sent successfully",
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
