import {
  getPortalDocuments,
  type PortalDocumentDto,
} from "@/server/Services/portalDocumentService";
import {
  getDocumentCategories,
  type DocumentCategoryDto,
} from "@/server/Services/documentCategoryService";
import PortalDocumentsView from "./PortalDocumentsView";

export default async function PortalDocumentsPage() {
  let documents: PortalDocumentDto[];
  let categories: DocumentCategoryDto[];

  try {
    const result = await getPortalDocuments({ page: 1, pageSize: 50 });
    documents = result.items;
  } catch (error) {
    console.error("[PortalDocumentsPage] Failed to fetch documents:", error);
    documents = [];
  }

  try {
    categories = await getDocumentCategories();
  } catch (error) {
    console.error("[PortalDocumentsPage] Failed to fetch categories:", error);
    categories = [];
  }

  return (
    <PortalDocumentsView
      initialDocuments={documents}
      initialCategories={categories}
    />
  );
}
