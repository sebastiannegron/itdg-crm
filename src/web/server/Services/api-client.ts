const API_BASE = process.env.API_BASE_URL ?? "http://localhost:5000";

/**
 * Typed error response matching the .NET ProblemDetails format.
 */
export interface ApiErrorResponse {
  type?: string;
  title?: string;
  status: number;
  detail?: string;
  errors?: Record<string, string[]>;
  errorCode?: string;
}

/**
 * Typed error thrown when the .NET API returns a non-success status code.
 */
export class ApiError extends Error {
  public readonly status: number;
  public readonly errorCode?: string;
  public readonly detail?: string;
  public readonly errors?: Record<string, string[]>;

  constructor(response: ApiErrorResponse) {
    super(response.detail ?? response.title ?? `API error: ${response.status}`);
    this.name = "ApiError";
    this.status = response.status;
    this.errorCode = response.errorCode;
    this.detail = response.detail;
    this.errors = response.errors;
  }
}

/**
 * Acquires an MSAL bearer token for the .NET API.
 * Placeholder until Microsoft Entra ID integration is configured.
 */
async function getAuthToken(): Promise<string | null> {
  // TODO: Acquire MSAL token via @azure/msal-node ConfidentialClientApplication
  // when Microsoft Entra ID auth is added (Epic 0.7).
  return null;
}

/**
 * Generic fetch wrapper for the .NET API backend.
 *
 * - Attaches `Authorization` header when an MSAL token is available.
 * - Sends a `X-Correlation-Id` header for request tracing.
 * - Throws `ApiError` with typed ProblemDetails on non-success responses.
 */
export async function apiFetch<T>(
  path: string,
  options?: RequestInit,
): Promise<T> {
  const token = await getAuthToken();
  const correlationId = crypto.randomUUID();

  const headers: Record<string, string> = {
    "Content-Type": "application/json",
    "X-Correlation-Id": correlationId,
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
    ...(options?.headers as Record<string, string> | undefined),
  };

  const res = await fetch(`${API_BASE}${path}`, {
    ...options,
    headers,
  });

  if (!res.ok) {
    let errorResponse: ApiErrorResponse;
    try {
      const body = await res.json();
      errorResponse = { status: res.status, ...body };
    } catch {
      errorResponse = { status: res.status, detail: res.statusText };
    }
    throw new ApiError(errorResponse);
  }

  if (res.status === 204) {
    return undefined as T;
  }

  return res.json() as Promise<T>;
}
