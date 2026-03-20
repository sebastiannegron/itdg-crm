import { getClientById, getClientAssignments, type ClientAssignmentDto } from "@/server/Services/clientService";
import { getUsers } from "@/server/Services/userService";
import ClientDetailView from "./ClientDetailView";
import type { AssociateOption } from "./ClientAssignmentsPanel";

interface ClientDetailPageProps {
  params: Promise<{ client_id: string }>;
}

export default async function ClientDetailPage({
  params,
}: ClientDetailPageProps) {
  const { client_id } = await params;
  let client;
  let assignments: ClientAssignmentDto[] = [];
  let users: AssociateOption[] = [];

  try {
    client = await getClientById(client_id);
  } catch (error) {
    console.error("[ClientDetailPage] Failed to fetch client:", error);
    client = null;
  }

  try {
    assignments = client ? await getClientAssignments(client_id) : [];
  } catch (error) {
    console.error("[ClientDetailPage] Failed to fetch assignments:", error);
    assignments = [];
  }

  try {
    const result = await getUsers();
    users = result.items.map((u) => ({
      user_id: u.user_id,
      display_name: u.display_name,
      email: u.email,
    }));
  } catch (error) {
    console.error("[ClientDetailPage] Failed to fetch users:", error);
    users = [];
  }

  return (
    <ClientDetailView
      client={client}
      assignments={assignments}
      users={users}
    />
  );
}
