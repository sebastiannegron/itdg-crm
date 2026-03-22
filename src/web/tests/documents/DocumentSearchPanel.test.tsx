import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
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

const mockFetchSearchDocuments = vi.fn();

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
      data: null,
    }),
    fetchSearchDocuments: (...args: unknown[]) =>
      mockFetchSearchDocuments(...args),
  }),
);

import DocumentSearchPanel from "@/app/[locale]/(admin)/documents/DocumentSearchPanel";

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

describe("DocumentSearchPanel", () => {
  const onClose = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("renders search panel with title and input", () => {
    render(
      <DocumentSearchPanel
        clients={[]}
        categories={[]}
        onClose={onClose}
      />,
    );

    expect(screen.getAllByText("Search Documents").length).toBeGreaterThanOrEqual(1);
    expect(
      screen.getByPlaceholderText("Search documents…"),
    ).toBeInTheDocument();
    expect(screen.getByText("Search", { selector: "button" })).toBeInTheDocument();
  });

  it("renders close button that calls onClose", () => {
    render(
      <DocumentSearchPanel
        clients={[createClient()]}
        categories={[createCategory()]}
        onClose={onClose}
      />,
    );

    const closeButton = screen.getByLabelText("Close search");
    fireEvent.click(closeButton);
    expect(onClose).toHaveBeenCalledTimes(1);
  });

  it("renders client filter dropdown with clients", () => {
    const clients = [
      createClient({ client_id: "c1", name: "Client A" }),
      createClient({ client_id: "c2", name: "Client B" }),
    ];
    render(
      <DocumentSearchPanel
        clients={clients}
        categories={[]}
        onClose={onClose}
      />,
    );

    const clientSelect = screen.getByLabelText("Client");
    expect(clientSelect).toBeInTheDocument();
    expect(screen.getByText("All Clients")).toBeInTheDocument();
    expect(screen.getByText("Client A")).toBeInTheDocument();
    expect(screen.getByText("Client B")).toBeInTheDocument();
  });

  it("renders category filter dropdown with categories", () => {
    const categories = [
      createCategory({ category_id: "cat1", name: "Bank Statements" }),
      createCategory({ category_id: "cat2", name: "Tax Documents" }),
    ];
    render(
      <DocumentSearchPanel
        clients={[]}
        categories={categories}
        onClose={onClose}
      />,
    );

    const categorySelect = screen.getByLabelText("Category");
    expect(categorySelect).toBeInTheDocument();
    expect(screen.getByText("All Categories")).toBeInTheDocument();
    expect(screen.getByText("Bank Statements")).toBeInTheDocument();
    expect(screen.getByText("Tax Documents")).toBeInTheDocument();
  });

  it("renders date range filters", () => {
    render(
      <DocumentSearchPanel
        clients={[]}
        categories={[]}
        onClose={onClose}
      />,
    );

    expect(screen.getByText("From")).toBeInTheDocument();
    expect(screen.getByText("To")).toBeInTheDocument();
  });

  it("shows empty state when no search has been performed", () => {
    render(
      <DocumentSearchPanel
        clients={[]}
        categories={[]}
        onClose={onClose}
      />,
    );

    expect(screen.getAllByText("Search Documents").length).toBeGreaterThanOrEqual(2);
  });

  it("disables search button when query is empty", () => {
    render(
      <DocumentSearchPanel
        clients={[]}
        categories={[]}
        onClose={onClose}
      />,
    );

    const searchButton = screen.getByText("Search", { selector: "button" }).closest("button");
    expect(searchButton).toBeDisabled();
  });

  it("enables search button when query has text", () => {
    render(
      <DocumentSearchPanel
        clients={[]}
        categories={[]}
        onClose={onClose}
      />,
    );

    const searchInput = screen.getByPlaceholderText("Search documents…");
    fireEvent.change(searchInput, { target: { value: "tax return" } });

    const searchButton = screen.getByText("Search", { selector: "button" }).closest("button");
    expect(searchButton).not.toBeDisabled();
  });

  it("calls fetchSearchDocuments when search button is clicked", async () => {
    mockFetchSearchDocuments.mockResolvedValue({
      success: true,
      message: "OK",
      data: {
        items: [
          {
            document_id: "d1-uuid",
            client_id: "c1-uuid",
            client_name: "Rodriguez & Associates",
            file_name: "tax-return-2024.pdf",
            category: "Tax Returns",
            uploaded_at: "2024-06-01T14:30:00Z",
            relevance_snippet: "...tax return <em>content</em>...",
          },
        ],
        total_count: 1,
        page: 1,
        page_size: 20,
      },
    });

    render(
      <DocumentSearchPanel
        clients={[]}
        categories={[]}
        onClose={onClose}
      />,
    );

    const searchInput = screen.getByPlaceholderText("Search documents…");
    fireEvent.change(searchInput, { target: { value: "tax return" } });

    const searchButton = screen.getByText("Search", { selector: "button" });
    fireEvent.click(searchButton);

    await waitFor(() => {
      expect(mockFetchSearchDocuments).toHaveBeenCalledWith({
        query: "tax return",
        clientId: undefined,
        category: undefined,
        dateFrom: undefined,
        dateTo: undefined,
        page: 1,
        pageSize: 20,
      });
    });
  });

  it("displays search results with document name, client, and category", async () => {
    mockFetchSearchDocuments.mockResolvedValue({
      success: true,
      message: "OK",
      data: {
        items: [
          {
            document_id: "d1-uuid",
            client_id: "c1-uuid",
            client_name: "Rodriguez & Associates",
            file_name: "tax-return-2024.pdf",
            category: "Tax Returns",
            uploaded_at: "2024-06-01T14:30:00Z",
            relevance_snippet: "relevant content snippet",
          },
        ],
        total_count: 1,
        page: 1,
        page_size: 20,
      },
    });

    render(
      <DocumentSearchPanel
        clients={[]}
        categories={[]}
        onClose={onClose}
      />,
    );

    const searchInput = screen.getByPlaceholderText("Search documents…");
    fireEvent.change(searchInput, { target: { value: "tax" } });
    fireEvent.click(screen.getByText("Search", { selector: "button" }));

    await waitFor(() => {
      expect(screen.getByText("tax-return-2024.pdf")).toBeInTheDocument();
      expect(screen.getByText("Rodriguez & Associates")).toBeInTheDocument();
      expect(screen.getByText("Tax Returns")).toBeInTheDocument();
      expect(screen.getByText("relevant content snippet")).toBeInTheDocument();
    });
  });

  it("shows no results message when search returns empty", async () => {
    mockFetchSearchDocuments.mockResolvedValue({
      success: true,
      message: "OK",
      data: {
        items: [],
        total_count: 0,
        page: 1,
        page_size: 20,
      },
    });

    render(
      <DocumentSearchPanel
        clients={[]}
        categories={[]}
        onClose={onClose}
      />,
    );

    const searchInput = screen.getByPlaceholderText("Search documents…");
    fireEvent.change(searchInput, { target: { value: "nonexistent" } });
    fireEvent.click(screen.getByText("Search", { selector: "button" }));

    await waitFor(() => {
      expect(
        screen.getByText("No documents match your search."),
      ).toBeInTheDocument();
    });
  });

  it("triggers search on Enter key press", async () => {
    mockFetchSearchDocuments.mockResolvedValue({
      success: true,
      message: "OK",
      data: {
        items: [],
        total_count: 0,
        page: 1,
        page_size: 20,
      },
    });

    render(
      <DocumentSearchPanel
        clients={[]}
        categories={[]}
        onClose={onClose}
      />,
    );

    const searchInput = screen.getByPlaceholderText("Search documents…");
    fireEvent.change(searchInput, { target: { value: "test query" } });
    fireEvent.keyDown(searchInput, { key: "Enter" });

    await waitFor(() => {
      expect(mockFetchSearchDocuments).toHaveBeenCalled();
    });
  });
});
