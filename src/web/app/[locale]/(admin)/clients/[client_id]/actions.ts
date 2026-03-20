"use server";

import { Span, SpanStatusCode, trace } from "@opentelemetry/api";
import {
  getClientById,
  updateClient as updateClientService,
  type ClientDto,
  type UpdateClientParams,
} from "@/server/Services/clientService";

const tracer = trace.getTracer("web");

export interface ActionResult<T = undefined> {
  success: boolean;
  message: string;
  data?: T;
}

export async function fetchClientById(
  clientId: string,
): Promise<ActionResult<ClientDto>> {
  return tracer.startActiveSpan("Fetch Client By Id", async (span: Span) => {
    try {
      span.setAttribute("client_id", clientId);
      const client = await getClientById(clientId);
      span.setStatus({ code: SpanStatusCode.OK });
      return {
        success: true,
        message: "Client fetched successfully",
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

export async function updateClientAction(
  clientId: string,
  params: UpdateClientParams,
): Promise<ActionResult> {
  return tracer.startActiveSpan("Update Client", async (span: Span) => {
    try {
      span.setAttribute("client_id", clientId);
      await updateClientService(clientId, params);
      span.setStatus({ code: SpanStatusCode.OK });
      return {
        success: true,
        message: "Client updated successfully",
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
