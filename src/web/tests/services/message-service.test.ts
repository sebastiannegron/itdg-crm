import { describe, it, expect, vi, beforeEach } from "vitest";
import {
  sendTemplateMessage,
  type SendTemplateMessageParams,
} from "@/server/Services/messageService";
import { apiFetch } from "@/server/Services/api-client";

vi.mock("@/server/Services/api-client", () => ({
  apiFetch: vi.fn(),
}));

const mockApiFetch = vi.mocked(apiFetch);

beforeEach(() => {
  mockApiFetch.mockReset();
});

describe("sendTemplateMessage", () => {
  it("calls apiFetch with POST method and correct body", async () => {
    mockApiFetch.mockResolvedValue(undefined);

    const params: SendTemplateMessageParams = {
      template_id: "tmpl-123",
      client_id: "client-456",
      merge_fields: { client_name: "John Doe" },
      send_via_portal: true,
      send_via_email: false,
    };

    await sendTemplateMessage(params);

    expect(mockApiFetch).toHaveBeenCalledWith("/api/v1/Messages/SendTemplate", {
      method: "POST",
      body: JSON.stringify(params),
    });
  });

  it("sends with email recipient when send_via_email is true", async () => {
    mockApiFetch.mockResolvedValue(undefined);

    const params: SendTemplateMessageParams = {
      template_id: "tmpl-789",
      client_id: "client-012",
      merge_fields: { client_name: "Jane" },
      send_via_portal: false,
      send_via_email: true,
      recipient_email: "jane@example.com",
    };

    await sendTemplateMessage(params);

    expect(mockApiFetch).toHaveBeenCalledWith("/api/v1/Messages/SendTemplate", {
      method: "POST",
      body: JSON.stringify(params),
    });
  });

  it("sends with both channels enabled", async () => {
    mockApiFetch.mockResolvedValue(undefined);

    const params: SendTemplateMessageParams = {
      template_id: "tmpl-abc",
      client_id: "client-def",
      merge_fields: { client_name: "Bob", due_date: "April 15" },
      send_via_portal: true,
      send_via_email: true,
      recipient_email: "bob@example.com",
    };

    await sendTemplateMessage(params);

    expect(mockApiFetch).toHaveBeenCalledWith("/api/v1/Messages/SendTemplate", {
      method: "POST",
      body: JSON.stringify(params),
    });
  });

  it("propagates errors from apiFetch", async () => {
    mockApiFetch.mockRejectedValue(new Error("Network error"));

    const params: SendTemplateMessageParams = {
      template_id: "tmpl-err",
      client_id: "client-err",
      merge_fields: {},
      send_via_portal: true,
      send_via_email: false,
    };

    await expect(sendTemplateMessage(params)).rejects.toThrow("Network error");
  });
});
