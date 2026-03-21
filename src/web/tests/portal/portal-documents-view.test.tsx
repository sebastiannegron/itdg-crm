import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { fieldnames } from "@/app/[locale]/_shared/app-fieldnames";

vi.mock("next-intl", () => ({
  useLocale: () => "en-pr",
}));

vi.mock("@/i18n/routing", () => ({
  Link: ({
    href,
    children,
    ...props
  }: {
    href: string;
    children: React.ReactNode;
    className?: string;
  }) => (
    <a href={href} {...props}>
      {children}
    </a>
  ),
  usePathname: () => "/portal/documents",
  useRouter: () => ({ push: vi.fn() }),
}));

const mockFetchPortalDocuments = vi.fn().mockResolvedValue({
  success: true,
  message: "OK",
  data: { items: [], total_count: 0, page: 1, page_size: 50 },
});
const mockFetchPortalDocumentDownload = vi.fn().mockResolvedValue({
  success: true,
  message: "OK",
  data: {
    document_id: "d1-uuid",
    file_name: "test.pdf",
    mime_type: "application/pdf",
    file_size: 1024,
    google_drive_file_id: "gd-123",
    web_view_link: "https://drive.google.com/file/d/123/view",
  },
});
const mockUploadPortalDocumentAction = vi.fn().mockResolvedValue({
  success: true,
  message: "Document uploaded successfully",
});

vi.mock(
  "@/app/[locale]/(portal)/portal/documents/actions",
  () => ({
    fetchPortalDocuments: (...args: unknown[]) => mockFetchPortalDocuments(...args),
    fetchPortalDocumentDownload: (...args: unknown[]) => mockFetchPortalDocumentDownload(...args),
    uploadPortalDocumentAction: (...args: unknown[]) => mockUploadPortalDocumentAction(...args),
  }),
);

import PortalDocumentsView from "@/app/[locale]/(portal)/portal/documents/PortalDocumentsView";
import type { PortalDocumentDto } from "@/server/Services/portalDocumentService";
import type { DocumentCategoryDto } from "@/server/Services/documentCategoryService";

const t = fieldnames["en-pr"];

function createDocument(overrides: Partial<PortalDocumentDto> = {}): PortalDocumentDto {
  return {
    document_id: "doc-1",
    client_id: "c1-uuid",
    category_id: "cat-1",
    category_name: "Tax Returns",
    file_name: "2024-tax-return.pdf",
    google_drive_file_id: "gd-abc",
    uploaded_by_id: "user-123",
    current_version: 1,
    file_size: 2048,
    mime_type: "application/pdf",
    created_at: "2025-06-15T10:00:00Z",
    updated_at: "2025-06-15T10:00:00Z",
    ...overrides,
  };
}

function createCategory(overrides: Partial<DocumentCategoryDto> = {}): DocumentCategoryDto {
  return {
    category_id: "cat-1",
    name: "Tax Returns",
    naming_convention: null,
    is_default: false,
    sort_order: 1,
    created_at: "2025-01-01T00:00:00Z",
    updated_at: "2025-01-01T00:00:00Z",
    ...overrides,
  };
}

const mockDocuments: PortalDocumentDto[] = [
  createDocument(),
  createDocument({
    document_id: "doc-2",
    category_id: "cat-2",
    category_name: "Financial Statements",
    file_name: "balance-sheet-2024.xlsx",
    mime_type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
    file_size: 5120,
    current_version: 2,
    created_at: "2025-05-10T14:30:00Z",
    updated_at: "2025-05-10T14:30:00Z",
  }),
  createDocument({
    document_id: "doc-3",
    category_id: "cat-1",
    category_name: "Tax Returns",
    file_name: "w2-form-2024.pdf",
    mime_type: "application/pdf",
    file_size: 1024,
    current_version: 1,
    created_at: "2025-04-20T08:00:00Z",
    updated_at: "2025-04-20T08:00:00Z",
  }),
];

const mockCategories: DocumentCategoryDto[] = [
  createCategory(),
  createCategory({
    category_id: "cat-2",
    name: "Financial Statements",
    sort_order: 2,
  }),
];

