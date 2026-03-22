"use server";

import { Span, SpanStatusCode, trace } from "@opentelemetry/api";
import {
  getHealthStatus,
  getDashboardSummary,
  getDashboardCalendar,
  type HealthStatusDto,
  type DashboardSummaryDto,
  type DashboardCalendarDto,
} from "@/server/Services/dashboardService";

const tracer = trace.getTracer("web");

export interface ActionResult<T = undefined> {
  success: boolean;
  message: string;
  data?: T;
}

export async function checkApiHealth(): Promise<ActionResult<HealthStatusDto>> {
  return tracer.startActiveSpan("Check API Health", async (span: Span) => {
      try {
        const health = await getHealthStatus();
        span.setStatus({ code: SpanStatusCode.OK });
        return {
          success: true,
          message: "API is healthy",
          data: health,
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

export async function getDashboardSummaryAction(): Promise<
  ActionResult<DashboardSummaryDto>
> {
  return tracer.startActiveSpan(
    "Get Dashboard Summary",
    async (span: Span) => {
      try {
        const summary = await getDashboardSummary();
        span.setStatus({ code: SpanStatusCode.OK });
        return {
          success: true,
          message: "Dashboard summary fetched successfully",
          data: summary,
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

export async function getDashboardCalendarAction(
  startDate: string,
  endDate: string,
): Promise<ActionResult<DashboardCalendarDto>> {
  return tracer.startActiveSpan(
    "Get Dashboard Calendar",
    async (span: Span) => {
      try {
        const calendar = await getDashboardCalendar(startDate, endDate);
        span.setStatus({ code: SpanStatusCode.OK });
        return {
          success: true,
          message: "Dashboard calendar fetched successfully",
          data: calendar,
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
