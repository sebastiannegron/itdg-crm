import { describe, it, expect, vi, beforeEach } from "vitest";
import { getClients } from "@/server/Services/clientService";
import { apiFetch } from "@/server/Services/api-client";

vi.mock("@/server/Services/api-client", () => ({
  apiFetch: vi.fn(),
}));

const mockApiFetch = vi.mocked(apiFetch);

beforeEach(() => {
  mockApiFetch.mockReset();
});

describe("getClients", () => {
  it("calls apiFetch with default path when no params", async () => {
    const mockResponse = {
      items: [],
      total_count: 0,
      page: 1,
      page_size: 20,
    };
    mockApiFetch.mockResolvedValue(mockResponse);

    const result = await getClients();

    expect(mockApiFetch).toHaveBeenCalledWith("/api/v1/Clients");
    expect(result).toEqual(mockResponse);
  });

  it("includes page and pageSize in query string", async () => {
    mockApiFetch.mockResolvedValue({ items: [], total_count: 0, page: 2, page_size: 10 });

    await getClients({ page: 2, pageSize: 10 });

    expect(mockApiFetch).toHaveBeenCalledWith(
      expect.stringContaining("page=2")
    );
    expect(mockApiFetch).toHaveBeenCalledWith(
      expect.stringContaining("pageSize=10")
    );
  });

  it("includes status filter in query string", async () => {
    mockApiFetch.mockResolvedValue({ items: [], total_count: 0, page: 1, page_size: 20 });

    await getClients({ status: "Active" });

    expect(mockApiFetch).toHaveBeenCalledWith(
      expect.stringContaining("status=Active")
    );
  });

  it("includes tierId filter in query string", async () => {
    mockApiFetch.mockResolvedValue({ items: [], total_count: 0, page: 1, page_size: 20 });

    await getClients({ tierId: "abc-123" });

    expect(mockApiFetch).toHaveBeenCalledWith(
      expect.stringContaining("tierId=abc-123")
    );
  });

  it("includes search in query string", async () => {
    mockApiFetch.mockResolvedValue({ items: [], total_count: 0, page: 1, page_size: 20 });

    await getClients({ search: "Acme" });

    expect(mockApiFetch).toHaveBeenCalledWith(
      expect.stringContaining("search=Acme")
    );
  });

  it("omits undefined params from query string", async () => {
    mockApiFetch.mockResolvedValue({ items: [], total_count: 0, page: 1, page_size: 20 });

    await getClients({ page: 1 });

    const calledPath = mockApiFetch.mock.calls[0][0] as string;
    expect(calledPath).not.toContain("status");
    expect(calledPath).not.toContain("tierId");
    expect(calledPath).not.toContain("search");
  });

  it("returns client data from API response", async () => {
    const mockClients = {
      items: [
        {
          client_id: "c1",
          name: "Acme Corp",
          contact_email: "info@acme.com",
          phone: null,
          address: null,
          tier_id: "t1",
          tier_name: "Tier 1",
          status: "Active",
          industry_tag: "Technology",
          notes: null,
          custom_fields: null,
          created_at: "2024-01-01T00:00:00Z",
          updated_at: "2024-06-01T00:00:00Z",
        },
      ],
      total_count: 1,
      page: 1,
      page_size: 20,
    };
    mockApiFetch.mockResolvedValue(mockClients);

    const result = await getClients();

    expect(result.items).toHaveLength(1);
    expect(result.items[0].name).toBe("Acme Corp");
    expect(result.total_count).toBe(1);
  });
});
