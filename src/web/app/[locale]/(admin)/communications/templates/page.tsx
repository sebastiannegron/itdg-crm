import {
  getTemplates,
  type CommunicationTemplateDto,
} from "@/server/Services/templateService";
import TemplatesView from "./TemplatesView";

export default async function TemplatesPage() {
  let templates: CommunicationTemplateDto[];
  try {
    templates = await getTemplates();
  } catch (error) {
    console.error("[TemplatesPage] Failed to fetch templates:", error);
    templates = [];
  }

  return <TemplatesView initialTemplates={templates} />;
}
