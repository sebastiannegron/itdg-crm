import { render, screen } from "@testing-library/react";
import { describe, it, expect } from "vitest";
import { EmptyState } from "@/app/_components/EmptyState";
import { Inbox } from "lucide-react";

describe("EmptyState", () => {
  it("renders icon, title, and message", () => {
    render(
      <EmptyState
        icon={Inbox}
        title="No clients"
        message="Get started by adding your first client."
      />
    );
    expect(screen.getByText("No clients")).toBeInTheDocument();
    expect(
      screen.getByText("Get started by adding your first client.")
    ).toBeInTheDocument();
  });

  it("renders CTA button with onClick", () => {
    render(
      <EmptyState
        icon={Inbox}
        title="No clients"
        message="Get started."
        actionLabel="Add Client"
        onAction={() => {}}
      />
    );
    expect(
      screen.getByRole("button", { name: "Add Client" })
    ).toBeInTheDocument();
  });

  it("renders CTA as link when actionHref is provided", () => {
    render(
      <EmptyState
        icon={Inbox}
        title="No clients"
        message="Get started."
        actionLabel="Add Client"
        actionHref="/clients/new"
      />
    );
    const link = screen.getByRole("link", { name: "Add Client" });
    expect(link).toHaveAttribute("href", "/clients/new");
  });

  it("does not render CTA when no action props provided", () => {
    render(
      <EmptyState icon={Inbox} title="No clients" message="Get started." />
    );
    expect(screen.queryByRole("button")).not.toBeInTheDocument();
    expect(screen.queryByRole("link")).not.toBeInTheDocument();
  });
});
