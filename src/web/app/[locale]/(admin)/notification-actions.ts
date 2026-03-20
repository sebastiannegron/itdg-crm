"use server";

import { Span, SpanStatusCode, trace } from "@opentelemetry/api";
import {
  getNotifications,
  getUnreadNotificationCount,
  markNotificationAsRead,
  markAllNotificationsAsRead,
  type PaginatedNotifications,
} from "@/server/Services/notificationService";

const tracer = trace.getTracer("web");

export interface ActionResult<T = undefined> {
  success: boolean;
  message: string;
  data?: T;
}

export async function getNotificationsAction(
  page = 1,
  pageSize = 10,
  status?: string,
): Promise<ActionResult<PaginatedNotifications>> {
  return tracer.startActiveSpan(
    "Get Notifications",
    async (span: Span) => {
      try {
        const result = await getNotifications(page, pageSize, status);
        span.setStatus({ code: SpanStatusCode.OK });
        return {
          success: true,
          message: "Notifications fetched successfully",
          data: result,
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

export async function getUnreadCountAction(): Promise<ActionResult<number>> {
  return tracer.startActiveSpan(
    "Get Unread Notification Count",
    async (span: Span) => {
      try {
        const count = await getUnreadNotificationCount();
        span.setStatus({ code: SpanStatusCode.OK });
        return {
          success: true,
          message: "Unread count fetched successfully",
          data: count,
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

export async function markNotificationAsReadAction(
  notificationId: string,
): Promise<ActionResult> {
  return tracer.startActiveSpan(
    "Mark Notification As Read",
    async (span: Span) => {
      try {
        await markNotificationAsRead(notificationId);
        span.setStatus({ code: SpanStatusCode.OK });
        return {
          success: true,
          message: "Notification marked as read",
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

export async function markAllNotificationsAsReadAction(): Promise<ActionResult> {
  return tracer.startActiveSpan(
    "Mark All Notifications As Read",
    async (span: Span) => {
      try {
        await markAllNotificationsAsRead();
        span.setStatus({ code: SpanStatusCode.OK });
        return {
          success: true,
          message: "All notifications marked as read",
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
