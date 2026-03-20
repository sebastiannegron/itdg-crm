import { describe, it, expect, vi, beforeEach } from "vitest";
import {
  getPortalMessages,
  sendPortalMessage,
  markMessageAsRead,
} from "@/server/Services/portalMessageService";
import { apiFetch } from "@/server/Services/api-client";

vi.mock("@/server/Services/api-client", () => ({
  apiFetch: vi.fn(),
}));

const mockApiFetch = vi.mocked(apiFetch);

beforeEach(() => {
  mockApiFetch.mockReset();
});

describe("getPortalMessages", () => {
  it("calls apiFetch with correct path", async () => {
    const mockMessages = [
      {
        id: "m1",
        client_id: "c1",
        sender_id: "s1",
        direction: "Inbound",
        subject: "Tax Update",
        body: "Your tax filing is ready.",
        template_id: null,
        is_portal_message: true,
        is_read: false,
        attachments: null,
        created_at: "2025-01-15T10:00:00Z",
      },
    ];
    mockApiFetch.mockResolvedValue(mockMessages);

    const result = await getPortalMessages();

    expect(mockApiFetch).toHaveBeenCalledWith("/api/v1/Portal/Messages");
    expect(result).toEqual(mockMessages);
  });

  it("returns empty array when no messages", async () => {
    mockApiFetch.mockResolvedValue([]);

    const result = await getPortalMessages();

    expect(result).toEqual([]);
  });

  it("returns multiple messages", async () => {
    const mockMessages = [
      {
        id: "m1",
        client_id: "c1",
        sender_id: "s1",
        direction: "Inbound",
        subject: "First Message",
        body: "Body 1",
        template_id: null,
        is_portal_message: true,
        is_read: true,
        attachments: null,
        created_at: "2025-01-15T10:00:00Z",
      },
      {
        id: "m2",
        client_id: "c1",
        sender_id: "c1",
        direction: "Outbound",
        subject: "Reply",
        body: "Body 2",
        template_id: null,
        is_portal_message: true,
        is_read: true,
        attachments: null,
        created_at: "2025-01-16T10:00:00Z",
      },
    ];
    mockApiFetch.mockResolvedValue(mockMessages);

    const result = await getPortalMessages();

    expect(result).toHaveLength(2);
    expect(result[0].subject).toBe("First Message");
    expect(result[1].subject).toBe("Reply");
  });
});

describe("sendPortalMessage", () => {
  it("calls apiFetch with POST method and correct body", async () => {
    mockApiFetch.mockResolvedValue(undefined);

    await sendPortalMessage("Test Subject", "Test Body");

    expect(mockApiFetch).toHaveBeenCalledWith("/api/v1/Portal/Messages", {
      method: "POST",
      body: JSON.stringify({ subject: "Test Subject", body: "Test Body" }),
    });
  });
});

describe("markMessageAsRead", () => {
  it("calls apiFetch with PUT method and correct path", async () => {
    mockApiFetch.mockResolvedValue(undefined);

    await markMessageAsRead("msg-123");

    expect(mockApiFetch).toHaveBeenCalledWith(
      "/api/v1/Portal/Messages/msg-123/Read",
      { method: "PUT" },
    );
  });
});
