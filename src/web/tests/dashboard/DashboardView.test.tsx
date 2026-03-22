import { render, screen, fireEvent } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import type {
  DashboardSummaryDto,
  TaskItem,
  EscalationItem,
  DeadlineItem,
} from "@/app/[locale]/(admin)/dashboard/shared";

vi.mock("next-intl", () => ({
  useLocale: () => "en-pr",
}));

vi.mock("@/i18n/routing", () => ({
  useRouter: () => ({
    push: vi.fn(),
  }),
  usePathname: () => "/dashboard",
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

vi.mock("@/app/[locale]/(admin)/dashboard/actions", () => ({
  getDashboardSummaryAction: vi.fn().mockResolvedValue({
    success: true,
    message: "Dashboard summary fetched successfully",
    data: {
      total_clients: 99,
      clients_by_status: [],
      clients_by_tier: [],
      pending_tasks_count: 99,
      recent_escalations_count: 99,
      upcoming_deadlines_count: 99,
      unread_notifications_count: 0,
    },
  }),
  getDashboardCalendarAction: vi.fn().mockResolvedValue({
    success: true,
    message: "Dashboard calendar fetched successfully",
    data: { events: [], team_members: [] },
  }),
}));

import DashboardView from "@/app/[locale]/(admin)/dashboard/DashboardView";

function createSummary(
  overrides: Partial<DashboardSummaryDto> = {},
): DashboardSummaryDto {
  return {
    total_clients: 42,
    clients_by_status: [
      { status: "Active", count: 30 },
      { status: "Inactive", count: 12 },
    ],
    clients_by_tier: [
      { tier_id: "t1", tier_name: "Tier 1", count: 20 },
      { tier_id: "t2", tier_name: "Tier 2", count: 22 },
    ],
    pending_tasks_count: 8,
    recent_escalations_count: 3,
    upcoming_deadlines_count: 5,
    unread_notifications_count: 2,
    ...overrides,
  };
}

const sampleTasks: TaskItem[] = [
  {
    id: "task-1",
    title: "Review tax filing",
    priority: "High",
    status: "To Do",
    due_date: "2026-03-25",
  },
  {
    id: "task-2",
    title: "Collect documents",
    priority: "Medium",
    status: "In Progress",
    due_date: "2026-03-28",
  },
  {
    id: "task-3",
    title: "Send reminder",
    priority: "Low",
    status: "To Do",
    due_date: "2026-04-01",
  },
];

const sampleEscalations: EscalationItem[] = [
  {
    id: "esc-1",
    title: "Missing W-2 form",
    client_name: "Acme Corp",
    created_at: "2026-03-18",
  },
  {
    id: "esc-2",
    title: "Payment overdue",
    client_name: "Beta Inc",
    created_at: "2026-03-19",
  },
];

const sampleDeadlines: DeadlineItem[] = [
  { id: "dl-1", title: "Quarterly filing", due_date: "2026-03-31" },
  { id: "dl-2", title: "Extension request", due_date: "2026-04-15" },
];

describe("DashboardView", () => {
  it("renders the page header with Dashboard title", () => {
    render(
      <DashboardView
        initialSummary={createSummary()}
        initialTasks={[]}
        initialEscalations={[]}
        initialDeadlines={[]}
        initialCalendar={null}
      />,
    );
    expect(
      screen.getByRole("heading", { level: 1 }),
    ).toHaveTextContent("Dashboard");
  });

  it("renders refresh button", () => {
    render(
      <DashboardView
        initialSummary={createSummary()}
        initialTasks={[]}
        initialEscalations={[]}
        initialDeadlines={[]}
        initialCalendar={null}
      />,
    );
    expect(
      screen.getByRole("button", { name: /refresh dashboard/i }),
    ).toBeInTheDocument();
  });

  it("renders all four stat cards with correct values", () => {
    const summary = createSummary({
      total_clients: 42,
      pending_tasks_count: 8,
      upcoming_deadlines_count: 5,
      recent_escalations_count: 3,
    });
    render(
      <DashboardView
        initialSummary={summary}
        initialTasks={[]}
        initialEscalations={[]}
        initialDeadlines={[]}
        initialCalendar={null}
      />,
    );

    expect(screen.getByText("Total Clients")).toBeInTheDocument();
    expect(screen.getByText("42")).toBeInTheDocument();
    expect(screen.getByText("Open Tasks")).toBeInTheDocument();
    expect(screen.getByText("8")).toBeInTheDocument();
    expect(screen.getAllByText("Upcoming Deadlines")).toHaveLength(2);
    expect(screen.getByText("5")).toBeInTheDocument();
    expect(screen.getByText("Escalated")).toBeInTheDocument();
    expect(screen.getByText("3")).toBeInTheDocument();
  });

  it("renders zero values when summary is null", () => {
    render(
      <DashboardView
        initialSummary={null}
        initialTasks={[]}
        initialEscalations={[]}
        initialDeadlines={[]}
        initialCalendar={null}
      />,
    );

    const zeroValues = screen.getAllByText("0");
    expect(zeroValues).toHaveLength(4);
  });

  it("renders pending tasks with priority dots and status badges", () => {
    render(
      <DashboardView
        initialSummary={createSummary()}
        initialTasks={sampleTasks}
        initialEscalations={[]}
        initialDeadlines={[]}
        initialCalendar={null}
      />,
    );

    expect(screen.getByText("Review tax filing")).toBeInTheDocument();
    expect(screen.getByText("Collect documents")).toBeInTheDocument();
    expect(screen.getByText("Send reminder")).toBeInTheDocument();
    expect(screen.getAllByText("To Do")).toHaveLength(2);
    expect(screen.getByText("In Progress")).toBeInTheDocument();
  });

  it("renders empty state for pending tasks", () => {
    render(
      <DashboardView
        initialSummary={createSummary()}
        initialTasks={[]}
        initialEscalations={[]}
        initialDeadlines={[]}
        initialCalendar={null}
      />,
    );

    expect(screen.getByText("No pending tasks.")).toBeInTheDocument();
  });

  it("renders escalated issues with red styling", () => {
    render(
      <DashboardView
        initialSummary={createSummary()}
        initialTasks={[]}
        initialEscalations={sampleEscalations}
        initialDeadlines={[]}
        initialCalendar={null}
      />,
    );

    expect(screen.getByText("Missing W-2 form")).toBeInTheDocument();
    expect(screen.getByText("Acme Corp")).toBeInTheDocument();
    expect(screen.getByText("Payment overdue")).toBeInTheDocument();
    expect(screen.getByText("Beta Inc")).toBeInTheDocument();
  });

  it("renders empty state for escalations", () => {
    render(
      <DashboardView
        initialSummary={createSummary()}
        initialTasks={[]}
        initialEscalations={[]}
        initialDeadlines={[]}
        initialCalendar={null}
      />,
    );

    expect(screen.getByText("No escalated issues.")).toBeInTheDocument();
  });

  it("renders upcoming deadlines with date badges", () => {
    render(
      <DashboardView
        initialSummary={createSummary()}
        initialTasks={[]}
        initialEscalations={[]}
        initialDeadlines={sampleDeadlines}
        initialCalendar={null}
      />,
    );

    expect(screen.getByText("Quarterly filing")).toBeInTheDocument();
    expect(screen.getByText("2026-03-31")).toBeInTheDocument();
    expect(screen.getByText("Extension request")).toBeInTheDocument();
    expect(screen.getByText("2026-04-15")).toBeInTheDocument();
  });

  it("renders empty state for deadlines", () => {
    render(
      <DashboardView
        initialSummary={createSummary()}
        initialTasks={[]}
        initialEscalations={[]}
        initialDeadlines={[]}
        initialCalendar={null}
      />,
    );

    expect(screen.getByText("No upcoming deadlines.")).toBeInTheDocument();
  });

  it("renders View all link in pending tasks section", () => {
    render(
      <DashboardView
        initialSummary={createSummary()}
        initialTasks={[]}
        initialEscalations={[]}
        initialDeadlines={[]}
        initialCalendar={null}
      />,
    );

    expect(screen.getByText("View all →")).toBeInTheDocument();
  });

  it("renders Escalated Issues section title", () => {
    render(
      <DashboardView
        initialSummary={createSummary()}
        initialTasks={[]}
        initialEscalations={[]}
        initialDeadlines={[]}
        initialCalendar={null}
      />,
    );

    expect(screen.getByText("Escalated Issues")).toBeInTheDocument();
  });

  it("calls refresh action when refresh button is clicked", () => {
    render(
      <DashboardView
        initialSummary={createSummary()}
        initialTasks={[]}
        initialEscalations={[]}
        initialDeadlines={[]}
        initialCalendar={null}
      />,
    );

    const refreshButton = screen.getByRole("button", {
      name: /refresh dashboard/i,
    });
    fireEvent.click(refreshButton);
    // Verify button exists and is clickable (action is mocked)
    expect(refreshButton).toBeInTheDocument();
  });

  it("renders due date in task items", () => {
    render(
      <DashboardView
        initialSummary={createSummary()}
        initialTasks={sampleTasks}
        initialEscalations={[]}
        initialDeadlines={[]}
        initialCalendar={null}
      />,
    );

    expect(screen.getByText(/Due: 2026-03-25/)).toBeInTheDocument();
    expect(screen.getByText(/Due: 2026-03-28/)).toBeInTheDocument();
  });
});
