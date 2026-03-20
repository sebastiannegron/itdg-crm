import { getClients } from "@/server/Services/clientService";
import ClientsView from "./ClientsView";

export default async function ClientsPage() {
  let clients;
  try {
    clients = await getClients({ page: 1, pageSize: 20 });
  } catch {
    clients = { items: [], total_count: 0, page: 1, page_size: 20 };
  }

  return <ClientsView initialData={clients} />;
}
