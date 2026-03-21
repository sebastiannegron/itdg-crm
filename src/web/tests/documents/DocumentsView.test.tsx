import { render, screen, fireEvent, within } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import type { ClientDto } from "@/server/Services/clientService";
import type { DocumentCategoryDto } from "@/server/Services/documentCategoryService";

vi.mock("next-intl", () => ({
  useLocale: () => "en-pr",
}));

vi.mock("@/i18n/routing", () => ({
  useRouter: () => ({
    push: vi.fn(),
  }),
  usePathname: () => "/documents",
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
  "@/app/[locale]/(admin)/documents/actions",
  () => ({
    fetchClientDocuments: vi.fn().mockResolvedValue({
      success: true,
      message: "OK",
      data: { items: [], total_count: 0, page: 1, page_size: 50 },
    }),
    fetchDocumentDownload: vi.fn().mockResolvedValue({
      success: true,
      message: "OK",
      data: {
        document_id: "d1-uuid",
        file_name: "test.pdf",
        mime_type: "application/pdf",
        file_size: 1024,
        google_drive_file_id: "gd-123",
        web_view_link: "https://drive.google.com/file/d/123",
      },
    }),
  }),
);

import DocumentsView from "@/app/[locale]/(admin)/documents/DocumentsView";

function createClient(overrides: Partial<ClientDto> = {}): ClientDto {
  return {
    client_id: "c1-uuid",
    name: "Rodriguez & Associates LLC",
    contact_email: "carlos@rodassoc.com",
    phone: "787-555-0101",
    address: "123 Main St",
    tier_id: "t1-uuid",
    tier_name: "Tier 1",
    status: "Active",
    industry_tag: "Legal",
    notes: null,
    custom_fields: null,
    created_at: "2024-01-15T10:00:00Z",
    updated_at: "2024-06-01T14:30:00Z",
    ...overrides,
  };
}

function createCategory(
  overrides: Partial<DocumentCategoryDto> = {},
): DocumentCategoryDto {
  return {
    category_id: "cat1-uuid",
    name: "Bank Statements",
    naming_convention: null,
    is_default: false,
    sort_order: 1,
    created_at: "2024-01-01T00:00:00Z",
    updated_at: "2024-01-01T00:00:00Z",
    ...overrides,
  };
}

describe("DocumentsView", () => {
  it("renders the page header with Documents title", () => {
    render(<DocumentsView initialClients={[]} initialCategories={[]} />);
    expect(screen.getByRole("heading", { level: 1 })).toHaveTextContent(
      "Documents",
    );
  });

  it("renders breadcrumbs with Dashboard and Documents", () => {
    render(<DocumentsView initialClients={[]} initialCategories={[]} />);
    expect(screen.getByText("Dashboard")).toBeInTheDocument();
    expect(
      screen.getAllByText("Documents").length,
    ).toBeGreaterThanOrEqual(1);
  });

  it("renders explorer panel with client names", () => {
    const clients = [createClient()];
    const categories = [createCategory()];
    render(
      <DocumentsView
        initialClients={clients}
        initialCategories={categories}
      />,
    );

    expect(
      screen.getAllByText("Rodriguez & Associates LLC").length,
    ).toBeGreaterThanOrEqual(1);
  });

  it("renders select client message when no client is selected", () => {
    const clients = [createClient()];
    const categories = [createCategory()];
    render(
      <DocumentsView
        initialClients={clients}
        initialCategories={categories}
      />,
    );

    expect(
      screen.getAllByText("Select a client to view documents").length,
    ).toBeGreaterThanOrEqual(1);
  });

  it("renders mobile tab toggle buttons", () => {
    const clients = [createClient()];
    const categories = [createCategory()];
    render(
      <DocumentsView
        initialClients={clients}
        initialCategories={categories}
      />,
    );

    expect(screen.getAllByText("Explorer").length).toBeGreaterThanOrEqual(1);
    expect(screen.getAllByText("Files").length).toBeGreaterThanOrEqual(1);
  });

  it("expands tree node when clicking a client", () => {
    const clients = [createClient()];
    const categories = [createCategory()];
    render(
      <DocumentsView
        initialClients={clients}
        initialCategories={categories}
      />,
    );

    const clientButtons = screen.getAllByText("Rodriguez & Associates LLC");
    fireEvent.click(clientButtons[0]);

    // After expanding, should see year nodes
    const currentYear = new Date().getFullYear();
    expect(
      screen.getAllByText(String(currentYear)).length,
    ).toBeGreaterThanOrEqual(1);
  });

  it("shows category nodes when expanding a year", () => {
    const clients = [createClient()];
    const categories = [
      createCategory({ category_id: "cat1", name: "Bank Statements" }),
      createCategory({
        category_id: "cat2",
        name: "Tax Documents",
        sort_order: 2,
      }),
    ];
    render(
      <DocumentsView
        initialClients={clients}
        initialCategories={categories}
      />,
    );

    // Expand client
    const clientButtons = screen.getAllByText("Rodriguez & Associates LLC");
    fireEvent.click(clientButtons[0]);

    // Expand year
    const currentYear = String(new Date().getFullYear());
    const yearButtons = screen.getAllByText(currentYear);
    fireEvent.click(yearButtons[0]);

    // Should see categories
    expect(screen.getAllByText("Bank Statements").length).toBeGreaterThanOrEqual(1);
    expect(screen.getAllByText("Tax Documents").length).toBeGreaterThanOrEqual(1);
  });

  it("renders search input in files panel", () => {
    const clients = [createClient()];
    const categories = [createCategory()];
    render(
      <DocumentsView
        initialClients={clients}
        initialCategories={categories}
      />,
    );

    expect(
      screen.getAllByPlaceholderText("Search documents…").length,
    ).toBeGreaterThanOrEqual(1);
  });

  it("renders upload button", () => {
    const clients = [createClient()];
    const categories = [createCategory()];
    render(
      <DocumentsView
        initialClients={clients}
        initialCategories={categories}
      />,
    );

    expect(
      screen.getAllByText("Upload").length,
    ).toBeGreaterThanOrEqual(1);
  });

  it("renders Download All button", () => {
    const clients = [createClient()];
    const categories = [createCategory()];
    render(
      <DocumentsView
        initialClients={clients}
        initialCategories={categories}
      />,
    );

    expect(
      screen.getAllByText("Download All").length,
    ).toBeGreaterThanOrEqual(1);
  });

  it("shows empty state when no clients are provided", () => {
    render(<DocumentsView initialClients={[]} initialCategories={[]} />);

    expect(
      screen.getAllByText("Select a client to view documents").length,
    ).toBeGreaterThanOrEqual(1);
  });

  it("switches mobile tab when clicking Files tab", () => {
    const clients = [createClient()];
    const categories = [createCategory()];
    render(
      <DocumentsView
        initialClients={clients}
        initialCategories={categories}
      />,
    );

    const filesTab = screen.getByText("Files");
    fireEvent.click(filesTab);

    // Files tab should now be active (navy bg)
    expect(filesTab.closest("button")).toHaveClass("bg-slate-800");
  });

  it("renders explorer heading in sidebar", () => {
    const clients = [createClient()];
    const categories = [createCategory()];
    render(
      <DocumentsView
        initialClients={clients}
        initialCategories={categories}
      />,
    );

    expect(screen.getAllByText("Explorer").length).toBeGreaterThanOrEqual(1);
  });

  it("renders multiple clients in the tree", () => {
    const clients = [
      createClient({ client_id: "c1", name: "Client A" }),
      createClient({ client_id: "c2", name: "Client B" }),
    ];
    const categories = [createCategory()];
    render(
      <DocumentsView
        initialClients={clients}
        initialCategories={categories}
      />,
    );

    expect(screen.getAllByText("Client A").length).toBeGreaterThanOrEqual(1);
    expect(screen.getAllByText("Client B").length).toBeGreaterThanOrEqual(1);
  });
});
