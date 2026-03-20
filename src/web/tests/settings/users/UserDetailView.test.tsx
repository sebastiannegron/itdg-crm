import { render, screen, fireEvent } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import type { UserDto } from "@/server/Services/userService";

vi.mock("next-intl", () => ({
  useLocale: () => "en-pr",
}));

vi.mock("@/i18n/routing", () => ({
  useRouter: () => ({
    push: vi.fn(),
  }),
  usePathname: () => "/settings/users/user-1-uuid",
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

vi.mock("@/app/[locale]/(admin)/settings/users/[user_id]/actions", () => ({
  updateUserAction: vi.fn().mockResolvedValue({
    success: true,
    message: "User updated successfully",
  }),
  getUserAction: vi.fn().mockResolvedValue({
    success: true,
    data: {
      user_id: "user-1-uuid",
      entra_object_id: "entra-1",
      email: "john@example.com",
      display_name: "John Doe",
      role: "Administrator",
      is_active: true,
      created_at: "2025-01-01T00:00:00Z",
      updated_at: "2025-01-01T00:00:00Z",
    },
  }),
}));

import UserDetailView from "@/app/[locale]/(admin)/settings/users/[user_id]/UserDetailView";

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

describe("UserDetailView", () => {
  it("renders user name as page title", () => {
    render(<UserDetailView user={createUser()} />);
    expect(
      screen.getByRole("heading", { level: 1 }),
    ).toHaveTextContent("John Doe");
  });

  it("renders breadcrumbs with Dashboard, Settings, Users, and user name", () => {
    render(<UserDetailView user={createUser()} />);
    expect(screen.getByText("Dashboard")).toBeInTheDocument();
    expect(screen.getByText("Settings")).toBeInTheDocument();
    expect(screen.getByText("Users")).toBeInTheDocument();
  });

  it("renders user email", () => {
    render(<UserDetailView user={createUser()} />);
    expect(screen.getByText("john@example.com")).toBeInTheDocument();
  });

  it("renders user role badge", () => {
    render(<UserDetailView user={createUser({ role: "Associate" })} />);
    expect(screen.getByText("Associate")).toBeInTheDocument();
  });

  it("renders user status badge as Active", () => {
    render(<UserDetailView user={createUser({ is_active: true })} />);
    expect(screen.getByText("Active")).toBeInTheDocument();
  });

  it("renders user status badge as Inactive", () => {
    render(<UserDetailView user={createUser({ is_active: false })} />);
    expect(screen.getByText("Inactive")).toBeInTheDocument();
  });

  it("renders Edit User button", () => {
    render(<UserDetailView user={createUser()} />);
    expect(
      screen.getByRole("button", { name: "Edit User" }),
    ).toBeInTheDocument();
  });

  it("renders Back to users link", () => {
    render(<UserDetailView user={createUser()} />);
    const backLink = screen.getByRole("link", { name: /Back to users/i });
    expect(backLink).toHaveAttribute("href", "/settings/users");
  });

  it("shows edit form when Edit User button is clicked", () => {
    render(<UserDetailView user={createUser()} />);
    const editButton = screen.getByRole("button", { name: "Edit User" });
    fireEvent.click(editButton);
    expect(screen.getByLabelText("Role")).toBeInTheDocument();
    expect(screen.getByLabelText("Status")).toBeInTheDocument();
  });

  it("shows Save and Cancel buttons in edit mode", () => {
    render(<UserDetailView user={createUser()} />);
    fireEvent.click(screen.getByRole("button", { name: "Edit User" }));
    expect(
      screen.getByRole("button", { name: "Save" }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: "Cancel" }),
    ).toBeInTheDocument();
  });

  it("exits edit mode when Cancel is clicked", () => {
    render(<UserDetailView user={createUser()} />);
    fireEvent.click(screen.getByRole("button", { name: "Edit User" }));
    expect(screen.getByLabelText("Role")).toBeInTheDocument();

    fireEvent.click(screen.getByRole("button", { name: "Cancel" }));
    expect(screen.queryByLabelText("Role")).not.toBeInTheDocument();
  });

  it("renders not found state when user is null", () => {
    render(<UserDetailView user={null} />);
    expect(screen.getByText("User not found.")).toBeInTheDocument();
  });

  it("renders last login field", () => {
    render(<UserDetailView user={createUser()} />);
    expect(screen.getByText("Last Login")).toBeInTheDocument();
    expect(screen.getByText("Never")).toBeInTheDocument();
  });
});
