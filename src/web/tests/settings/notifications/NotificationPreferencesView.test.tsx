import { render, screen, fireEvent } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";

vi.mock("next-intl", () => ({
  useLocale: () => "en-pr",
}));

vi.mock("@/i18n/routing", () => ({
  useRouter: () => ({
    push: vi.fn(),
  }),
  usePathname: () => "/settings/notifications",
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
  "@/app/[locale]/(admin)/settings/notifications/actions",
  () => ({
    updateNotificationPreferencesAction: vi.fn().mockResolvedValue({
      success: true,
      message: "Preferences updated successfully",
    }),
  }),
);

import NotificationPreferencesView from "@/app/[locale]/(admin)/settings/notifications/NotificationPreferencesView";
import type { NotificationPreferenceDto } from "@/app/[locale]/(admin)/settings/notifications/shared";

const samplePreferences: NotificationPreferenceDto[] = [
  {
    preference_id: "pref-1",
    event_type: "TaskAssigned",
    channel: "InApp",
    is_enabled: true,
    digest_mode: "Immediate",
  },
  {
    preference_id: "pref-2",
    event_type: "TaskAssigned",
    channel: "Email",
    is_enabled: false,
    digest_mode: "Immediate",
  },
  {
    preference_id: "pref-3",
    event_type: "DocumentUploaded",
    channel: "InApp",
    is_enabled: true,
    digest_mode: "Immediate",
  },
  {
    preference_id: "pref-4",
    event_type: "DocumentUploaded",
    channel: "Email",
    is_enabled: true,
    digest_mode: "Immediate",
  },
];

describe("NotificationPreferencesView", () => {
  it("renders the page header with Notification Preferences title", () => {
    render(
      <NotificationPreferencesView initialPreferences={samplePreferences} />,
    );
    expect(
      screen.getByRole("heading", { level: 1 }),
    ).toHaveTextContent("Notification Preferences");
  });

  it("renders breadcrumbs with Settings link", () => {
    render(
      <NotificationPreferencesView initialPreferences={samplePreferences} />,
    );
    expect(screen.getByText("Settings")).toBeInTheDocument();
  });

  it("renders all event type rows", () => {
    render(
      <NotificationPreferencesView initialPreferences={samplePreferences} />,
    );
    expect(screen.getByText("Document Uploaded")).toBeInTheDocument();
    expect(screen.getByText("Payment Completed")).toBeInTheDocument();
    expect(screen.getByText("Payment Failed")).toBeInTheDocument();
    expect(screen.getByText("Task Assigned")).toBeInTheDocument();
    expect(screen.getByText("Task Due Soon")).toBeInTheDocument();
    expect(screen.getByText("Escalation Received")).toBeInTheDocument();
    expect(screen.getByText("Portal Message Received")).toBeInTheDocument();
    expect(screen.getByText("System Alert")).toBeInTheDocument();
  });

  it("renders column headers for In-App and Email", () => {
    render(
      <NotificationPreferencesView initialPreferences={samplePreferences} />,
    );
    expect(screen.getByText("In-App")).toBeInTheDocument();
    expect(screen.getByText("Email")).toBeInTheDocument();
  });

  it("renders toggle switches for each event type and channel", () => {
    render(
      <NotificationPreferencesView initialPreferences={samplePreferences} />,
    );
    const switches = screen.getAllByRole("switch");
    // 8 event types × 2 channels = 16 switches
    expect(switches).toHaveLength(16);
  });

  it("renders save button", () => {
    render(
      <NotificationPreferencesView initialPreferences={samplePreferences} />,
    );
    expect(
      screen.getByRole("button", { name: /Save Preferences/i }),
    ).toBeInTheDocument();
  });

  it("reflects initial preference state in switches", () => {
    render(
      <NotificationPreferencesView initialPreferences={samplePreferences} />,
    );
    // TaskAssigned In-App should be checked (true)
    const taskInApp = screen.getByLabelText("Task Assigned In-App");
    expect(taskInApp).toHaveAttribute("aria-checked", "true");

    // TaskAssigned Email should be unchecked (false)
    const taskEmail = screen.getByLabelText("Task Assigned Email");
    expect(taskEmail).toHaveAttribute("aria-checked", "false");
  });

  it("toggles switch when clicked", () => {
    render(
      <NotificationPreferencesView initialPreferences={samplePreferences} />,
    );
    const taskEmail = screen.getByLabelText("Task Assigned Email");
    expect(taskEmail).toHaveAttribute("aria-checked", "false");

    fireEvent.click(taskEmail);
    expect(taskEmail).toHaveAttribute("aria-checked", "true");
  });

  it("defaults to enabled when no preferences exist for an event type", () => {
    render(<NotificationPreferencesView initialPreferences={[]} />);
    // With empty preferences, all should default to enabled (true)
    const switches = screen.getAllByRole("switch");
    switches.forEach((switchEl) => {
      expect(switchEl).toHaveAttribute("aria-checked", "true");
    });
  });

  it("renders subtitle description", () => {
    render(
      <NotificationPreferencesView initialPreferences={samplePreferences} />,
    );
    expect(
      screen.getByText("Control how and when you receive notifications"),
    ).toBeInTheDocument();
  });
});
