import {
  getAllUsers,
  type PaginatedUsers,
} from "@/server/Services/userService";
import UsersView from "./UsersView";

export default async function UsersPage() {
  let users: PaginatedUsers;
  try {
    users = await getAllUsers();
  } catch (error) {
    console.error("[UsersPage] Failed to fetch users:", error);
    users = { items: [], total_count: 0, page: 1, page_size: 100 };
  }

  return <UsersView initialUsers={users} />;
}
