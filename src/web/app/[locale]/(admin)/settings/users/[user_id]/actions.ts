"use server";

import { Span, SpanStatusCode, trace } from "@opentelemetry/api";
import {
  getUserById,
  updateUser as updateUserService,
  type UserDto,
  type UpdateUserParams,
} from "@/server/Services/userService";

const tracer = trace.getTracer("web");

export interface ActionResult<T = undefined> {
  success: boolean;
  message: string;
  data?: T;
}

export async function getUserAction(
  userId: string,
): Promise<ActionResult<UserDto>> {
  return tracer.startActiveSpan("Get User", async (span: Span) => {
    try {
      span.setAttribute("user_id", userId);
      const user = await getUserById(userId);
      span.setStatus({ code: SpanStatusCode.OK });
      return {
        success: true,
        message: "User fetched successfully",
        data: user,
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

export async function updateUserAction(
  userId: string,
  params: UpdateUserParams,
): Promise<ActionResult> {
  return tracer.startActiveSpan("Update User", async (span: Span) => {
    try {
      span.setAttribute("user_id", userId);
      await updateUserService(userId, params);
      span.setStatus({ code: SpanStatusCode.OK });
      return {
        success: true,
        message: "User updated successfully",
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
