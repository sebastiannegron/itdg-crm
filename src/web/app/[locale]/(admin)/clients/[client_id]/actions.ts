"use server";

import { Span, SpanStatusCode, trace } from "@opentelemetry/api";
import {
  getClientById,
  updateClient as updateClientService,
  getClientAssignments as getClientAssignmentsService,
  assignClient as assignClientService,
  unassignClient as unassignClientService,
  type ClientDto,
  type UpdateClientParams,
  type ClientAssignmentDto,
} from "@/server/Services/clientService";
import {
  getClientDocuments,
  getDocumentDetail,
  uploadNewVersion as uploadNewVersionService,
  deleteDocument as deleteDocumentService,
  type GetClientDocumentsParams,
  type PaginatedDocuments,
  type DocumentDetailDto,
} from "@/server/Services/documentService";
import {
  getClientEmails,
  type GetClientEmailsParams,
  type PaginatedEmails,
} from "@/server/Services/emailMirrorService";
import {
  getClientTimeline,
  type GetClientTimelineParams,
  type PaginatedTimeline,
} from "@/server/Services/timelineService";

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

export async function fetchClientAssignments(
  clientId: string,
): Promise<ActionResult<ClientAssignmentDto[]>> {
  return tracer.startActiveSpan(
    "Fetch Client Assignments",
    async (span: Span) => {
      try {
        span.setAttribute("client_id", clientId);
        const assignments = await getClientAssignmentsService(clientId);
        span.setStatus({ code: SpanStatusCode.OK });
        return {
          success: true,
          message: "Assignments fetched successfully",
          data: assignments,
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

export async function assignClientAction(
  clientId: string,
  userId: string,
): Promise<ActionResult> {
  return tracer.startActiveSpan("Assign Client", async (span: Span) => {
    try {
      span.setAttribute("client_id", clientId);
      span.setAttribute("user_id", userId);
      await assignClientService(clientId, userId);
      span.setStatus({ code: SpanStatusCode.OK });
      return {
        success: true,
        message: "Associate assigned successfully",
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

export async function unassignClientAction(
  clientId: string,
  userId: string,
): Promise<ActionResult> {
  return tracer.startActiveSpan("Unassign Client", async (span: Span) => {
    try {
      span.setAttribute("client_id", clientId);
      span.setAttribute("user_id", userId);
      await unassignClientService(clientId, userId);
      span.setStatus({ code: SpanStatusCode.OK });
      return {
        success: true,
        message: "Associate removed successfully",
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

export async function fetchClientDocumentsAction(
  params: GetClientDocumentsParams,
): Promise<ActionResult<PaginatedDocuments>> {
  return tracer.startActiveSpan(
    "Fetch Client Documents",
    async (span: Span) => {
      try {
        const documents = await getClientDocuments(params);
        span.setStatus({ code: SpanStatusCode.OK });
        return {
          success: true,
          message: "Documents fetched successfully",
          data: documents,
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

export async function fetchDocumentDetailAction(
  documentId: string,
): Promise<ActionResult<DocumentDetailDto>> {
  return tracer.startActiveSpan(
    "Fetch Document Detail",
    async (span: Span) => {
      try {
        span.setAttribute("document_id", documentId);
        const detail = await getDocumentDetail(documentId);
        span.setStatus({ code: SpanStatusCode.OK });
        return {
          success: true,
          message: "Document detail fetched successfully",
          data: detail,
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

export async function uploadNewVersionAction(
  documentId: string,
  formData: FormData,
): Promise<ActionResult> {
  return tracer.startActiveSpan(
    "Upload New Version",
    async (span: Span) => {
      try {
        span.setAttribute("document_id", documentId);
        const file = formData.get("file") as File | null;
        if (!file) {
          return { success: false, message: "A file is required." };
        }
        await uploadNewVersionService(documentId, file);
        span.setStatus({ code: SpanStatusCode.OK });
        return {
          success: true,
          message: "New version uploaded successfully",
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

export async function deleteDocumentAction(
  documentId: string,
): Promise<ActionResult> {
  return tracer.startActiveSpan(
    "Delete Document",
    async (span: Span) => {
      try {
        span.setAttribute("document_id", documentId);
        await deleteDocumentService(documentId);
        span.setStatus({ code: SpanStatusCode.OK });
        return {
          success: true,
          message: "Document deleted successfully",
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

export async function fetchClientEmailsAction(
  params: GetClientEmailsParams,
): Promise<ActionResult<PaginatedEmails>> {
  return tracer.startActiveSpan(
    "Fetch Client Emails",
    async (span: Span) => {
      try {
        const emails = await getClientEmails(params);
        span.setStatus({ code: SpanStatusCode.OK });
        return {
          success: true,
          message: "Emails fetched successfully",
          data: emails,
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

export async function fetchClientTimelineAction(
  params: GetClientTimelineParams,
): Promise<ActionResult<PaginatedTimeline>> {
  return tracer.startActiveSpan(
    "Fetch Client Timeline",
    async (span: Span) => {
      try {
        span.setAttribute("client_id", params.clientId);
        const timeline = await getClientTimeline(params);
        span.setStatus({ code: SpanStatusCode.OK });
        return {
          success: true,
          message: "Timeline fetched successfully",
          data: timeline,
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
