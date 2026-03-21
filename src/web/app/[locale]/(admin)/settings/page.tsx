import { getTiers, type ClientTierDto } from "@/server/Services/tierService";
import {
  getDocumentCategories,
  type DocumentCategoryDto,
} from "@/server/Services/documentCategoryService";
import {
  getGoogleConnectionStatus,
  type GoogleConnectionStatusDto,
} from "@/server/Services/integrationService";
import SettingsView from "./SettingsView";

export default async function SettingsPage() {
  let tiers: ClientTierDto[];
  let categories: DocumentCategoryDto[];
  let googleStatus: GoogleConnectionStatusDto;

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

  try {
    googleStatus = await getGoogleConnectionStatus();
  } catch (error) {
    console.error(
      "[SettingsPage] Failed to fetch Google connection status:",
      error,
    );
    googleStatus = { is_connected: false, connected_at: null };
  }

  return (
    <SettingsView
      initialTiers={tiers}
      initialCategories={categories}
      initialGoogleStatus={googleStatus}
    />
  );
}
