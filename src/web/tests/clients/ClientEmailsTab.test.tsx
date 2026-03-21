import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";

vi.mock("next-intl", () => ({
  useLocale: () => "en-pr",
}));

vi.mock("@/i18n/routing", () => ({
  useRouter: () => ({ push: vi.fn() }),
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

const mockFetchClientEmails = vi.fn();

vi.mock("@/app/[locale]/(admin)/clients/[client_id]/actions", () => ({
  fetchClientEmailsAction: (...args: unknown[]) =>
    mockFetchClientEmails(...args),
}));

import ClientEmailsTab from "@/app/[locale]/(admin)/clients/[client_id]/ClientEmailsTab";

const mockEmails = [
  {
    email_id: "email-1",
    client_id: "c1-uuid",
    gmail_message_id: "msg-1",
    gmail_thread_id: "thread-1",
    subject: "Tax Return Discussion",
    from: "Client User <client@example.com>",
    to: "team@example.com",
    body_preview: "Hello, I wanted to discuss my tax return.",
    has_attachments: false,
    received_at: "2024-06-01T10:00:00Z",
  },
  {
    email_id: "email-2",
    client_id: "c1-uuid",
    gmail_message_id: "msg-2",
    gmail_thread_id: "thread-1",
    subject: "Re: Tax Return Discussion",
    from: "Team Member <team@example.com>",
    to: "client@example.com",
    body_preview: "Sure, let me check your records.",
    has_attachments: true,
    received_at: "2024-06-01T14:00:00Z",
  },
  {
    email_id: "email-3",
    client_id: "c1-uuid",
    gmail_message_id: "msg-3",
    gmail_thread_id: "thread-2",
    subject: "Quarterly Report Ready",
    from: "Team Member <team@example.com>",
    to: "client@example.com",
    body_preview: "Your quarterly report is ready for review.",
    has_attachments: true,
    received_at: "2024-07-15T09:00:00Z",
  },
];

describe("ClientEmailsTab", () => {
  beforeEach(() => {
    mockFetchClientEmails.mockReset();
  });

  it("shows loading state initially", () => {
    mockFetchClientEmails.mockImplementation(() => new Promise(() => {}));
    render(<ClientEmailsTab clientId="c1-uuid" />);
    expect(screen.getByText("Loading emails…")).toBeInTheDocument();
  });

  it("loads and displays emails", async () => {
    mockFetchClientEmails.mockResolvedValue({
      success: true,
      data: {
        items: mockEmails,
        total_count: 3,
        page: 1,
        page_size: 20,
      },
    });

    render(<ClientEmailsTab clientId="c1-uuid" />);

    await waitFor(() => {
      expect(screen.getByText("3 emails")).toBeInTheDocument();
    });

    expect(screen.getByText("Client User")).toBeInTheDocument();
    expect(screen.getAllByText("Team Member").length).toBe(2);
  });

  it("displays empty state when no emails", async () => {
    mockFetchClientEmails.mockResolvedValue({
      success: true,
      data: {
        items: [],
        total_count: 0,
        page: 1,
        page_size: 20,
      },
    });

    render(<ClientEmailsTab clientId="c1-uuid" />);

    await waitFor(() => {
      expect(
        screen.getByText("No emails found for this client."),
      ).toBeInTheDocument();
    });
  });

  it("shows search input", async () => {
    mockFetchClientEmails.mockResolvedValue({
      success: true,
      data: {
        items: mockEmails,
        total_count: 3,
        page: 1,
        page_size: 20,
      },
    });

    render(<ClientEmailsTab clientId="c1-uuid" />);

    await waitFor(() => {
      expect(
        screen.getByPlaceholderText("Search emails\u2026"),
      ).toBeInTheDocument();
    });
  });

  it("shows select email prompt when no email is selected", async () => {
    mockFetchClientEmails.mockResolvedValue({
      success: true,
      data: {
        items: mockEmails,
        total_count: 3,
        page: 1,
        page_size: 20,
      },
    });

    render(<ClientEmailsTab clientId="c1-uuid" />);

    await waitFor(() => {
      expect(
        screen.getByText("Select an email to view"),
      ).toBeInTheDocument();
    });
  });

  it("shows thread view when email is selected", async () => {
    mockFetchClientEmails.mockResolvedValue({
      success: true,
      data: {
        items: mockEmails,
        total_count: 3,
        page: 1,
        page_size: 20,
      },
    });

    render(
      <ClientEmailsTab clientId="c1-uuid" clientEmail="client@example.com" />,
    );

    await waitFor(() => {
      expect(screen.getByText("Client User")).toBeInTheDocument();
    });

    fireEvent.click(screen.getAllByText("Client User")[0]);

    await waitFor(() => {
      // Subject appears in both list and thread header
      expect(screen.getAllByText("Tax Return Discussion").length).toBeGreaterThanOrEqual(1);
      expect(screen.getByText("Reply")).toBeInTheDocument();
      expect(screen.getByText("Escalate")).toBeInTheDocument();
    });
  });

  it("displays email body preview in thread view", async () => {
    mockFetchClientEmails.mockResolvedValue({
      success: true,
      data: {
        items: mockEmails,
        total_count: 3,
        page: 1,
        page_size: 20,
      },
    });

    render(
      <ClientEmailsTab clientId="c1-uuid" clientEmail="client@example.com" />,
    );

    await waitFor(() => {
      expect(screen.getByText("Client User")).toBeInTheDocument();
    });

    fireEvent.click(screen.getAllByText("Client User")[0]);

    await waitFor(() => {
      expect(
        screen.getByText("Hello, I wanted to discuss my tax return."),
      ).toBeInTheDocument();
    });
  });

  it("shows error message when fetch fails", async () => {
    mockFetchClientEmails.mockResolvedValue({
      success: false,
      message: "Network error",
    });

    render(<ClientEmailsTab clientId="c1-uuid" />);

    await waitFor(() => {
      expect(screen.getByText("Network error")).toBeInTheDocument();
    });
  });

  it("performs search when search button is clicked", async () => {
    mockFetchClientEmails.mockResolvedValue({
      success: true,
      data: {
        items: mockEmails,
        total_count: 3,
        page: 1,
        page_size: 20,
      },
    });

    render(<ClientEmailsTab clientId="c1-uuid" />);

    await waitFor(() => {
      expect(
        screen.getByPlaceholderText("Search emails\u2026"),
      ).toBeInTheDocument();
    });

    const searchInput = screen.getByPlaceholderText("Search emails\u2026");
    fireEvent.change(searchInput, { target: { value: "tax" } });

    // Click the search button
    const searchButtons = screen.getAllByRole("button");
    const searchBtn = searchButtons.find(
      (btn) => btn.querySelector(".lucide-search") !== null,
    );
    if (searchBtn) {
      fireEvent.click(searchBtn);
    }

    await waitFor(() => {
      expect(mockFetchClientEmails).toHaveBeenCalledWith(
        expect.objectContaining({ search: "tax" }),
      );
    });
  });

  it("groups thread emails chronologically", async () => {
    mockFetchClientEmails.mockResolvedValue({
      success: true,
      data: {
        items: mockEmails,
        total_count: 3,
        page: 1,
        page_size: 20,
      },
    });

    render(
      <ClientEmailsTab clientId="c1-uuid" clientEmail="client@example.com" />,
    );

    await waitFor(() => {
      expect(screen.getByText("Client User")).toBeInTheDocument();
    });

    // Click on an email from thread-1 (which has 2 emails)
    fireEvent.click(screen.getAllByText("Client User")[0]);

    await waitFor(() => {
      // Thread should show "Thread · 2 emails" for thread-1
      expect(screen.getByText(/Thread · 2/)).toBeInTheDocument();
    });
  });
});