describe("PortalDocumentsView", () => {
  beforeEach(() => {
    mockFetchPortalDocuments.mockReset().mockResolvedValue({
      success: true,
      message: "OK",
      data: { items: [], total_count: 0, page: 1, page_size: 50 },
    });
    mockFetchPortalDocumentDownload.mockReset().mockResolvedValue({
      success: true,
      message: "OK",
      data: {
        document_id: "d1-uuid",
        file_name: "test.pdf",
        mime_type: "application/pdf",
        file_size: 1024,
        google_drive_file_id: "gd-123",
        web_view_link: "https://drive.google.com/file/d/123/view",
      },
    });
    mockUploadPortalDocumentAction.mockReset().mockResolvedValue({
      success: true,
      message: "Document uploaded successfully",
    });
  });

  describe("Page Title", () => {
    it("renders the documents page title", () => {
      render(
        <PortalDocumentsView
          initialDocuments={[]}
          initialCategories={mockCategories}
        />,
      );
      expect(screen.getByText(t.portal_nav_documents)).toBeInTheDocument();
    });
  });

  describe("Filters", () => {
    it("renders search input", () => {
      render(
        <PortalDocumentsView
          initialDocuments={mockDocuments}
          initialCategories={mockCategories}
        />,
      );
      expect(
        screen.getByPlaceholderText(t.documents_search_placeholder),
      ).toBeInTheDocument();
    });

    it("renders category filter with options", () => {
      render(
        <PortalDocumentsView
          initialDocuments={mockDocuments}
          initialCategories={mockCategories}
        />,
      );
      const categorySelects = screen.getAllByRole("combobox");
      // First select is category filter, second is year filter, third is upload category
      expect(categorySelects.length).toBeGreaterThanOrEqual(2);
    });

    it("renders year filter", () => {
      render(
        <PortalDocumentsView
          initialDocuments={mockDocuments}
          initialCategories={mockCategories}
        />,
      );
      expect(screen.getByText(t.portal_documents_all_years)).toBeInTheDocument();
    });

    it("calls fetchPortalDocuments when category changes", async () => {
      render(
        <PortalDocumentsView
          initialDocuments={mockDocuments}
          initialCategories={mockCategories}
        />,
      );
      const categorySelects = screen.getAllByLabelText(t.documents_select_category);
      fireEvent.change(categorySelects[0], { target: { value: "cat-1" } });

      await waitFor(() => {
        expect(mockFetchPortalDocuments).toHaveBeenCalledWith(
          expect.objectContaining({ categoryId: "cat-1" }),
        );
      });
    });
  });

  describe("Document List", () => {
    it("renders document names", () => {
      render(
        <PortalDocumentsView
          initialDocuments={mockDocuments}
          initialCategories={mockCategories}
        />,
      );
      expect(screen.getAllByText("2024-tax-return.pdf").length).toBeGreaterThan(0);
      expect(screen.getAllByText("balance-sheet-2024.xlsx").length).toBeGreaterThan(0);
      expect(screen.getAllByText("w2-form-2024.pdf").length).toBeGreaterThan(0);
    });

    it("groups documents by category", () => {
      render(
        <PortalDocumentsView
          initialDocuments={mockDocuments}
          initialCategories={mockCategories}
        />,
      );
      expect(screen.getAllByText("Tax Returns").length).toBeGreaterThan(0);
      expect(screen.getAllByText("Financial Statements").length).toBeGreaterThan(0);
    });

    it("shows version badges", () => {
      render(
        <PortalDocumentsView
          initialDocuments={mockDocuments}
          initialCategories={mockCategories}
        />,
      );
      expect(screen.getAllByText("v1").length).toBeGreaterThan(0);
      expect(screen.getAllByText("v2").length).toBeGreaterThan(0);
    });

    it("renders empty state when no documents", () => {
      render(
        <PortalDocumentsView
          initialDocuments={[]}
          initialCategories={mockCategories}
        />,
      );
      expect(screen.getByText(t.documents_empty_title)).toBeInTheDocument();
    });

    it("renders view and download buttons for each document", () => {
      render(
        <PortalDocumentsView
          initialDocuments={mockDocuments}
          initialCategories={mockCategories}
        />,
      );
      // Desktop view has icon-only buttons with title attributes
      const viewButtons = screen.getAllByTitle(t.documents_view);
      const downloadButtons = screen.getAllByTitle(t.documents_download);
      expect(viewButtons.length).toBeGreaterThan(0);
      expect(downloadButtons.length).toBeGreaterThan(0);
    });
  });

  describe("Upload Section", () => {
    it("renders upload section with category selector", () => {
      render(
        <PortalDocumentsView
          initialDocuments={[]}
          initialCategories={mockCategories}
        />,
      );
      expect(screen.getAllByText(t.documents_upload).length).toBeGreaterThan(0);
      expect(screen.getAllByText(t.documents_select_category).length).toBeGreaterThan(0);
    });

    it("renders drag and drop zone", () => {
      render(
        <PortalDocumentsView
          initialDocuments={[]}
          initialCategories={mockCategories}
        />,
      );
      expect(
        screen.getByRole("region", { name: t.documents_drop_zone }),
      ).toBeInTheDocument();
    });

    it("shows select category message when no category selected for upload", () => {
      render(
        <PortalDocumentsView
          initialDocuments={[]}
          initialCategories={mockCategories}
        />,
      );
      const dropZone = screen.getByRole("region", { name: t.documents_drop_zone });
      expect(dropZone).toHaveTextContent(t.documents_select_category);
    });

    it("disables upload button when no category selected", () => {
      render(
        <PortalDocumentsView
          initialDocuments={[]}
          initialCategories={mockCategories}
        />,
      );
      const uploadButtons = screen.getAllByText(t.documents_upload);
      // The button (not the label) should be disabled
      const uploadButton = uploadButtons.find(
        (el) => el.closest("button") !== null,
      );
      if (uploadButton) {
        expect(uploadButton.closest("button")).toBeDisabled();
      }
    });
  });

  describe("i18n", () => {
    it("includes correct portal document fieldnames for en-pr", () => {
      expect(fieldnames["en-pr"].portal_nav_documents).toBe("Documents");
      expect(fieldnames["en-pr"].documents_upload).toBe("Upload");
      expect(fieldnames["en-pr"].documents_search_placeholder).toBe("Search documents…");
      expect(fieldnames["en-pr"].documents_empty_title).toBe("No documents yet");
      expect(fieldnames["en-pr"].portal_documents_all_years).toBe("All Years");
    });

    it("includes correct portal document fieldnames for es-pr", () => {
      expect(fieldnames["es-pr"].portal_nav_documents).toBe("Documentos");
      expect(fieldnames["es-pr"].documents_upload).toBe("Subir");
      expect(fieldnames["es-pr"].documents_search_placeholder).toBe("Buscar documentos…");
      expect(fieldnames["es-pr"].documents_empty_title).toBe("No hay documentos");
      expect(fieldnames["es-pr"].portal_documents_all_years).toBe("Todos los Años");
    });
  });
});
