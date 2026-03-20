"use server";

import { Span, SpanStatusCode, trace } from "@opentelemetry/api";
import {
  createClient as createClientService,
  type ClientDto,
  type CreateClientParams,
} from "@/server/Services/clientService";

const tracer = trace.getTracer("web");

export interface ActionResult<T = undefined> {
  success: boolean;
  message: string;
  data?: T;
}

export async function createClientAction(
  params: CreateClientParams,
): Promise<ActionResult<ClientDto>> {
  return tracer.startActiveSpan("Create Client", async (span: Span) => {
    try {
      const client = await createClientService(params);
      span.setStatus({ code: SpanStatusCode.OK });
      return {
        success: true,
        message: "Client created successfully",
        data: client,
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
