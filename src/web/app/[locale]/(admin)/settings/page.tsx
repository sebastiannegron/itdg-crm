import { getTiers, type ClientTierDto } from "@/server/Services/tierService";
import {
  getDocumentCategories,
  type DocumentCategoryDto,
} from "@/server/Services/documentCategoryService";
import SettingsView from "./SettingsView";

export default async function SettingsPage() {
  let tiers: ClientTierDto[];
  let categories: DocumentCategoryDto[];

  try {
    tiers = await getTiers();
  } catch (error) {
    console.error("[SettingsPage] Failed to fetch tiers:", error);
    tiers = [];
  }

  try {
    categories = await getDocumentCategories();
  } catch (error) {
    console.error("[SettingsPage] Failed to fetch document categories:", error);
    categories = [];
  }

  return <SettingsView initialTiers={tiers} initialCategories={categories} />;
}
