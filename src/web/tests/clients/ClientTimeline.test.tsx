import { render, screen, fireEvent, waitFor, act } from "@testing-library/react";
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

const mockFetchTimeline = vi.fn();

vi.mock(
  "@/app/[locale]/(admin)/clients/[client_id]/actions",
  () => ({
    fetchClientTimelineAction: (...args: unknown[]) =>
      mockFetchTimeline(...args),
  }),
);

import ClientTimeline from "@/app/[locale]/(admin)/clients/[client_id]/ClientTimeline";

function createTimelineItem(overrides: Record<string, unknown> = {}) {
  return {
    id: "item-1",
    type: "document",
    description: "Tax_Return_2024.pdf",
    timestamp: "2024-06-15T14:30:00Z",
    actor: null,
    ...overrides,
  };
}

describe("ClientTimeline", () => {
  beforeEach(() => {
    mockFetchTimeline.mockReset();
    mockFetchTimeline.mockResolvedValue({
      success: true,
      data: { items: [], total_count: 0, page: 1, page_size: 10 },
    });
  });

  it("renders timeline title heading", async () => {
    render(<ClientTimeline clientId="c1-uuid" />);

    await waitFor(() => {
      expect(screen.getByText("Activity Timeline")).toBeInTheDocument();
    });
  });

  it("renders empty state when no items", async () => {
    render(<ClientTimeline clientId="c1-uuid" />);

    await waitFor(() => {
      expect(
        screen.getByText("No activity yet for this client."),
      ).toBeInTheDocument();
    });
  });

  it("renders timeline items with description and type", async () => {
    mockFetchTimeline.mockResolvedValue({
      success: true,
      data: {
        items: [
          createTimelineItem({
            id: "1",
            type: "document",
            description: "Tax_Return_2024.pdf",
          }),
          createTimelineItem({
            id: "2",
            type: "email",
            description: "Re: Tax filing update",
            actor: "john@example.com",
          }),
          createTimelineItem({
            id: "3",
            type: "message",
            description: "Payment reminder sent",
          }),
        ],
        total_count: 3,
        page: 1,
        page_size: 10,
      },
    });

    render(<ClientTimeline clientId="c1-uuid" />);

    await waitFor(() => {
      expect(screen.getByText("Tax_Return_2024.pdf")).toBeInTheDocument();
      expect(screen.getByText("Re: Tax filing update")).toBeInTheDocument();
      expect(screen.getByText("Payment reminder sent")).toBeInTheDocument();
    });

    expect(screen.getByText("Document")).toBeInTheDocument();
    expect(screen.getByText("Email")).toBeInTheDocument();
    expect(screen.getByText("Message")).toBeInTheDocument();
  });

  it("renders actor when present", async () => {
    mockFetchTimeline.mockResolvedValue({
      success: true,
      data: {
        items: [
          createTimelineItem({
            id: "1",
            type: "email",
            description: "Some email",
            actor: "john@example.com",
          }),
        ],
        total_count: 1,
        page: 1,
        page_size: 10,
      },
    });

    render(<ClientTimeline clientId="c1-uuid" />);

    await waitFor(() => {
      expect(screen.getByText("john@example.com")).toBeInTheDocument();
    });
  });

  it("renders formatted timestamp", async () => {
    mockFetchTimeline.mockResolvedValue({
      success: true,
      data: {
        items: [
          createTimelineItem({
            id: "1",
            timestamp: "2024-06-15T14:30:00Z",
          }),
        ],
        total_count: 1,
        page: 1,
        page_size: 10,
      },
    });

    render(<ClientTimeline clientId="c1-uuid" />);

    await waitFor(() => {
      expect(screen.getByText(/Jun 15, 2024/)).toBeInTheDocument();
    });
  });

  it("shows Load more button when there are more items", async () => {
    mockFetchTimeline.mockResolvedValue({
      success: true,
      data: {
        items: [createTimelineItem({ id: "1" })],
        total_count: 15,
        page: 1,
        page_size: 10,
      },
    });

    render(<ClientTimeline clientId="c1-uuid" />);

    await waitFor(() => {
      expect(
        screen.getByRole("button", { name: /load more/i }),
      ).toBeInTheDocument();
    });
  });

  it("does not show Load more button when all items loaded", async () => {
    mockFetchTimeline.mockResolvedValue({
      success: true,
      data: {
        items: [createTimelineItem({ id: "1" })],
        total_count: 1,
        page: 1,
        page_size: 10,
      },
    });

    render(<ClientTimeline clientId="c1-uuid" />);

    await waitFor(() => {
      expect(screen.getByText("Tax_Return_2024.pdf")).toBeInTheDocument();
    });

    expect(screen.queryByRole("button", { name: /load more/i })).not.toBeInTheDocument();
  });

  it("loads more items when Load more is clicked", async () => {
    mockFetchTimeline
      .mockResolvedValueOnce({
        success: true,
        data: {
          items: [createTimelineItem({ id: "1", description: "First item" })],
          total_count: 2,
          page: 1,
          page_size: 10,
        },
      })
      .mockResolvedValueOnce({
        success: true,
        data: {
          items: [
            createTimelineItem({ id: "2", description: "Second item" }),
          ],
          total_count: 2,
          page: 2,
          page_size: 10,
        },
      });

    render(<ClientTimeline clientId="c1-uuid" />);

    await waitFor(() => {
      expect(screen.getByText("First item")).toBeInTheDocument();
    });

    const loadMoreButton = await screen.findByRole("button", {
      name: /load more/i,
    });
    await act(async () => {
      fireEvent.click(loadMoreButton);
    });

    await waitFor(() => {
      expect(screen.getByText("Second item")).toBeInTheDocument();
    });

    expect(mockFetchTimeline).toHaveBeenCalledTimes(2);
    expect(mockFetchTimeline).toHaveBeenLastCalledWith({
      clientId: "c1-uuid",
      page: 2,
      pageSize: 10,
    });
  });

  it("renders error message on fetch failure", async () => {
    mockFetchTimeline.mockResolvedValue({
      success: false,
      message: "Server error",
    });

    render(<ClientTimeline clientId="c1-uuid" />);

    await waitFor(() => {
      expect(screen.getByText("Server error")).toBeInTheDocument();
    });
  });

  it("calls fetchClientTimelineAction with correct params", async () => {
    render(<ClientTimeline clientId="c1-uuid" />);

    await waitFor(() => {
      expect(mockFetchTimeline).toHaveBeenCalledWith({
        clientId: "c1-uuid",
        page: 1,
        pageSize: 10,
      });
    });
  });
});
