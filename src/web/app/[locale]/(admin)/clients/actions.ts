"use server";

import { Span, SpanStatusCode, trace } from "@opentelemetry/api";
import {
  getClients,
  type GetClientsParams,
  type PaginatedResult,
  type ClientDto,
} from "@/server/Services/clientService";

const tracer = trace.getTracer("web");

export interface ActionResult<T = undefined> {
  success: boolean;
  message: string;
  data?: T;
}

export async function fetchClients(
  params?: GetClientsParams,
): Promise<ActionResult<PaginatedResult<ClientDto>>> {
  return tracer.startActiveSpan("Fetch Clients", async (span: Span) => {
    try {
      const clients = await getClients(params);
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
