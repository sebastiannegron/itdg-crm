import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";

vi.mock("next-intl", () => ({
  useLocale: () => "en-pr",
}));

vi.mock("@/i18n/routing", () => ({
  useRouter: () => ({ push: vi.fn() }),
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

const mockFetchClientDocuments = vi.fn();
const mockFetchDocumentDetail = vi.fn();
const mockUploadNewVersion = vi.fn();
const mockDeleteDocument = vi.fn();

vi.mock("@/app/[locale]/(admin)/clients/[client_id]/actions", () => ({
  fetchClientDocumentsAction: (...args: unknown[]) =>
    mockFetchClientDocuments(...args),
  fetchDocumentDetailAction: (...args: unknown[]) =>
    mockFetchDocumentDetail(...args),
  uploadNewVersionAction: (...args: unknown[]) =>
    mockUploadNewVersion(...args),
  deleteDocumentAction: (...args: unknown[]) => mockDeleteDocument(...args),
}));

import ClientDocumentsTab from "@/app/[locale]/(admin)/clients/[client_id]/ClientDocumentsTab";

const mockDocuments = [
  {
    document_id: "doc-1",
    client_id: "c1-uuid",
    category_id: "cat-1",
    category_name: "Tax Returns",
    file_name: "2024-tax-return.pdf",
    google_drive_file_id: "gdrive-1",
    uploaded_by_id: "user-1",
    current_version: 2,
    file_size: 1048576,
    mime_type: "application/pdf",
    created_at: "2024-06-01T10:00:00Z",
    updated_at: "2024-07-15T14:30:00Z",
  },
  {
    document_id: "doc-2",
    client_id: "c1-uuid",
    category_id: "cat-2",
    category_name: "Contracts",
    file_name: "service-agreement.docx",
    google_drive_file_id: "gdrive-2",
    uploaded_by_id: "user-1",
    current_version: 1,
    file_size: 524288,
    mime_type: "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
    created_at: "2024-05-20T08:00:00Z",
    updated_at: "2024-05-20T08:00:00Z",
  },
];

const mockDocumentDetail = {
  document_id: "doc-1",
  client_id: "c1-uuid",
  category_id: "cat-1",
  category_name: "Tax Returns",
  file_name: "2024-tax-return.pdf",
  google_drive_file_id: "gdrive-1",
  uploaded_by_id: "user-1",
  current_version: 2,
  file_size: 1048576,
  mime_type: "application/pdf",
  created_at: "2024-06-01T10:00:00Z",
  updated_at: "2024-07-15T14:30:00Z",
  web_view_link: "https://drive.google.com/file/d/gdrive-1/view",
  versions: [
    {
      version_id: "ver-2",
      document_id: "doc-1",
      version_number: 2,
      google_drive_file_id: "gdrive-1-v2",
      uploaded_by_id: "user-1",
      uploaded_at: "2024-07-15T14:30:00Z",
    },
    {
      version_id: "ver-1",
      document_id: "doc-1",
      version_number: 1,
      google_drive_file_id: "gdrive-1-v1",
      uploaded_by_id: "user-2",
      uploaded_at: "2024-06-01T10:00:00Z",
    },
  ],
};

describe("ClientDocumentsTab", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockFetchClientDocuments.mockResolvedValue({
      success: true,
      data: {
        items: mockDocuments,
        total_count: 2,
        page: 1,
        page_size: 50,
      },
    });
    mockFetchDocumentDetail.mockResolvedValue({
      success: true,
      data: mockDocumentDetail,
    });
    mockUploadNewVersion.mockResolvedValue({
      success: true,
      message: "New version uploaded successfully",
    });
    mockDeleteDocument.mockResolvedValue({
      success: true,
      message: "Document deleted successfully",
    });
  });

  it("renders search input", async () => {
    render(<ClientDocumentsTab clientId="c1-uuid" />);
    await waitFor(() => {
      expect(
        screen.getByPlaceholderText("Search documents\u2026"),
      ).toBeInTheDocument();
    });
  });

  it("loads and displays documents", async () => {
    render(<ClientDocumentsTab clientId="c1-uuid" />);
    await waitFor(() => {
      expect(screen.getAllByText("2024-tax-return.pdf").length).toBeGreaterThan(0);
    });
    expect(screen.getAllByText("service-agreement.docx").length).toBeGreaterThan(0);
  });

  it("displays document count", async () => {
    render(<ClientDocumentsTab clientId="c1-uuid" />);
    await waitFor(() => {
      expect(screen.getByText("2 documents")).toBeInTheDocument();
    });
  });

  it("displays version badges", async () => {
    render(<ClientDocumentsTab clientId="c1-uuid" />);
    await waitFor(() => {
      expect(screen.getAllByText("v2").length).toBeGreaterThan(0);
    });
    expect(screen.getAllByText("v1").length).toBeGreaterThan(0);
  });

  it("shows empty state when no documents", async () => {
    mockFetchClientDocuments.mockResolvedValue({
      success: true,
      data: { items: [], total_count: 0, page: 1, page_size: 50 },
    });
    render(<ClientDocumentsTab clientId="c1-uuid" />);
    await waitFor(() => {
      expect(screen.getByText("No documents yet")).toBeInTheDocument();
    });
  });

  it("opens document detail dialog on document click", async () => {
    render(<ClientDocumentsTab clientId="c1-uuid" />);
    await waitFor(() => {
      expect(screen.getAllByText("2024-tax-return.pdf").length).toBeGreaterThan(0);
    });
    fireEvent.click(screen.getAllByText("2024-tax-return.pdf")[0]);
    await waitFor(() => {
      expect(screen.getByText("Document Details")).toBeInTheDocument();
    });
  });

  it("displays document metadata in detail dialog", async () => {
    render(<ClientDocumentsTab clientId="c1-uuid" />);
    await waitFor(() => {
      expect(screen.getAllByText("2024-tax-return.pdf").length).toBeGreaterThan(0);
    });
    fireEvent.click(screen.getAllByText("2024-tax-return.pdf")[0]);
    await waitFor(() => {
      expect(screen.getByText("Tax Returns")).toBeInTheDocument();
    });
    expect(screen.getByText("application/pdf")).toBeInTheDocument();
    expect(screen.getByText("1.0 MB")).toBeInTheDocument();
  });

  it("displays version history in detail dialog", async () => {
    render(<ClientDocumentsTab clientId="c1-uuid" />);
    await waitFor(() => {
      expect(screen.getAllByText("2024-tax-return.pdf").length).toBeGreaterThan(0);
    });
    fireEvent.click(screen.getAllByText("2024-tax-return.pdf")[0]);
    await waitFor(() => {
      expect(screen.getByText("Version History")).toBeInTheDocument();
    });
  });

  it("displays action buttons in detail dialog", async () => {
    render(<ClientDocumentsTab clientId="c1-uuid" />);
    await waitFor(() => {
      expect(screen.getAllByText("2024-tax-return.pdf").length).toBeGreaterThan(0);
    });
    fireEvent.click(screen.getAllByText("2024-tax-return.pdf")[0]);
    await waitFor(() => {
      expect(screen.getByText("View")).toBeInTheDocument();
    });
    expect(screen.getByText("Download")).toBeInTheDocument();
    expect(screen.getByText("Upload New Version")).toBeInTheDocument();
    expect(screen.getByText("Delete")).toBeInTheDocument();
  });

  it("marks latest version badge", async () => {
    render(<ClientDocumentsTab clientId="c1-uuid" />);
    await waitFor(() => {
      expect(screen.getAllByText("2024-tax-return.pdf").length).toBeGreaterThan(0);
    });
    fireEvent.click(screen.getAllByText("2024-tax-return.pdf")[0]);
    await waitFor(() => {
      expect(screen.getByText("(latest)")).toBeInTheDocument();
    });
  });

  it("fetches documents on mount with clientId", async () => {
    render(<ClientDocumentsTab clientId="c1-uuid" />);
    await waitFor(() => {
      expect(mockFetchClientDocuments).toHaveBeenCalledWith(
        expect.objectContaining({ clientId: "c1-uuid" }),
      );
    });
  });

  it("fetches document detail when document is clicked", async () => {
    render(<ClientDocumentsTab clientId="c1-uuid" />);
    await waitFor(() => {
      expect(screen.getAllByText("2024-tax-return.pdf").length).toBeGreaterThan(0);
    });
    fireEvent.click(screen.getAllByText("2024-tax-return.pdf")[0]);
    await waitFor(() => {
      expect(mockFetchDocumentDetail).toHaveBeenCalledWith("doc-1");
    });
  });

  it("shows no versions message when versions list is empty", async () => {
    mockFetchDocumentDetail.mockResolvedValue({
      success: true,
      data: { ...mockDocumentDetail, versions: [] },
    });
    render(<ClientDocumentsTab clientId="c1-uuid" />);
    await waitFor(() => {
      expect(screen.getAllByText("2024-tax-return.pdf").length).toBeGreaterThan(0);
    });
    fireEvent.click(screen.getAllByText("2024-tax-return.pdf")[0]);
    await waitFor(() => {
      expect(
        screen.getByText("No version history available."),
      ).toBeInTheDocument();
    });
  });
});
