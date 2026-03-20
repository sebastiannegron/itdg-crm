import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";

vi.mock("next-intl", () => ({
  useLocale: () => "en-pr",
}));

const mockPush = vi.fn();
vi.mock("@/i18n/routing", () => ({
  useRouter: () => ({
    push: mockPush,
  }),
  usePathname: () => "/clients/new",
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

vi.mock("@/app/[locale]/(admin)/clients/new/actions", () => ({
  createClientAction: vi.fn().mockResolvedValue({
    success: true,
    message: "Client created successfully",
    data: { client_id: "new-uuid" },
  }),
}));

import ClientForm from "@/app/[locale]/(admin)/clients/new/ClientForm";

describe("ClientForm", () => {
  beforeEach(() => {
    mockPush.mockClear();
  });

  it("renders the page header with New Client title", () => {
    render(<ClientForm />);
    expect(
      screen.getByRole("heading", { level: 1 }),
    ).toHaveTextContent("New Client");
  });

  it("renders breadcrumbs with Dashboard, Clients, and New Client", () => {
    render(<ClientForm />);
    expect(screen.getByText("Dashboard")).toBeInTheDocument();
    expect(screen.getByText("Clients")).toBeInTheDocument();
  });

  it("renders form fields for client creation", () => {
    render(<ClientForm />);
    expect(screen.getByLabelText(/Client \/ Contact/)).toBeInTheDocument();
    expect(screen.getByLabelText("Email")).toBeInTheDocument();
    expect(screen.getByLabelText("Phone")).toBeInTheDocument();
    expect(screen.getByLabelText(/Status/)).toBeInTheDocument();
    expect(screen.getByLabelText("Industry")).toBeInTheDocument();
    expect(screen.getByLabelText("Address")).toBeInTheDocument();
    expect(screen.getByLabelText("Notes")).toBeInTheDocument();
  });

  it("renders Create Client submit button", () => {
    render(<ClientForm />);
    expect(
      screen.getByRole("button", { name: "Create Client" }),
    ).toBeInTheDocument();
  });

  it("renders Back to clients button", () => {
    render(<ClientForm />);
    const backElements = screen.getAllByText("Back to clients");
    expect(backElements.length).toBeGreaterThanOrEqual(1);
  });

  it("has empty default values for all fields", () => {
    render(<ClientForm />);
    expect(screen.getByLabelText(/Client \/ Contact/)).toHaveValue("");
    expect(screen.getByLabelText("Email")).toHaveValue("");
    expect(screen.getByLabelText("Phone")).toHaveValue("");
  });

  it("has Active as default status", () => {
    render(<ClientForm />);
    expect(screen.getByLabelText(/Status/)).toHaveValue("Active");
  });

  it("renders all status options", () => {
    render(<ClientForm />);
    const statusSelect = screen.getByLabelText(/Status/);
    const options = statusSelect.querySelectorAll("option");
    const optionValues = Array.from(options).map((opt) => opt.textContent);
    expect(optionValues).toContain("Active");
    expect(optionValues).toContain("Inactive");
    expect(optionValues).toContain("Suspended");
  });
});
