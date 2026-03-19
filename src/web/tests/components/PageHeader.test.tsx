import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import { PageHeader } from "@/app/_components/PageHeader";

vi.mock("@/i18n/routing", () => ({
  Link: ({ href, children, ...props }: { href: string; children: React.ReactNode; [key: string]: unknown }) => (
    <a href={href} {...props}>{children}</a>
  ),
}));

describe("PageHeader", () => {
  it("renders title", () => {
    render(<PageHeader title="Clients" />);
    expect(
      screen.getByRole("heading", { level: 1, name: "Clients" })
    ).toBeInTheDocument();
  });

  it("renders breadcrumbs with links", () => {
    render(
      <PageHeader
        title="Details"
        breadcrumbs={[
          { label: "Home", href: "/" },
          { label: "Clients", href: "/clients" },
          { label: "Current" },
        ]}
      />
    );
    expect(screen.getByLabelText("Breadcrumb")).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Home" })).toHaveAttribute(
      "href",
      "/"
    );
    expect(screen.getByRole("link", { name: "Clients" })).toHaveAttribute(
      "href",
      "/clients"
    );
    expect(screen.getByText("Current")).toBeInTheDocument();
  });

  it("renders action buttons", () => {
    render(
      <PageHeader
        title="Clients"
        actions={<button type="button">Add Client</button>}
      />
    );
    expect(
      screen.getByRole("button", { name: "Add Client" })
    ).toBeInTheDocument();
  });

  it("does not render breadcrumbs when not provided", () => {
    render(<PageHeader title="Clients" />);
    expect(screen.queryByLabelText("Breadcrumb")).not.toBeInTheDocument();
  });
});
