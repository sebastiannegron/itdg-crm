import { render, screen, fireEvent, within } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import type { PaginatedResult, ClientDto } from "@/app/[locale]/(admin)/clients/shared";

vi.mock("next-intl", () => ({
  useLocale: () => "en-pr",
}));

vi.mock("@/i18n/routing", () => ({
  useRouter: () => ({
    push: vi.fn(),
  }),
  usePathname: () => "/clients",
  Link: ({ href, children, ...props }: { href: string; children: React.ReactNode }) => (
    <a href={href} {...props}>{children}</a>
  ),
}));

import ClientsView from "@/app/[locale]/(admin)/clients/ClientsView";

function createClient(overrides: Partial<ClientDto> = {}): ClientDto {
  return {
    client_id: "c1-uuid",
    name: "Acme Corp",
    contact_email: "info@acme.com",
    phone: "787-555-0100",
    address: "123 Main St",
    tier_id: "t1-uuid",
    tier_name: "Tier 1",
    status: "Active",
    industry_tag: "Technology",
    notes: null,
    custom_fields: null,
    created_at: "2024-01-15T10:00:00Z",
    updated_at: "2024-06-01T14:30:00Z",
    ...overrides,
  };
}

function createPaginatedData(
  clients: ClientDto[] = [],
): PaginatedResult<ClientDto> {
  return {
    items: clients,
    total_count: clients.length,
    page: 1,
    page_size: 20,
  };
}

function getDesktopTable() {
  return document.querySelector(".hidden.md\\:block") as HTMLElement;
}

