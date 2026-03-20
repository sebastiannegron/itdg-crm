import { getClientById } from "@/server/Services/clientService";
import ClientDetailView from "./ClientDetailView";

interface ClientDetailPageProps {
  params: Promise<{ client_id: string }>;
}

export default async function ClientDetailPage({
  params,
}: ClientDetailPageProps) {
  const { client_id } = await params;
  let client;
  try {
    client = await getClientById(client_id);
  } catch (error) {
    console.error("[ClientDetailPage] Failed to fetch client:", error);
    client = null;
  }

  return <ClientDetailView client={client} />;
}
