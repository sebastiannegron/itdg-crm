import { render, screen, fireEvent } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";

vi.mock("next-intl", () => ({
  useLocale: () => "en-pr",
}));

vi.mock("@/i18n/routing", () => ({
  useRouter: () => ({
    push: vi.fn(),
  }),
  usePathname: () => "/communications/send",
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
  "@/app/[locale]/(admin)/communications/send/actions",
  () => ({
    fetchActiveTemplates: vi.fn(),
    fetchClients: vi.fn(),
    previewTemplate: vi.fn(),
    sendMessage: vi.fn(),
  }),
);

import SendTemplateView from "@/app/[locale]/(admin)/communications/send/SendTemplateView";
import type { CommunicationTemplateDto } from "@/app/[locale]/(admin)/communications/send/shared";

function createTemplate(
  overrides: Partial<CommunicationTemplateDto> = {},
): CommunicationTemplateDto {
  return {
    id: "t1-uuid",
    category: 4,
    name: "Welcome Email",
    subject_template: "Welcome {{client_name}}!",
    body_template: "Dear {{client_name}}, welcome to {{company_name}}.",
    language: "en",
    version: 1,
    is_active: true,
    created_by_id: "user-1",
    created_at: "2025-01-01T00:00:00Z",
    updated_at: "2025-01-01T00:00:00Z",
    ...overrides,
  };
}

function createClient(overrides = {}) {
  return {
    client_id: "c1-uuid",
    name: "John Doe",
    contact_email: "john@example.com",
    ...overrides,
  };
}

describe("SendTemplateView", () => {
  it("renders the page title", () => {
    render(
      <SendTemplateView initialTemplates={[]} initialClients={[]} />,
    );

    expect(screen.getByRole("heading", { name: "Send Template Message" })).toBeInTheDocument();
  });

  it("renders empty state when no templates are available", () => {
    render(
      <SendTemplateView initialTemplates={[]} initialClients={[]} />,
    );

    expect(screen.getByText("No templates yet")).toBeInTheDocument();
  });

  it("renders template cards when templates are provided", () => {
    const templates = [
      createTemplate({ id: "t1", name: "Welcome Email" }),
      createTemplate({
        id: "t2",
        name: "Tax Season Alert",
        category: 3,
      }),
    ];

    render(
      <SendTemplateView
        initialTemplates={templates}
        initialClients={[createClient()]}
      />,
    );

    expect(screen.getByText("Welcome Email")).toBeInTheDocument();
    expect(screen.getByText("Tax Season Alert")).toBeInTheDocument();
  });

  it("renders client selector with provided clients", () => {
    const clients = [
      createClient({ client_id: "c1", name: "John Doe" }),
      createClient({ client_id: "c2", name: "Jane Smith", contact_email: "jane@example.com" }),
    ];

    render(
      <SendTemplateView
        initialTemplates={[createTemplate()]}
        initialClients={clients}
      />,
    );

    // There are multiple comboboxes (client + category), find the client one
    const selects = screen.getAllByRole("combobox");
    const clientSelect = selects.find((s) =>
      s.querySelector("option[value='c1']"),
    );
    expect(clientSelect).toBeTruthy();
  });

  it("shows merge fields section when a template is selected", () => {
    const templates = [createTemplate()];
    const clients = [createClient()];

    render(
      <SendTemplateView
        initialTemplates={templates}
        initialClients={clients}
      />,
    );

    // Select a template
    fireEvent.click(screen.getByText("Welcome Email"));

    expect(screen.getByText("Merge Fields")).toBeInTheDocument();
    expect(screen.getByText("Client Name")).toBeInTheDocument();
  });

  it("shows delivery channels when template and client are selected", () => {
    const templates = [createTemplate()];
    const clients = [createClient()];

    render(
      <SendTemplateView
        initialTemplates={templates}
        initialClients={clients}
      />,
    );

    // Select a template
    fireEvent.click(screen.getByText("Welcome Email"));

    // Select a client using the first combobox
    const selects = screen.getAllByRole("combobox");
    const clientSelect = selects.find((s) =>
      s.querySelector("option[value='c1-uuid']"),
    );
    fireEvent.change(clientSelect!, { target: { value: "c1-uuid" } });

    expect(screen.getByText("Delivery Channels")).toBeInTheDocument();
    expect(screen.getByText("Portal Message")).toBeInTheDocument();
  });

  it("filters templates by search", () => {
    const templates = [
      createTemplate({ id: "t1", name: "Welcome Email" }),
      createTemplate({ id: "t2", name: "Tax Season Alert" }),
    ];

    render(
      <SendTemplateView
        initialTemplates={templates}
        initialClients={[createClient()]}
      />,
    );

    const searchInput = screen.getByPlaceholderText("Search templates…");
    fireEvent.change(searchInput, { target: { value: "Tax" } });

    expect(screen.queryByText("Welcome Email")).not.toBeInTheDocument();
    expect(screen.getByText("Tax Season Alert")).toBeInTheDocument();
  });

  it("filters templates by category", () => {
    const templates = [
      createTemplate({ id: "t1", name: "Welcome Email", category: 0 }),
      createTemplate({ id: "t2", name: "General Notice", category: 4 }),
    ];

    render(
      <SendTemplateView
        initialTemplates={templates}
        initialClients={[createClient()]}
      />,
    );

    // Find the category filter select (the one with "All Categories")
    const selects = screen.getAllByRole("combobox");
    const categorySelect = selects.find((s) =>
      s.querySelector("option[value='all']"),
    );

    fireEvent.change(categorySelect!, { target: { value: "0" } });

    expect(screen.getByText("Welcome Email")).toBeInTheDocument();
    expect(screen.queryByText("General Notice")).not.toBeInTheDocument();
  });

  it("disables preview button when no template or client is selected", () => {
    render(
      <SendTemplateView
        initialTemplates={[createTemplate()]}
        initialClients={[createClient()]}
      />,
    );

    const previewButton = screen.getByRole("button", { name: /Preview/ });
    expect(previewButton).toBeDisabled();
  });
});
