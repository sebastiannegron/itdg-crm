"use server";

import { Span, SpanStatusCode, trace } from "@opentelemetry/api";
import {
  getPortalMessages,
  sendPortalMessage as sendMessage,
  markMessageAsRead as markRead,
  type MessageDto,
} from "@/server/Services/portalMessageService";

const tracer = trace.getTracer("web");

export interface ActionResult<T = undefined> {
  success: boolean;
  message: string;
  data?: T;
}

export async function fetchPortalMessages(): Promise<
  ActionResult<MessageDto[]>
> {
  return tracer.startActiveSpan(
    "Fetch Portal Messages",
    async (span: Span) => {
      try {
        const messages = await getPortalMessages();
        span.setStatus({ code: SpanStatusCode.OK });
        return {
          success: true,
          message: "Messages fetched successfully",
          data: messages,
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

export async function sendPortalMessage(
  formData: FormData,
): Promise<ActionResult> {
  return tracer.startActiveSpan(
    "Send Portal Message",
    async (span: Span) => {
      try {
        const subject = formData.get("subject") as string;
        const body = formData.get("body") as string;

        if (!subject || subject.trim().length === 0) {
          return { success: false, message: "Subject is required" };
        }
        if (!body || body.trim().length === 0) {
          return { success: false, message: "Body is required" };
        }
        if (subject.length > 500) {
          return {
            success: false,
            message: "Subject must not exceed 500 characters",
          };
        }
        if (body.length > 4000) {
          return {
            success: false,
            message: "Body must not exceed 4000 characters",
          };
        }

        await sendMessage(subject.trim(), body.trim());
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

export async function markPortalMessageAsRead(
  messageId: string,
): Promise<ActionResult> {
  return tracer.startActiveSpan(
    "Mark Portal Message As Read",
    async (span: Span) => {
      try {
        await markRead(messageId);
        span.setStatus({ code: SpanStatusCode.OK });
        return {
          success: true,
          message: "Message marked as read",
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
