import { getTemplates, type CommunicationTemplateDto } from "@/server/Services/templateService";
import { getClients } from "@/server/Services/clientService";
import type { ClientOption } from "./shared";
import SendTemplateView from "./SendTemplateView";

export default async function SendTemplatePage() {
  let templates: CommunicationTemplateDto[];
  let clients: ClientOption[];

  try {
    const [templateData, clientData] = await Promise.all([
      getTemplates(),
      getClients(),
    ]);
    templates = templateData.filter((t) => t.is_active);
    clients = clientData.items.map((c) => ({
      client_id: c.client_id,
      name: c.name,
      contact_email: c.contact_email,
    }));
  } catch (error) {
    console.error("[SendTemplatePage] Failed to fetch data:", error);
    templates = [];
    clients = [];
  }

  return (
    <SendTemplateView initialTemplates={templates} initialClients={clients} />
  );
}
