import { render, screen, fireEvent } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import type { ClientTierDto } from "@/app/[locale]/(admin)/settings/shared";

vi.mock("next-intl", () => ({
  useLocale: () => "en-pr",
}));

vi.mock("@/i18n/routing", () => ({
  useRouter: () => ({
    push: vi.fn(),
  }),
  usePathname: () => "/settings",
  Link: ({
    href,
    children,
    ...props
  }: {
    href: string;
    children: React.ReactNode;
  }) => (
    <a href={href} {...props}>
      {children}
    </a>
  ),
}));

vi.mock("@/app/[locale]/(admin)/settings/actions", () => ({
  createTierAction: vi.fn().mockResolvedValue({
    success: true,
    message: "Tier created successfully",
  }),
  updateTierAction: vi.fn().mockResolvedValue({
    success: true,
    message: "Tier updated successfully",
  }),
  getTiersAction: vi.fn().mockResolvedValue({
    success: true,
    data: [],
  }),
  getDocumentCategoriesAction: vi.fn().mockResolvedValue({
    success: true,
    data: [],
  }),
  createDocumentCategoryAction: vi.fn().mockResolvedValue({
    success: true,
    message: "Category created successfully",
  }),
  updateDocumentCategoryAction: vi.fn().mockResolvedValue({
    success: true,
    message: "Category updated successfully",
  }),
  deleteDocumentCategoryAction: vi.fn().mockResolvedValue({
    success: true,
    message: "Category deleted successfully",
  }),
  reorderDocumentCategoriesAction: vi.fn().mockResolvedValue({
    success: true,
    message: "Categories reordered successfully",
  }),
}));

import SettingsView from "@/app/[locale]/(admin)/settings/SettingsView";

function createTier(
  overrides: Partial<ClientTierDto> = {},
): ClientTierDto {
  return {
    tier_id: "tier-1-uuid",
    name: "Tier 1",
    sort_order: 1,
    created_at: "2025-01-01T00:00:00Z",
    updated_at: "2025-01-01T00:00:00Z",
    ...overrides,
  };
}

const sampleTiers: ClientTierDto[] = [
  createTier({ tier_id: "tier-1-uuid", name: "Tier 1", sort_order: 1 }),
  createTier({ tier_id: "tier-2-uuid", name: "Tier 2", sort_order: 2 }),
  createTier({ tier_id: "tier-3-uuid", name: "Tier 3", sort_order: 3 }),
];

describe("SettingsView", () => {
  it("renders the page header with Settings title", () => {
    render(<SettingsView initialTiers={sampleTiers} />);
    expect(
      screen.getByRole("heading", { level: 1 }),
    ).toHaveTextContent("Settings");
  });

  it("renders breadcrumbs with Dashboard and Settings", () => {
    render(<SettingsView initialTiers={sampleTiers} />);
    expect(screen.getByText("Dashboard")).toBeInTheDocument();
  });

  it("renders the Client Tiers section heading", () => {
    render(<SettingsView initialTiers={sampleTiers} />);
    expect(
      screen.getByRole("heading", { level: 2 }),
    ).toHaveTextContent("Client Tiers");
  });

  it("renders all initial tiers", () => {
    render(<SettingsView initialTiers={sampleTiers} />);
    expect(screen.getByText("Tier 1")).toBeInTheDocument();
    expect(screen.getByText("Tier 2")).toBeInTheDocument();
    expect(screen.getByText("Tier 3")).toBeInTheDocument();
  });

  it("renders sort order badges for each tier", () => {
    render(<SettingsView initialTiers={sampleTiers} />);
    expect(screen.getByText("1")).toBeInTheDocument();
    expect(screen.getByText("2")).toBeInTheDocument();
    expect(screen.getByText("3")).toBeInTheDocument();
  });

  it("renders New Tier button", () => {
    render(<SettingsView initialTiers={sampleTiers} />);
    expect(
      screen.getByRole("button", { name: /New Tier/i }),
    ).toBeInTheDocument();
  });

  it("renders Edit buttons for each tier", () => {
    render(<SettingsView initialTiers={sampleTiers} />);
    const editButtons = screen.getAllByRole("button", { name: /Edit/i });
    expect(editButtons).toHaveLength(3);
  });

  it("shows create form when New Tier button is clicked", () => {
    render(<SettingsView initialTiers={sampleTiers} />);
    const newTierButton = screen.getByRole("button", { name: /New Tier/i });
    fireEvent.click(newTierButton);
    expect(screen.getByLabelText(/Name/)).toBeInTheDocument();
    expect(screen.getByLabelText(/Sort Order/)).toBeInTheDocument();
  });

  it("shows edit form when Edit button is clicked", () => {
    render(<SettingsView initialTiers={sampleTiers} />);
    const editButtons = screen.getAllByRole("button", { name: /Edit/i });
    fireEvent.click(editButtons[0]);
    expect(screen.getByDisplayValue("Tier 1")).toBeInTheDocument();
  });

  it("renders empty state when no tiers exist", () => {
    render(<SettingsView initialTiers={[]} />);
    expect(
      screen.getByText("No tiers configured yet."),
    ).toBeInTheDocument();
  });

  it("hides create form when Cancel button is clicked", () => {
    render(<SettingsView initialTiers={sampleTiers} />);
    const newTierButton = screen.getByRole("button", { name: /New Tier/i });
    fireEvent.click(newTierButton);
    expect(screen.getByLabelText(/Name/)).toBeInTheDocument();

    const cancelButton = screen.getByRole("button", { name: /Cancel/i });
    fireEvent.click(cancelButton);

    // Create form should be gone, verify by checking Cancel button is gone
    expect(
      screen.queryByRole("button", { name: /Cancel/i }),
    ).not.toBeInTheDocument();
  });
});
