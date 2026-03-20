import { render, screen, fireEvent } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";

vi.mock("next-intl", () => ({
  useLocale: () => "en-pr",
}));

vi.mock("@/i18n/routing", () => ({
  useRouter: () => ({
    push: vi.fn(),
  }),
  usePathname: () => "/clients/c1-uuid",
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

vi.mock(
  "@/app/[locale]/(admin)/clients/[client_id]/actions",
  () => ({
    updateClientAction: vi.fn().mockResolvedValue({
      success: true,
      message: "Client updated successfully",
    }),
    assignClientAction: vi.fn().mockResolvedValue({
      success: true,
      message: "Associate assigned successfully",
    }),
    unassignClientAction: vi.fn().mockResolvedValue({
      success: true,
      message: "Associate removed successfully",
    }),
  }),
);

import { assignClientAction, unassignClientAction } from "@/app/[locale]/(admin)/clients/[client_id]/actions";
import ClientAssignmentsPanel from "@/app/[locale]/(admin)/clients/[client_id]/ClientAssignmentsPanel";
import type { ClientAssignmentDto } from "@/app/[locale]/(admin)/clients/[client_id]/shared";
import type { AssociateOption } from "@/app/[locale]/(admin)/clients/[client_id]/ClientAssignmentsPanel";

function createAssignment(
  overrides: Partial<ClientAssignmentDto> = {},
): ClientAssignmentDto {
  return {
    user_id: "u1-uuid",
    display_name: "Jane Doe",
    email: "jane@example.com",
    assigned_at: "2024-01-15T10:00:00Z",
    ...overrides,
  };
}

function createUser(
  overrides: Partial<AssociateOption> = {},
): AssociateOption {
  return {
    user_id: "u2-uuid",
    display_name: "John Smith",
    email: "john@example.com",
    ...overrides,
  };
}

describe("ClientAssignmentsPanel", () => {
  const mockAssign = vi.mocked(assignClientAction);
  const mockUnassign = vi.mocked(unassignClientAction);

  beforeEach(() => {
    mockAssign.mockClear();
    mockUnassign.mockClear();
  });

  it("renders the panel title", () => {
    render(
      <ClientAssignmentsPanel
        clientId="c1-uuid"
        initialAssignments={[]}
        users={[]}
      />,
    );
    expect(screen.getByText("Assigned Associates")).toBeInTheDocument();
  });

  it("shows empty message when no assignments", () => {
    render(
      <ClientAssignmentsPanel
        clientId="c1-uuid"
        initialAssignments={[]}
        users={[]}
      />,
    );
    expect(
      screen.getByText("No associates assigned to this client."),
    ).toBeInTheDocument();
  });

  it("renders assigned associates list", () => {
    const assignments = [
      createAssignment(),
      createAssignment({
        user_id: "u2-uuid",
        display_name: "Bob Builder",
        email: "bob@example.com",
      }),
    ];

    render(
      <ClientAssignmentsPanel
        clientId="c1-uuid"
        initialAssignments={assignments}
        users={[]}
      />,
    );

    expect(screen.getByText("Jane Doe")).toBeInTheDocument();
    expect(screen.getByText("jane@example.com")).toBeInTheDocument();
    expect(screen.getByText("Bob Builder")).toBeInTheDocument();
    expect(screen.getByText("bob@example.com")).toBeInTheDocument();
  });

  it("shows dropdown with available users (excluding assigned)", () => {
    const assignments = [createAssignment({ user_id: "u1-uuid" })];
    const users = [
      createUser({ user_id: "u1-uuid", display_name: "Jane Doe", email: "jane@example.com" }),
      createUser({ user_id: "u2-uuid", display_name: "John Smith", email: "john@example.com" }),
    ];

    render(
      <ClientAssignmentsPanel
        clientId="c1-uuid"
        initialAssignments={assignments}
        users={users}
      />,
    );

    const select = screen.getByLabelText("Select an associate");
    expect(select).toBeInTheDocument();
    // u1 should not be in the dropdown since already assigned
    expect(screen.queryByText("Jane Doe (jane@example.com)")).not.toBeInTheDocument();
    // u2 should be in the dropdown
    expect(screen.getByText("John Smith (john@example.com)")).toBeInTheDocument();
  });

  it("renders assign button disabled when no user selected", () => {
    const users = [createUser()];

    render(
      <ClientAssignmentsPanel
        clientId="c1-uuid"
        initialAssignments={[]}
        users={users}
      />,
    );

    const assignButton = screen.getByText("Assign");
    expect(assignButton).toBeDisabled();
  });

  it("renders remove button for each assignment", () => {
    const assignments = [createAssignment()];

    render(
      <ClientAssignmentsPanel
        clientId="c1-uuid"
        initialAssignments={assignments}
        users={[]}
      />,
    );

    const removeButton = screen.getByLabelText("Remove Jane Doe");
    expect(removeButton).toBeInTheDocument();
  });

  it("does not show dropdown when all users are assigned", () => {
    const assignments = [createAssignment({ user_id: "u1-uuid" })];
    const users = [
      createUser({ user_id: "u1-uuid", display_name: "Jane Doe", email: "jane@example.com" }),
    ];

    render(
      <ClientAssignmentsPanel
        clientId="c1-uuid"
        initialAssignments={assignments}
        users={users}
      />,
    );

    expect(screen.queryByLabelText("Select an associate")).not.toBeInTheDocument();
  });

  it("calls assignClientAction when assign button is clicked", async () => {
    const users = [createUser({ user_id: "u2-uuid" })];

    render(
      <ClientAssignmentsPanel
        clientId="c1-uuid"
        initialAssignments={[]}
        users={users}
      />,
    );

    const select = screen.getByLabelText("Select an associate");
    fireEvent.change(select, { target: { value: "u2-uuid" } });

    const assignButton = screen.getByText("Assign");
    fireEvent.click(assignButton);

    expect(mockAssign).toHaveBeenCalledWith("c1-uuid", "u2-uuid");
  });

  it("calls unassignClientAction when remove button is clicked", () => {
    const assignments = [createAssignment({ user_id: "u1-uuid" })];

    render(
      <ClientAssignmentsPanel
        clientId="c1-uuid"
        initialAssignments={assignments}
        users={[]}
      />,
    );

    const removeButton = screen.getByLabelText("Remove Jane Doe");
    fireEvent.click(removeButton);

    expect(mockUnassign).toHaveBeenCalledWith("c1-uuid", "u1-uuid");
  });
});
