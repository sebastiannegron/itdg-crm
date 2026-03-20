import { getTiers, type ClientTierDto } from "@/server/Services/tierService";
import SettingsView from "./SettingsView";

export default async function SettingsPage() {
  let tiers: ClientTierDto[];
  try {
    tiers = await getTiers();
  } catch (error) {
    console.error("[SettingsPage] Failed to fetch tiers:", error);
    tiers = [];
  }

  return <SettingsView initialTiers={tiers} />;
}
