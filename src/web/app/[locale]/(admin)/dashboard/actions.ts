"use server";

import { Span, SpanStatusCode, trace } from "@opentelemetry/api";
import {
  getHealthStatus,
  type HealthStatusDto,
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
