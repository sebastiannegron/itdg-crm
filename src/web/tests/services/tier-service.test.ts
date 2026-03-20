import { describe, it, expect, vi, beforeEach } from "vitest";
import {
  getTiers,
  createTier,
  updateTier,
} from "@/server/Services/tierService";
import { apiFetch } from "@/server/Services/api-client";

vi.mock("@/server/Services/api-client", () => ({
  apiFetch: vi.fn(),
}));

const mockApiFetch = vi.mocked(apiFetch);

beforeEach(() => {
  mockApiFetch.mockReset();
});

describe("getTiers", () => {
  it("calls apiFetch with correct path", async () => {
    const mockResponse = [
      {
        tier_id: "t1",
        name: "Tier 1",
        sort_order: 1,
        created_at: "2025-01-01T00:00:00Z",
        updated_at: "2025-01-01T00:00:00Z",
      },
    ];
    mockApiFetch.mockResolvedValue(mockResponse);

    const result = await getTiers();

    expect(mockApiFetch).toHaveBeenCalledWith("/api/v1/Tiers");
    expect(result).toEqual(mockResponse);
  });

  it("returns empty array when no tiers exist", async () => {
    mockApiFetch.mockResolvedValue([]);

    const result = await getTiers();

    expect(result).toEqual([]);
  });
});

describe("createTier", () => {
  it("calls apiFetch with POST method and body", async () => {
    mockApiFetch.mockResolvedValue(undefined);

    const params = { name: "Tier 4", sort_order: 4 };
    await createTier(params);

    expect(mockApiFetch).toHaveBeenCalledWith("/api/v1/Tiers", {
      method: "POST",
      body: JSON.stringify(params),
    });
  });
});

describe("updateTier", () => {
  it("calls apiFetch with PUT method and body", async () => {
    mockApiFetch.mockResolvedValue(undefined);

    const params = { name: "Updated Tier", sort_order: 5 };
    await updateTier("t1-uuid", params);

    expect(mockApiFetch).toHaveBeenCalledWith("/api/v1/Tiers/t1-uuid", {
      method: "PUT",
      body: JSON.stringify(params),
    });
  });
});
