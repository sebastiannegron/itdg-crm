"use server";

import { Span, SpanStatusCode, trace } from "@opentelemetry/api";
import {
  getAllUsers,
  inviteUser as inviteUserService,
  type PaginatedUsers,
  type InviteUserParams,
} from "@/server/Services/userService";

const tracer = trace.getTracer("web");

export interface ActionResult<T = undefined> {
  success: boolean;
  message: string;
  data?: T;
}

export async function getUsersAction(): Promise<ActionResult<PaginatedUsers>> {
  return tracer.startActiveSpan("Get Users", async (span: Span) => {
    try {
      const users = await getAllUsers();
      span.setStatus({ code: SpanStatusCode.OK });
      return {
        success: true,
        message: "Users fetched successfully",
        data: users,
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

export async function inviteUserAction(
  params: InviteUserParams,
): Promise<ActionResult> {
  return tracer.startActiveSpan("Invite User", async (span: Span) => {
    try {
      span.setAttribute("email", params.email);
      await inviteUserService(params);
      span.setStatus({ code: SpanStatusCode.OK });
      return {
        success: true,
        message: "Invitation sent successfully",
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
