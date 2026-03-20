import { render, screen, fireEvent } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import type { PaginatedUsers, UserDto } from "@/server/Services/userService";

vi.mock("next-intl", () => ({
  useLocale: () => "en-pr",
}));

vi.mock("@/i18n/routing", () => ({
  useRouter: () => ({
    push: vi.fn(),
  }),
  usePathname: () => "/settings/users",
  Link: ({
    href,
    children,
    ...props
  }: {
    href: string;
    children: React.ReactNode;
  }) => (
    <a href={href} {...props}>
      {children}
    </a>
  ),
}));

vi.mock("@/app/[locale]/(admin)/settings/users/actions", () => ({
  getUsersAction: vi.fn().mockResolvedValue({
    success: true,
    data: { items: [], total_count: 0, page: 1, page_size: 100 },
  }),
  inviteUserAction: vi.fn().mockResolvedValue({
    success: true,
    message: "Invitation sent successfully",
  }),
}));

import UsersView from "@/app/[locale]/(admin)/settings/users/UsersView";

function createUser(overrides: Partial<UserDto> = {}): UserDto {
  return {
    user_id: "user-1-uuid",
    entra_object_id: "entra-1",
    email: "john@example.com",
    display_name: "John Doe",
    role: "Administrator",
    is_active: true,
    created_at: "2025-01-01T00:00:00Z",
    updated_at: "2025-01-01T00:00:00Z",
    ...overrides,
  };
}

const sampleUsers: PaginatedUsers = {
  items: [
    createUser({
      user_id: "user-1-uuid",
      display_name: "John Doe",
      email: "john@example.com",
      role: "Administrator",
      is_active: true,
    }),
    createUser({
      user_id: "user-2-uuid",
      display_name: "Jane Smith",
      email: "jane@example.com",
      role: "Associate",
      is_active: true,
    }),
    createUser({
      user_id: "user-3-uuid",
      display_name: "Bob Wilson",
      email: "bob@example.com",
      role: "ClientPortal",
      is_active: false,
    }),
  ],
  total_count: 3,
  page: 1,
  page_size: 100,
};

const emptyUsers: PaginatedUsers = {
  items: [],
  total_count: 0,
  page: 1,
  page_size: 100,
};

describe("UsersView", () => {
  it("renders the page header with Users title", () => {
    render(<UsersView initialUsers={sampleUsers} />);
    expect(
      screen.getByRole("heading", { level: 1 }),
    ).toHaveTextContent("Users");
  });

  it("renders breadcrumbs with Dashboard, Settings, and Users", () => {
    render(<UsersView initialUsers={sampleUsers} />);
    expect(screen.getByText("Dashboard")).toBeInTheDocument();
    expect(screen.getByText("Settings")).toBeInTheDocument();
  });

  it("renders all initial users", () => {
    render(<UsersView initialUsers={sampleUsers} />);
    expect(screen.getAllByText("John Doe").length).toBeGreaterThanOrEqual(1);
    expect(screen.getAllByText("Jane Smith").length).toBeGreaterThanOrEqual(1);
    expect(screen.getAllByText("Bob Wilson").length).toBeGreaterThanOrEqual(1);
  });

  it("renders user emails", () => {
    render(<UsersView initialUsers={sampleUsers} />);
    expect(screen.getAllByText("john@example.com").length).toBeGreaterThanOrEqual(1);
    expect(screen.getAllByText("jane@example.com").length).toBeGreaterThanOrEqual(1);
  });

  it("renders role badges for users", () => {
    render(<UsersView initialUsers={sampleUsers} />);
    expect(screen.getAllByText("Administrator").length).toBeGreaterThanOrEqual(1);
    expect(screen.getAllByText("Associate").length).toBeGreaterThanOrEqual(1);
    expect(screen.getAllByText("Client Portal").length).toBeGreaterThanOrEqual(1);
  });

  it("renders status badges for users", () => {
    render(<UsersView initialUsers={sampleUsers} />);
    expect(screen.getAllByText("Active").length).toBeGreaterThanOrEqual(1);
    expect(screen.getAllByText("Inactive").length).toBeGreaterThanOrEqual(1);
  });

  it("renders Invite User button", () => {
    render(<UsersView initialUsers={sampleUsers} />);
    expect(
      screen.getByText("Invite User"),
    ).toBeInTheDocument();
  });

  it("renders search input", () => {
    render(<UsersView initialUsers={sampleUsers} />);
    expect(
      screen.getByPlaceholderText("Search users…"),
    ).toBeInTheDocument();
  });

  it("renders filter dropdowns", () => {
    render(<UsersView initialUsers={sampleUsers} />);
    expect(
      screen.getByLabelText("Filter by role"),
    ).toBeInTheDocument();
    expect(
      screen.getByLabelText("Filter by status"),
    ).toBeInTheDocument();
  });

  it("filters users by search term", () => {
    render(<UsersView initialUsers={sampleUsers} />);
    const searchInput = screen.getByPlaceholderText("Search users…");
    fireEvent.change(searchInput, { target: { value: "John" } });
    expect(screen.getAllByText("John Doe").length).toBeGreaterThanOrEqual(1);
    expect(screen.queryByText("Jane Smith")).not.toBeInTheDocument();
  });

  it("filters users by role", () => {
    render(<UsersView initialUsers={sampleUsers} />);
    const roleSelect = screen.getByLabelText("Filter by role");
    fireEvent.change(roleSelect, { target: { value: "Administrator" } });
    expect(screen.getAllByText("John Doe").length).toBeGreaterThanOrEqual(1);
    expect(screen.queryByText("Jane Smith")).not.toBeInTheDocument();
  });

  it("filters users by status", () => {
    render(<UsersView initialUsers={sampleUsers} />);
    const statusSelect = screen.getByLabelText("Filter by status");
    fireEvent.change(statusSelect, { target: { value: "inactive" } });
    expect(screen.getAllByText("Bob Wilson").length).toBeGreaterThanOrEqual(1);
    expect(screen.queryByText("John Doe")).not.toBeInTheDocument();
  });

  it("renders empty state when no users exist", () => {
    render(<UsersView initialUsers={emptyUsers} />);
    expect(
      screen.getByText("No users yet"),
    ).toBeInTheDocument();
    expect(
      screen.getByText("Users will appear here once they are invited."),
    ).toBeInTheDocument();
  });

  it("shows no results message when filters match nothing", () => {
    render(<UsersView initialUsers={sampleUsers} />);
    const searchInput = screen.getByPlaceholderText("Search users…");
    fireEvent.change(searchInput, { target: { value: "nonexistent" } });
    expect(
      screen.getByText("No users match your filters."),
    ).toBeInTheDocument();
  });

  it("renders user links to detail page", () => {
    render(<UsersView initialUsers={sampleUsers} />);
    const link = screen.getByRole("link", { name: "John Doe" });
    expect(link).toHaveAttribute("href", "/settings/users/user-1-uuid");
  });

  it("renders table headers", () => {
    render(<UsersView initialUsers={sampleUsers} />);
    expect(screen.getByText("Name")).toBeInTheDocument();
    expect(screen.getByText("Email")).toBeInTheDocument();
    expect(screen.getByText("Role")).toBeInTheDocument();
    expect(screen.getByText("Last Login")).toBeInTheDocument();
  });
});
