import { render, screen, fireEvent } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import type {
  DashboardCalendarDto,
  DashboardCalendarEventDto,
  CalendarTeamMemberDto,
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
  getDashboardCalendarAction: vi.fn().mockResolvedValue({
    success: true,
    message: "Dashboard calendar fetched successfully",
    data: { events: [], team_members: [] },
  }),
}));

import CalendarWidget from "@/app/[locale]/(admin)/dashboard/CalendarWidget";

function createCalendar(
  overrides: Partial<DashboardCalendarDto> = {},
): DashboardCalendarDto {
  return {
    events: [],
    team_members: [],
    ...overrides,
  };
}

function createEvent(
  overrides: Partial<DashboardCalendarEventDto> = {},
): DashboardCalendarEventDto {
  const now = new Date();
  const start = new Date(now);
  start.setHours(10, 0, 0, 0);
  const end = new Date(now);
  end.setHours(11, 0, 0, 0);

  return {
    id: "evt-1",
    summary: "Team Meeting",
    description: null,
    location: null,
    start: start.toISOString(),
    end: end.toISOString(),
    calendar_id: "primary",
    html_link: null,
    status: "confirmed",
    organizer_email: null,
    created: null,
    updated: null,
    team_member_name: "Alice Rivera",
    team_member_color: "#3B82F6",
    ...overrides,
  };
}

const sampleTeamMembers: CalendarTeamMemberDto[] = [
  { name: "Alice Rivera", color: "#3B82F6" },
  { name: "Bob Santiago", color: "#10B981" },
];

describe("CalendarWidget", () => {
  it("renders the calendar title", () => {
    render(<CalendarWidget initialCalendar={createCalendar()} />);
    expect(screen.getByText("Team Calendar")).toBeInTheDocument();
  });

  it("renders navigation buttons", () => {
    render(<CalendarWidget initialCalendar={createCalendar()} />);
    expect(
      screen.getByRole("button", { name: /previous/i }),
    ).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /today/i })).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: /next/i }),
    ).toBeInTheDocument();
  });

  it("renders team member legend when members are present", () => {
    render(
      <CalendarWidget
        initialCalendar={createCalendar({ team_members: sampleTeamMembers })}
      />,
    );
    expect(screen.getByText("Alice Rivera")).toBeInTheDocument();
    expect(screen.getByText("Bob Santiago")).toBeInTheDocument();
  });

  it("renders not connected message when no calendar data", () => {
    render(<CalendarWidget initialCalendar={null} />);
    const messages = screen.getAllByText("No team calendars connected.");
    expect(messages.length).toBeGreaterThanOrEqual(1);
  });

  it("renders not connected message when no team members and no events", () => {
    render(
      <CalendarWidget
        initialCalendar={createCalendar({ events: [], team_members: [] })}
      />,
    );
    const messages = screen.getAllByText("No team calendars connected.");
    expect(messages.length).toBeGreaterThanOrEqual(1);
  });

  it("renders no events message when team members exist but no events", () => {
    render(
      <CalendarWidget
        initialCalendar={createCalendar({
          events: [],
          team_members: sampleTeamMembers,
        })}
      />,
    );
    const messages = screen.getAllByText(
      "No calendar events in this period.",
    );
    expect(messages.length).toBeGreaterThanOrEqual(1);
  });

  it("renders events in agenda view (mobile) with team member info", () => {
    const events = [
      createEvent({
        id: "evt-1",
        summary: "Client Call",
        team_member_name: "Alice Rivera",
        team_member_color: "#3B82F6",
      }),
      createEvent({
        id: "evt-2",
        summary: "Tax Review",
        team_member_name: "Bob Santiago",
        team_member_color: "#10B981",
      }),
    ];

    render(
      <CalendarWidget
        initialCalendar={createCalendar({
          events,
          team_members: sampleTeamMembers,
        })}
      />,
    );

    // Events appear in both mobile agenda and desktop grid
    expect(screen.getAllByText("Client Call").length).toBeGreaterThanOrEqual(1);
    expect(screen.getAllByText("Tax Review").length).toBeGreaterThanOrEqual(1);
    // Team member names in legend + agenda items
    expect(screen.getAllByText("Alice Rivera").length).toBeGreaterThanOrEqual(1);
    expect(screen.getAllByText("Bob Santiago").length).toBeGreaterThanOrEqual(1);
  });

  it("renders color-coded events with team member colors", () => {
    const events = [
      createEvent({
        id: "evt-1",
        summary: "Team Standup",
        team_member_name: "Alice Rivera",
        team_member_color: "#3B82F6",
      }),
    ];

    const { container } = render(
      <CalendarWidget
        initialCalendar={createCalendar({
          events,
          team_members: [{ name: "Alice Rivera", color: "#3B82F6" }],
        })}
      />,
    );

    // Check that the color dot in the legend has the correct color
    const legendDots = container.querySelectorAll(
      '[data-testid="team-legend"] .rounded-full',
    );
    expect(legendDots.length).toBeGreaterThanOrEqual(1);
    expect((legendDots[0] as HTMLElement).style.backgroundColor).toBe(
      "rgb(59, 130, 246)",
    );
  });

  it("renders Today button with correct text", () => {
    render(<CalendarWidget initialCalendar={createCalendar()} />);
    expect(screen.getByText("Today")).toBeInTheDocument();
  });

  it("clicking Previous button does not crash", () => {
    render(
      <CalendarWidget
        initialCalendar={createCalendar({ team_members: sampleTeamMembers })}
      />,
    );
    const prevBtn = screen.getByRole("button", { name: /previous/i });
    fireEvent.click(prevBtn);
    expect(prevBtn).toBeInTheDocument();
  });

  it("clicking Next button does not crash", () => {
    render(
      <CalendarWidget
        initialCalendar={createCalendar({ team_members: sampleTeamMembers })}
      />,
    );
    const nextBtn = screen.getByRole("button", { name: /next/i });
    fireEvent.click(nextBtn);
    expect(nextBtn).toBeInTheDocument();
  });

  it("clicking Today button does not crash", () => {
    render(
      <CalendarWidget
        initialCalendar={createCalendar({ team_members: sampleTeamMembers })}
      />,
    );
    const todayBtn = screen.getByText("Today");
    fireEvent.click(todayBtn);
    expect(todayBtn).toBeInTheDocument();
  });
});
