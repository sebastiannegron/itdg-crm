"use server";

import { Span, SpanStatusCode, trace } from "@opentelemetry/api";
import {
  getTiers as getTiersService,
  createTier as createTierService,
  updateTier as updateTierService,
  type ClientTierDto,
  type CreateTierParams,
  type UpdateTierParams,
} from "@/server/Services/tierService";

const tracer = trace.getTracer("web");

export interface ActionResult<T = undefined> {
  success: boolean;
  message: string;
  data?: T;
}

export async function getTiersAction(): Promise<
  ActionResult<ClientTierDto[]>
> {
  return tracer.startActiveSpan("Get Tiers", async (span: Span) => {
    try {
      const tiers = await getTiersService();
      span.setStatus({ code: SpanStatusCode.OK });
      return {
        success: true,
        message: "Tiers fetched successfully",
        data: tiers,
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

export async function createTierAction(
  params: CreateTierParams,
): Promise<ActionResult> {
  return tracer.startActiveSpan("Create Tier", async (span: Span) => {
    try {
      await createTierService(params);
      span.setStatus({ code: SpanStatusCode.OK });
      return {
        success: true,
        message: "Tier created successfully",
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

export async function updateTierAction(
  tierId: string,
  params: UpdateTierParams,
): Promise<ActionResult> {
  return tracer.startActiveSpan("Update Tier", async (span: Span) => {
    try {
      await updateTierService(tierId, params);
      span.setStatus({ code: SpanStatusCode.OK });
      return {
        success: true,
        message: "Tier updated successfully",
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
