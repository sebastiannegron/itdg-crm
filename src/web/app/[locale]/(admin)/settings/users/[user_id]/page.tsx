import { getUserById } from "@/server/Services/userService";
import UserDetailView from "./UserDetailView";

interface UserDetailPageProps {
  params: Promise<{ user_id: string }>;
}

export default async function UserDetailPage({
  params,
}: UserDetailPageProps) {
  const { user_id } = await params;
  let user;

  try {
    user = await getUserById(user_id);
  } catch (error) {
    console.error("[UserDetailPage] Failed to fetch user:", error);
    user = null;
  }

  return <UserDetailView user={user} />;
}
