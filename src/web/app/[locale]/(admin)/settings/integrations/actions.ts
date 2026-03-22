"use server";

import { Span, SpanStatusCode, trace } from "@opentelemetry/api";
import {
  getGoogleConnectionStatus as getGoogleConnectionStatusService,
  disconnectGoogle as disconnectGoogleService,
  type GoogleConnectionStatusDto,
  getGmailConnectionStatus as getGmailConnectionStatusService,
  disconnectGmail as disconnectGmailService,
  type GmailConnectionStatusDto,
  getCalendarConnectionStatus as getCalendarConnectionStatusService,
  type CalendarConnectionStatusDto,
  getMsGraphConnectionStatus as getMsGraphConnectionStatusService,
  type MsGraphConnectionStatusDto,
  getAzureOpenAiConnectionStatus as getAzureOpenAiConnectionStatusService,
  type AzureOpenAiConnectionStatusDto,
} from "@/server/Services/integrationService";

const tracer = trace.getTracer("web");

export interface ActionResult<T = undefined> {
  success: boolean;
  message: string;
  data?: T;
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

export async function getGmailConnectionStatusAction(): Promise<
  ActionResult<GmailConnectionStatusDto>
> {
  return tracer.startActiveSpan(
    "Get Gmail Connection Status",
    async (span: Span) => {
      try {
        const status = await getGmailConnectionStatusService();
        span.setStatus({ code: SpanStatusCode.OK });
        return {
          success: true,
          message: "Gmail connection status fetched successfully",
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

export async function disconnectGmailAction(): Promise<ActionResult> {
  return tracer.startActiveSpan(
    "Disconnect Gmail",
    async (span: Span) => {
      try {
        await disconnectGmailService();
        span.setStatus({ code: SpanStatusCode.OK });
        return {
          success: true,
          message: "Gmail disconnected successfully",
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

export async function getCalendarConnectionStatusAction(): Promise<
  ActionResult<CalendarConnectionStatusDto>
> {
  return tracer.startActiveSpan(
    "Get Calendar Connection Status",
    async (span: Span) => {
      try {
        const status = await getCalendarConnectionStatusService();
        span.setStatus({ code: SpanStatusCode.OK });
        return {
          success: true,
          message: "Calendar connection status fetched successfully",
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

export async function getMsGraphConnectionStatusAction(): Promise<
  ActionResult<MsGraphConnectionStatusDto>
> {
  return tracer.startActiveSpan(
    "Get Microsoft Graph Connection Status",
    async (span: Span) => {
      try {
        const status = await getMsGraphConnectionStatusService();
        span.setStatus({ code: SpanStatusCode.OK });
        return {
          success: true,
          message: "Microsoft Graph connection status fetched successfully",
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

export async function getAzureOpenAiConnectionStatusAction(): Promise<
  ActionResult<AzureOpenAiConnectionStatusDto>
> {
  return tracer.startActiveSpan(
    "Get Azure OpenAI Connection Status",
    async (span: Span) => {
      try {
        const status = await getAzureOpenAiConnectionStatusService();
        span.setStatus({ code: SpanStatusCode.OK });
        return {
          success: true,
          message: "Azure OpenAI connection status fetched successfully",
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