describe("ClientsView", () => {
  it("renders the page header with Clients title", () => {
    render(<ClientsView initialData={createPaginatedData()} />);
    expect(screen.getByRole("heading", { level: 1 })).toHaveTextContent("Clients");
  });

  it("renders breadcrumbs with Dashboard and Clients", () => {
    render(<ClientsView initialData={createPaginatedData()} />);
    expect(screen.getByText("Dashboard")).toBeInTheDocument();
  });

  it("renders empty state when no clients", () => {
    render(<ClientsView initialData={createPaginatedData()} />);
    expect(screen.getByText("No clients yet")).toBeInTheDocument();
    expect(
      screen.getByText("Clients will appear here once they are added to the system.")
    ).toBeInTheDocument();
  });

  it("renders DataTable with client data", () => {
    const clients = [
      createClient(),
      createClient({
        client_id: "c2-uuid",
        name: "Beta Inc",
        contact_email: "hello@beta.com",
        tier_name: "Tier 2",
        status: "Inactive",
        industry_tag: "Finance",
      }),
    ];
    render(<ClientsView initialData={createPaginatedData(clients)} />);

    const desktop = getDesktopTable();
    expect(desktop).toBeInTheDocument();
    expect(within(desktop).getByText("Acme Corp")).toBeInTheDocument();
    expect(within(desktop).getByText("Beta Inc")).toBeInTheDocument();
  });

  it("displays contact email below client name", () => {
    const clients = [createClient()];
    render(<ClientsView initialData={createPaginatedData(clients)} />);

    const desktop = getDesktopTable();
    expect(within(desktop).getByText("info@acme.com")).toBeInTheDocument();
  });

  it("displays tier badges", () => {
    const clients = [createClient({ tier_name: "Tier 1" })];
    render(<ClientsView initialData={createPaginatedData(clients)} />);

    const desktop = getDesktopTable();
    expect(within(desktop).getByText("Tier 1")).toBeInTheDocument();
  });

  it("displays status badges", () => {
    const clients = [createClient({ status: "Active" })];
    render(<ClientsView initialData={createPaginatedData(clients)} />);

    const desktop = getDesktopTable();
    expect(within(desktop).getByText("Active")).toBeInTheDocument();
  });

  it("displays dash for missing tier", () => {
    const clients = [createClient({ tier_id: null, tier_name: null })];
    render(<ClientsView initialData={createPaginatedData(clients)} />);

    const desktop = getDesktopTable();
    const dashes = within(desktop).getAllByText("—");
    expect(dashes.length).toBeGreaterThanOrEqual(1);
  });

  it("renders search input", () => {
    const clients = [createClient()];
    render(<ClientsView initialData={createPaginatedData(clients)} />);

    expect(screen.getByPlaceholderText("Search clients…")).toBeInTheDocument();
  });

  it("renders status filter dropdown", () => {
    const clients = [createClient()];
    render(<ClientsView initialData={createPaginatedData(clients)} />);

    expect(screen.getByLabelText("Filter by status")).toBeInTheDocument();
  });

  it("renders tier filter dropdown", () => {
    const clients = [createClient()];
    render(<ClientsView initialData={createPaginatedData(clients)} />);

    expect(screen.getByLabelText("Filter by tier")).toBeInTheDocument();
  });

  it("filters by status when status dropdown changes", () => {
    const clients = [
      createClient({ client_id: "c1", name: "Active Client", status: "Active" }),
      createClient({ client_id: "c2", name: "Inactive Client", status: "Inactive" }),
    ];
    render(<ClientsView initialData={createPaginatedData(clients)} />);

    const statusSelect = screen.getByLabelText("Filter by status");
    fireEvent.change(statusSelect, { target: { value: "Active" } });

    const desktop = getDesktopTable();
    expect(within(desktop).getByText("Active Client")).toBeInTheDocument();
    expect(within(desktop).queryByText("Inactive Client")).not.toBeInTheDocument();
  });

  it("filters by tier when tier dropdown changes", () => {
    const clients = [
      createClient({ client_id: "c1", name: "Tier 1 Client", tier_name: "Tier 1" }),
      createClient({ client_id: "c2", name: "Tier 2 Client", tier_name: "Tier 2" }),
    ];
    render(<ClientsView initialData={createPaginatedData(clients)} />);

    const tierSelect = screen.getByLabelText("Filter by tier");
    fireEvent.change(tierSelect, { target: { value: "1" } });

    const desktop = getDesktopTable();
    expect(within(desktop).getByText("Tier 1 Client")).toBeInTheDocument();
    expect(within(desktop).queryByText("Tier 2 Client")).not.toBeInTheDocument();
  });

  it("renders mobile card view", () => {
    const clients = [createClient()];
    render(<ClientsView initialData={createPaginatedData(clients)} />);

    const mobileContainer = document.querySelector(".md\\:hidden") as HTMLElement;
    expect(mobileContainer).toBeInTheDocument();
    expect(within(mobileContainer).getByText("Acme Corp")).toBeInTheDocument();
  });

  it("displays industry tag in table", () => {
    const clients = [createClient({ industry_tag: "Technology" })];
    render(<ClientsView initialData={createPaginatedData(clients)} />);

    const desktop = getDesktopTable();
    expect(within(desktop).getByText("Technology")).toBeInTheDocument();
  });

  it("shows no results message when filters match nothing", () => {
    const clients = [createClient({ status: "Active" })];
    render(<ClientsView initialData={createPaginatedData(clients)} />);

    const statusSelect = screen.getByLabelText("Filter by status");
    fireEvent.change(statusSelect, { target: { value: "Suspended" } });

    const messages = screen.getAllByText("No clients match your filters.");
    expect(messages.length).toBeGreaterThanOrEqual(1);
  });

  it("renders column headers in desktop view", () => {
    const clients = [createClient()];
    render(<ClientsView initialData={createPaginatedData(clients)} />);

    const desktop = getDesktopTable();
    expect(within(desktop).getByText("Client / Contact")).toBeInTheDocument();
    expect(within(desktop).getByText("Tier")).toBeInTheDocument();
    expect(within(desktop).getByText("Status")).toBeInTheDocument();
    expect(within(desktop).getByText("Industry")).toBeInTheDocument();
    expect(within(desktop).getByText("Last Activity")).toBeInTheDocument();
  });
});
