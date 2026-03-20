"use server";

import { Span, SpanStatusCode, trace } from "@opentelemetry/api";
import {
  getNotificationPreferences as getPreferencesService,
  updateNotificationPreferences as updatePreferencesService,
  type NotificationPreferenceDto,
} from "@/server/Services/notificationPreferenceService";

const tracer = trace.getTracer("web");

export interface ActionResult<T = undefined> {
  success: boolean;
  message: string;
  data?: T;
}

export async function getNotificationPreferencesAction(): Promise<
  ActionResult<NotificationPreferenceDto[]>
> {
  return tracer.startActiveSpan(
    "Get Notification Preferences",
    async (span: Span) => {
      try {
        const preferences = await getPreferencesService();
        span.setStatus({ code: SpanStatusCode.OK });
        return {
          success: true,
          message: "Preferences fetched successfully",
          data: preferences,
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

export async function updateNotificationPreferencesAction(
  preferences: {
    event_type: string;
    channel: string;
    is_enabled: boolean;
    digest_mode: string;
  }[],
): Promise<ActionResult> {
  return tracer.startActiveSpan(
    "Update Notification Preferences",
    async (span: Span) => {
      try {
        await updatePreferencesService({ preferences });
        span.setStatus({ code: SpanStatusCode.OK });
        return {
          success: true,
          message: "Preferences updated successfully",
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
