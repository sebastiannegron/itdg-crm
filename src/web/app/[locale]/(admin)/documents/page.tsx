import { getClients, type ClientDto } from "@/server/Services/clientService";
import {
  getDocumentCategories,
  type DocumentCategoryDto,
} from "@/server/Services/documentCategoryService";
import DocumentsView from "./DocumentsView";

export default async function DocumentsPage() {
  let clients: ClientDto[];
  let categories: DocumentCategoryDto[];

  try {
    const result = await getClients({ page: 1, pageSize: 100 });
    clients = result.items;
  } catch (error) {
    console.error("[DocumentsPage] Failed to fetch clients:", error);
    clients = [];
  }

  try {
    categories = await getDocumentCategories();
  } catch (error) {
    console.error("[DocumentsPage] Failed to fetch categories:", error);
    categories = [];
  }

  return (
    <DocumentsView initialClients={clients} initialCategories={categories} />
  );
}
