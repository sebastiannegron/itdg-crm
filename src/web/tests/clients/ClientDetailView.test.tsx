import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import type { ClientDto } from "@/app/[locale]/(admin)/clients/[client_id]/shared";

vi.mock("next-intl", () => ({
  useLocale: () => "en-pr",
}));

const mockPush = vi.fn();
vi.mock("@/i18n/routing", () => ({
  useRouter: () => ({
    push: mockPush,
  }),
  usePathname: () => "/clients/c1-uuid",
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

vi.mock(
  "@/app/[locale]/(admin)/clients/[client_id]/actions",
  () => ({
    updateClientAction: vi.fn().mockResolvedValue({
      success: true,
      message: "Client updated successfully",
    }),
    fetchClientDocumentsAction: vi.fn().mockResolvedValue({
      success: true,
      data: { items: [], total_count: 0, page: 1, page_size: 50 },
    }),
    fetchDocumentDetailAction: vi.fn().mockResolvedValue({
      success: true,
      data: null,
    }),
    uploadNewVersionAction: vi.fn().mockResolvedValue({
      success: true,
      message: "New version uploaded successfully",
    }),
    deleteDocumentAction: vi.fn().mockResolvedValue({
      success: true,
      message: "Document deleted successfully",
    }),
  }),
);

import ClientDetailView from "@/app/[locale]/(admin)/clients/[client_id]/ClientDetailView";

function createClient(overrides: Partial<ClientDto> = {}): ClientDto {
  return {
    client_id: "c1-uuid",
    name: "Acme Corp",
    contact_email: "info@acme.com",
    phone: "787-555-0100",
    address: "123 Main St, San Juan, PR",
    tier_id: "t1-uuid",
    tier_name: "Tier 1",
    status: "Active",
    industry_tag: "Technology",
    notes: "Important client",
    custom_fields: null,
    created_at: "2024-01-15T10:00:00Z",
    updated_at: "2024-06-01T14:30:00Z",
    ...overrides,
  };
}

describe("ClientDetailView", () => {
  beforeEach(() => {
    mockPush.mockClear();
  });

  it("renders empty state when client is null", () => {
    render(<ClientDetailView client={null} />);
    expect(screen.getByText("No clients yet")).toBeInTheDocument();
  });

  it("renders client name in page header", () => {
    render(<ClientDetailView client={createClient()} />);
    expect(
      screen.getByRole("heading", { level: 1 }),
    ).toHaveTextContent("Acme Corp");
  });

  it("renders breadcrumbs with Dashboard, Clients, and client name", () => {
    render(<ClientDetailView client={createClient()} />);
    expect(screen.getByText("Dashboard")).toBeInTheDocument();
    expect(screen.getByText("Clients")).toBeInTheDocument();
  });

  it("displays TierBadge in client header", () => {
    render(<ClientDetailView client={createClient({ tier_name: "Tier 1" })} />);
    expect(screen.getByText("Tier 1")).toBeInTheDocument();
  });

  it("displays StatusBadge in client header", () => {
    render(<ClientDetailView client={createClient({ status: "Active" })} />);
    expect(screen.getByText("Active")).toBeInTheDocument();
  });

  it("renders action buttons (View Emails, Documents, Tasks)", () => {
    render(<ClientDetailView client={createClient()} />);
    expect(screen.getByText("View Emails")).toBeInTheDocument();
    const documentsElements = screen.getAllByText("Documents");
    expect(documentsElements.length).toBeGreaterThanOrEqual(1);
    const tasksElements = screen.getAllByText("Tasks");
    expect(tasksElements.length).toBeGreaterThanOrEqual(1);
  });

  it("renders tabbed layout with Overview, Documents, Communications, Tasks", () => {
    render(<ClientDetailView client={createClient()} />);
    expect(screen.getByText("Overview")).toBeInTheDocument();
    // "Documents" appears both as action button and tab
    const documentsElements = screen.getAllByText("Documents");
    expect(documentsElements.length).toBeGreaterThanOrEqual(2);
    expect(screen.getByText("Communications")).toBeInTheDocument();
    // "Tasks" appears both as action button and tab
    const tasksElements = screen.getAllByText("Tasks");
    expect(tasksElements.length).toBeGreaterThanOrEqual(2);
  });

  it("shows overview tab content by default with contact info", () => {
    const client = createClient();
    render(<ClientDetailView client={client} />);
    expect(screen.getByText("info@acme.com")).toBeInTheDocument();
    expect(screen.getByText("787-555-0100")).toBeInTheDocument();
    expect(screen.getByText("Technology")).toBeInTheDocument();
    expect(
      screen.getByText("123 Main St, San Juan, PR"),
    ).toBeInTheDocument();
  });

  it("shows notes when present", () => {
    render(
      <ClientDetailView
        client={createClient({ notes: "Important client" })}
      />,
    );
    expect(screen.getByText("Important client")).toBeInTheDocument();
  });

  it("switches to Documents tab and shows documents section", async () => {
    render(<ClientDetailView client={createClient()} />);

    // Find the Documents tab button (not the action button)
    const tabs = screen.getAllByText("Documents");
    const tabButton = tabs.find(
      (el) => el.closest("nav[aria-label='Tabs']") !== null,
    );
    expect(tabButton).toBeDefined();
    fireEvent.click(tabButton!);

    await waitFor(() => {
      expect(
        screen.getByPlaceholderText("Search documents\u2026"),
      ).toBeInTheDocument();
    });
  });

  it("switches to Communications tab and shows placeholder", () => {
    render(<ClientDetailView client={createClient()} />);
    fireEvent.click(screen.getByText("Communications"));
    expect(
      screen.getByText("Communications will be available here."),
    ).toBeInTheDocument();
  });

  it("switches to Tasks tab and shows placeholder", () => {
    render(<ClientDetailView client={createClient()} />);

    const tabs = screen.getAllByText("Tasks");
    const tabButton = tabs.find(
      (el) => el.closest("nav[aria-label='Tabs']") !== null,
    );
    expect(tabButton).toBeDefined();
    fireEvent.click(tabButton!);

    expect(
      screen.getByText("Tasks will be available here."),
    ).toBeInTheDocument();
  });

  it("shows edit form when Edit Client button is clicked", () => {
    render(<ClientDetailView client={createClient()} />);
    fireEvent.click(screen.getByText("Edit Client"));

    expect(screen.getByLabelText(/Client \/ Contact/)).toBeInTheDocument();
    expect(screen.getByLabelText("Email")).toBeInTheDocument();
    expect(screen.getByLabelText("Phone")).toBeInTheDocument();
    expect(screen.getByLabelText(/Status/)).toBeInTheDocument();
  });

  it("pre-fills form with client data", () => {
    render(<ClientDetailView client={createClient()} />);
    fireEvent.click(screen.getByText("Edit Client"));

    expect(screen.getByLabelText(/Client \/ Contact/)).toHaveValue("Acme Corp");
    expect(screen.getByLabelText("Email")).toHaveValue("info@acme.com");
    expect(screen.getByLabelText("Phone")).toHaveValue("787-555-0100");
  });

  it("renders Back to clients button", () => {
    render(<ClientDetailView client={createClient()} />);
    const backElements = screen.getAllByText("Back to clients");
    expect(backElements.length).toBeGreaterThanOrEqual(1);
  });

  it("hides TierBadge when no tier", () => {
    render(
      <ClientDetailView
        client={createClient({ tier_id: null, tier_name: null })}
      />,
    );
    expect(screen.queryByText(/Tier \d/)).not.toBeInTheDocument();
  });

  it("shows dash for missing contact fields", () => {
    render(
      <ClientDetailView
        client={createClient({
          contact_email: null,
          phone: null,
          address: null,
          industry_tag: null,
          notes: null,
        })}
      />,
    );
    const dashes = screen.getAllByText("—");
    expect(dashes.length).toBeGreaterThanOrEqual(3);
  });
});
