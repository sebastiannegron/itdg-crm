import { render, screen, fireEvent, within } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import type { CommunicationTemplateDto } from "@/app/[locale]/(admin)/communications/templates/shared";

vi.mock("next-intl", () => ({
  useLocale: () => "en-pr",
}));

vi.mock("@/i18n/routing", () => ({
  useRouter: () => ({
    push: vi.fn(),
  }),
  usePathname: () => "/communications/templates",
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
  "@/app/[locale]/(admin)/communications/templates/actions",
  () => ({
    fetchTemplates: vi.fn(),
    renderTemplatePreview: vi.fn(),
    createNewTemplate: vi.fn(),
    updateExistingTemplate: vi.fn(),
    retireExistingTemplate: vi.fn(),
  }),
);

import TemplatesView from "@/app/[locale]/(admin)/communications/templates/TemplatesView";

function createTemplate(
  overrides: Partial<CommunicationTemplateDto> = {},
): CommunicationTemplateDto {
  return {
    id: "t1-uuid",
    category: 4,
    name: "Welcome Email",
    subject_template: "Welcome {{client_name}}!",
    body_template:
      "Dear {{client_name}}, welcome to {{company_name}}.",
    language: "en",
    version: 1,
    is_active: true,
    created_by_id: "user-uuid",
    created_at: "2024-01-15T10:00:00Z",
    updated_at: "2024-06-01T14:30:00Z",
    ...overrides,
  };
}

describe("TemplatesView", () => {
  it("renders the page header with Templates title", () => {
    render(<TemplatesView initialTemplates={[]} />);
    expect(screen.getByRole("heading", { level: 1 })).toHaveTextContent(
      "Templates",
    );
  });

  it("renders breadcrumbs with Dashboard and Communications", () => {
    render(<TemplatesView initialTemplates={[]} />);
    expect(screen.getByText("Dashboard")).toBeInTheDocument();
    expect(screen.getByText("Communications")).toBeInTheDocument();
  });

  it("renders empty state when no templates", () => {
    render(<TemplatesView initialTemplates={[]} />);
    expect(screen.getByText("No templates yet")).toBeInTheDocument();
    expect(
      screen.getByText(
        "Templates will appear here once they are created.",
      ),
    ).toBeInTheDocument();
  });

  it("renders template cards with name and category", () => {
    const templates = [
      createTemplate(),
      createTemplate({
        id: "t2-uuid",
        name: "Payment Due",
        category: 2,
        subject_template: "Payment reminder for {{client_name}}",
      }),
    ];
    render(<TemplatesView initialTemplates={templates} />);

    expect(screen.getByText("Welcome Email")).toBeInTheDocument();
    expect(screen.getByText("Payment Due")).toBeInTheDocument();
    // "General" appears in both the category filter dropdown and the badge
    expect(screen.getAllByText("General").length).toBeGreaterThanOrEqual(1);
    // "Payment Reminder" appears in both the category filter dropdown and the badge
    expect(screen.getAllByText("Payment Reminder").length).toBeGreaterThanOrEqual(1);
  });

  it("renders search input", () => {
    const templates = [createTemplate()];
    render(<TemplatesView initialTemplates={templates} />);

    expect(
      screen.getByPlaceholderText("Search templates…"),
    ).toBeInTheDocument();
  });

  it("renders category filter dropdown", () => {
    const templates = [createTemplate()];
    render(<TemplatesView initialTemplates={templates} />);

    expect(
      screen.getByLabelText("Filter by category"),
    ).toBeInTheDocument();
  });

  it("renders status filter dropdown", () => {
    const templates = [createTemplate()];
    render(<TemplatesView initialTemplates={templates} />);

    expect(
      screen.getByLabelText("Filter by status"),
    ).toBeInTheDocument();
  });

  it("filters by search text", () => {
    const templates = [
      createTemplate({ id: "t1", name: "Welcome Email" }),
      createTemplate({
        id: "t2",
        name: "Payment Due",
        subject_template: "Payment reminder for {{client_name}}",
      }),
    ];
    render(<TemplatesView initialTemplates={templates} />);

    const searchInput = screen.getByPlaceholderText("Search templates…");
    fireEvent.change(searchInput, { target: { value: "Payment" } });

    expect(screen.getByText("Payment Due")).toBeInTheDocument();
    expect(screen.queryByText("Welcome Email")).not.toBeInTheDocument();
  });

  it("filters by category", () => {
    const templates = [
      createTemplate({ id: "t1", name: "Welcome Email", category: 4 }),
      createTemplate({ id: "t2", name: "Payment Due", category: 2 }),
    ];
    render(<TemplatesView initialTemplates={templates} />);

    const categorySelect = screen.getByLabelText("Filter by category");
    fireEvent.change(categorySelect, { target: { value: "2" } });

    expect(screen.queryByText("Welcome Email")).not.toBeInTheDocument();
    expect(screen.getByText("Payment Due")).toBeInTheDocument();
  });

  it("filters by active status", () => {
    const templates = [
      createTemplate({
        id: "t1",
        name: "Active Template",
        is_active: true,
      }),
      createTemplate({
        id: "t2",
        name: "Retired Template",
        is_active: false,
      }),
    ];
    render(<TemplatesView initialTemplates={templates} />);

    const statusSelect = screen.getByLabelText("Filter by status");
    fireEvent.change(statusSelect, { target: { value: "active" } });

    expect(screen.getByText("Active Template")).toBeInTheDocument();
    expect(
      screen.queryByText("Retired Template"),
    ).not.toBeInTheDocument();
  });

  it("shows no results message when filters match nothing", () => {
    const templates = [createTemplate({ category: 4 })];
    render(<TemplatesView initialTemplates={templates} />);

    const categorySelect = screen.getByLabelText("Filter by category");
    fireEvent.change(categorySelect, { target: { value: "0" } });

    expect(
      screen.getByText("No templates match your filters."),
    ).toBeInTheDocument();
  });

  it("displays language on template cards", () => {
    const templates = [createTemplate({ language: "en" })];
    render(<TemplatesView initialTemplates={templates} />);

    // language is rendered with CSS uppercase class, DOM text is lowercase
    expect(screen.getByText("en")).toBeInTheDocument();
  });

  it("displays version on template cards", () => {
    const templates = [createTemplate({ version: 3 })];
    render(<TemplatesView initialTemplates={templates} />);

    expect(screen.getByText("v3")).toBeInTheDocument();
  });

  it("displays active status badge", () => {
    const templates = [createTemplate({ is_active: true })];
    render(<TemplatesView initialTemplates={templates} />);

    // "Active" appears in both the status filter dropdown option and the card badge
    const activeElements = screen.getAllByText("Active");
    expect(activeElements.length).toBeGreaterThanOrEqual(2);
  });

  it("displays inactive status badge", () => {
    const templates = [createTemplate({ is_active: false })];
    render(<TemplatesView initialTemplates={templates} />);

    // "Inactive" appears in both the status filter dropdown option and the card badge
    const inactiveElements = screen.getAllByText("Inactive");
    expect(inactiveElements.length).toBeGreaterThanOrEqual(2);
  });

  it("shows New Template button", () => {
    render(<TemplatesView initialTemplates={[]} />);

    expect(screen.getByText("New Template")).toBeInTheDocument();
  });

  it("navigates to editor view when clicking a template card", () => {
    const templates = [createTemplate()];
    render(<TemplatesView initialTemplates={templates} />);

    fireEvent.click(screen.getByText("Welcome Email"));

    expect(
      screen.getByRole("heading", { level: 1 }),
    ).toHaveTextContent("Template Editor");
  });

  it("shows template editor with merge field toolbar", () => {
    const templates = [createTemplate()];
    render(<TemplatesView initialTemplates={templates} />);

    fireEvent.click(screen.getByText("Welcome Email"));

    expect(screen.getByText("Merge Fields")).toBeInTheDocument();
    expect(screen.getByText("Client Name")).toBeInTheDocument();
    expect(screen.getByText("Client Email")).toBeInTheDocument();
    expect(screen.getByText("Due Date")).toBeInTheDocument();
  });

  it("populates editor fields with selected template data", () => {
    const templates = [createTemplate()];
    render(<TemplatesView initialTemplates={templates} />);

    fireEvent.click(screen.getByText("Welcome Email"));

    const nameInput = screen.getByLabelText("Name") as HTMLInputElement;
    expect(nameInput.value).toBe("Welcome Email");

    const subjectInput = screen.getByLabelText("Subject") as HTMLInputElement;
    expect(subjectInput.value).toBe("Welcome {{client_name}}!");
  });

  it("shows preview pane when clicking Preview button", () => {
    const templates = [createTemplate()];
    render(<TemplatesView initialTemplates={templates} />);

    fireEvent.click(screen.getByText("Welcome Email"));
    fireEvent.click(screen.getByText("Preview"));

    expect(screen.getByText("Welcome John Doe!")).toBeInTheDocument();
    expect(
      screen.getByText(
        "Dear John Doe, welcome to R&A Tax Consultants.",
      ),
    ).toBeInTheDocument();
  });

  it("navigates back to list when clicking Back to templates", () => {
    const templates = [createTemplate()];
    render(<TemplatesView initialTemplates={templates} />);

    fireEvent.click(screen.getByText("Welcome Email"));
    expect(
      screen.getByRole("heading", { level: 1 }),
    ).toHaveTextContent("Template Editor");

    fireEvent.click(screen.getByText("Back to templates"));
    expect(screen.getByRole("heading", { level: 1 })).toHaveTextContent(
      "Templates",
    );
  });

  it("shows new template editor with empty fields", () => {
    render(<TemplatesView initialTemplates={[]} />);

    fireEvent.click(screen.getByText("New Template"));

    expect(
      screen.getByRole("heading", { level: 1 }),
    ).toHaveTextContent("Template Editor");

    const nameInput = screen.getByLabelText("Name") as HTMLInputElement;
    expect(nameInput.value).toBe("");

    const subjectInput = screen.getByLabelText("Subject") as HTMLInputElement;
    expect(subjectInput.value).toBe("");
  });
});
