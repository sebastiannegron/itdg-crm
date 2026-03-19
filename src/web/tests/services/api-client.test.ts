import { describe, it, expect, vi, beforeEach } from "vitest";
import { apiFetch, ApiError } from "@/server/Services/api-client";

const fetchMock = vi.fn();
vi.stubGlobal("fetch", fetchMock);

beforeEach(() => {
  fetchMock.mockReset();
});

describe("apiFetch", () => {
  it("returns parsed JSON on success", async () => {
    fetchMock.mockResolvedValue({
      ok: true,
      status: 200,
      json: () => Promise.resolve({ status: "Healthy" }),
    });

    const result = await apiFetch<{ status: string }>("/api/v1/Health");

    expect(result).toEqual({ status: "Healthy" });
    expect(fetchMock).toHaveBeenCalledOnce();
    const [url, options] = fetchMock.mock.calls[0];
    expect(url).toBe("http://localhost:5000/api/v1/Health");
    expect(options.headers["Content-Type"]).toBe("application/json");
    expect(options.headers["X-Correlation-Id"]).toBeDefined();
  });

  it("sends custom headers alongside defaults", async () => {
    fetchMock.mockResolvedValue({
      ok: true,
      status: 200,
      json: () => Promise.resolve({}),
    });

    await apiFetch("/test", {
      headers: { "X-Custom": "value" },
    });

    const [, options] = fetchMock.mock.calls[0];
    expect(options.headers["X-Custom"]).toBe("value");
    expect(options.headers["Content-Type"]).toBe("application/json");
  });

  it("returns undefined for 204 No Content", async () => {
    fetchMock.mockResolvedValue({
      ok: true,
      status: 204,
    });

    const result = await apiFetch("/api/v1/Resource");

    expect(result).toBeUndefined();
  });

  it("throws ApiError with ProblemDetails on error response", async () => {
    fetchMock.mockResolvedValue({
      ok: false,
      status: 404,
      json: () =>
        Promise.resolve({
          detail: "Client not found",
          errorCode: "client_not_found",
        }),
    });

    await expect(apiFetch("/api/v1/Clients/123")).rejects.toThrow(ApiError);

    try {
      await apiFetch("/api/v1/Clients/123");
    } catch (err) {
      const apiError = err as ApiError;
      expect(apiError.status).toBe(404);
      expect(apiError.errorCode).toBe("client_not_found");
      expect(apiError.detail).toBe("Client not found");
      expect(apiError.message).toBe("Client not found");
    }
  });

  it("throws ApiError with statusText when body is not JSON", async () => {
    fetchMock.mockResolvedValue({
      ok: false,
      status: 500,
      statusText: "Internal Server Error",
      json: () => Promise.reject(new Error("not JSON")),
    });

    try {
      await apiFetch("/api/v1/Broken");
    } catch (err) {
      const apiError = err as ApiError;
      expect(apiError).toBeInstanceOf(ApiError);
      expect(apiError.status).toBe(500);
      expect(apiError.detail).toBe("Internal Server Error");
    }
  });

  it("forwards method and body in options", async () => {
    fetchMock.mockResolvedValue({
      ok: true,
      status: 200,
      json: () => Promise.resolve({ id: "abc" }),
    });

    await apiFetch("/api/v1/Clients", {
      method: "POST",
      body: JSON.stringify({ name: "Test Client" }),
    });

    const [, options] = fetchMock.mock.calls[0];
    expect(options.method).toBe("POST");
    expect(options.body).toBe(JSON.stringify({ name: "Test Client" }));
  });
});

describe("ApiError", () => {
  it("uses detail as message when available", () => {
    const error = new ApiError({
      status: 400,
      detail: "Validation failed",
      errorCode: "validation_error",
    });

    expect(error.message).toBe("Validation failed");
    expect(error.name).toBe("ApiError");
    expect(error.status).toBe(400);
    expect(error.errorCode).toBe("validation_error");
  });

  it("uses title as fallback message", () => {
    const error = new ApiError({
      status: 409,
      title: "Conflict",
    });

    expect(error.message).toBe("Conflict");
  });

  it("uses generic message when no detail or title", () => {
    const error = new ApiError({ status: 503 });

    expect(error.message).toBe("API error: 503");
  });

  it("includes field-level errors when present", () => {
    const error = new ApiError({
      status: 400,
      detail: "Validation failed",
      errors: { name: ["Name is required"] },
    });

    expect(error.errors).toEqual({ name: ["Name is required"] });
  });
});
