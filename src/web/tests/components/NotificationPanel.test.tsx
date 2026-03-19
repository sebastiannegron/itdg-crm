import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi } from "vitest";
import {
  NotificationPanel,
  type NotificationItem,
} from "@/app/_components/NotificationPanel";

const defaultProps = {
  bellLabel: "Notifications",
  title: "Notifications",
  markAllReadLabel: "Mark all read",
  emptyLabel: "No notifications",
  onMarkAllRead: vi.fn(),
};

const sampleNotifications: NotificationItem[] = [
  {
    id: "1",
    type: "document",
    message: "New document uploaded",
    timestamp: "2 min ago",
    read: false,
  },
  {
    id: "2",
    type: "payment",
    message: "Payment received",
    timestamp: "1 hour ago",
    read: true,
  },
  {
    id: "3",
    type: "task",
    message: "Task assigned to you",
    timestamp: "3 hours ago",
    read: false,
  },
];

describe("NotificationPanel", () => {
  it("renders the bell button", () => {
    render(
      <NotificationPanel {...defaultProps} notifications={[]} />
    );
    expect(
      screen.getByRole("button", { name: "Notifications" })
    ).toBeInTheDocument();
  });

  it("does not show badge when there are no unread notifications", () => {
    const readNotifications = sampleNotifications.map((n) => ({
      ...n,
      read: true,
    }));
    const { container } = render(
      <NotificationPanel {...defaultProps} notifications={readNotifications} />
    );
    // Badge should not be present
    const badges = container.querySelectorAll(".rounded-full.px-1");
    expect(badges.length).toBe(0);
  });

  it("shows unread count badge when there are unread notifications", () => {
    render(
      <NotificationPanel
        {...defaultProps}
        notifications={sampleNotifications}
      />
    );
    // 2 unread notifications
    expect(screen.getByText("2")).toBeInTheDocument();
  });

  it("opens panel when bell is clicked", async () => {
    const user = userEvent.setup();
    render(
      <NotificationPanel
        {...defaultProps}
        notifications={sampleNotifications}
      />
    );

    await user.click(
      screen.getByRole("button", { name: "Notifications" })
    );

    expect(
      screen.getByRole("dialog", { name: "Notifications" })
    ).toBeInTheDocument();
  });

  it("closes panel when bell is clicked again", async () => {
    const user = userEvent.setup();
    render(
      <NotificationPanel
        {...defaultProps}
        notifications={sampleNotifications}
      />
    );

    const bell = screen.getByRole("button", { name: "Notifications" });
    await user.click(bell);
    expect(screen.getByRole("dialog")).toBeInTheDocument();

    await user.click(bell);
    expect(screen.queryByRole("dialog")).not.toBeInTheDocument();
  });

  it("closes panel when clicking outside", async () => {
    const user = userEvent.setup();
    const { container } = render(
      <div>
        <div data-testid="outside">Outside</div>
        <NotificationPanel
          {...defaultProps}
          notifications={sampleNotifications}
        />
      </div>
    );

    await user.click(
      screen.getByRole("button", { name: "Notifications" })
    );
    expect(screen.getByRole("dialog")).toBeInTheDocument();

    await user.click(screen.getByTestId("outside"));
    expect(screen.queryByRole("dialog")).not.toBeInTheDocument();
  });

  it("displays notification messages when open", async () => {
    const user = userEvent.setup();
    render(
      <NotificationPanel
        {...defaultProps}
        notifications={sampleNotifications}
      />
    );

    await user.click(
      screen.getByRole("button", { name: "Notifications" })
    );

    expect(screen.getByText("New document uploaded")).toBeInTheDocument();
    expect(screen.getByText("Payment received")).toBeInTheDocument();
    expect(screen.getByText("Task assigned to you")).toBeInTheDocument();
  });

  it("displays timestamps for notifications", async () => {
    const user = userEvent.setup();
    render(
      <NotificationPanel
        {...defaultProps}
        notifications={sampleNotifications}
      />
    );

    await user.click(
      screen.getByRole("button", { name: "Notifications" })
    );

    expect(screen.getByText("2 min ago")).toBeInTheDocument();
    expect(screen.getByText("1 hour ago")).toBeInTheDocument();
    expect(screen.getByText("3 hours ago")).toBeInTheDocument();
  });

  it("shows 'Mark all read' when there are unread notifications", async () => {
    const user = userEvent.setup();
    render(
      <NotificationPanel
        {...defaultProps}
        notifications={sampleNotifications}
      />
    );

    await user.click(
      screen.getByRole("button", { name: "Notifications" })
    );

    expect(screen.getByText("Mark all read")).toBeInTheDocument();
  });

  it("does not show 'Mark all read' when all are read", async () => {
    const user = userEvent.setup();
    const readNotifications = sampleNotifications.map((n) => ({
      ...n,
      read: true,
    }));
    render(
      <NotificationPanel
        {...defaultProps}
        notifications={readNotifications}
      />
    );

    await user.click(
      screen.getByRole("button", { name: "Notifications" })
    );

    expect(screen.queryByText("Mark all read")).not.toBeInTheDocument();
  });

  it("calls onMarkAllRead when 'Mark all read' is clicked", async () => {
    const user = userEvent.setup();
    const onMarkAllRead = vi.fn();
    render(
      <NotificationPanel
        {...defaultProps}
        onMarkAllRead={onMarkAllRead}
        notifications={sampleNotifications}
      />
    );

    await user.click(
      screen.getByRole("button", { name: "Notifications" })
    );
    await user.click(screen.getByText("Mark all read"));

    expect(onMarkAllRead).toHaveBeenCalledTimes(1);
  });

  it("shows empty message when there are no notifications", async () => {
    const user = userEvent.setup();
    render(
      <NotificationPanel {...defaultProps} notifications={[]} />
    );

    await user.click(
      screen.getByRole("button", { name: "Notifications" })
    );

    expect(screen.getByText("No notifications")).toBeInTheDocument();
  });

  it("applies unread background to unread notifications", async () => {
    const user = userEvent.setup();
    render(
      <NotificationPanel
        {...defaultProps}
        notifications={sampleNotifications}
      />
    );

    await user.click(
      screen.getByRole("button", { name: "Notifications" })
    );

    const list = screen.getByRole("list");
    const items = list.querySelectorAll("li");
    // First item is unread
    expect(items[0]).toHaveClass("bg-[#FFF8F5]");
    // Second item is read
    expect(items[1]).not.toHaveClass("bg-[#FFF8F5]");
    // Third item is unread
    expect(items[2]).toHaveClass("bg-[#FFF8F5]");
  });

  it("renders unread indicator dots for unread notifications", async () => {
    const user = userEvent.setup();
    render(
      <NotificationPanel
        {...defaultProps}
        notifications={sampleNotifications}
      />
    );

    await user.click(
      screen.getByRole("button", { name: "Notifications" })
    );

    const unreadDots = screen.getAllByLabelText("Unread");
    // 2 unread notifications
    expect(unreadDots).toHaveLength(2);
  });

  it("sets aria-expanded on bell button", async () => {
    const user = userEvent.setup();
    render(
      <NotificationPanel {...defaultProps} notifications={[]} />
    );

    const bell = screen.getByRole("button", { name: "Notifications" });
    expect(bell).toHaveAttribute("aria-expanded", "false");

    await user.click(bell);
    expect(bell).toHaveAttribute("aria-expanded", "true");
  });

  it("renders notification panel title in header", async () => {
    const user = userEvent.setup();
    render(
      <NotificationPanel {...defaultProps} notifications={[]} />
    );

    await user.click(
      screen.getByRole("button", { name: "Notifications" })
    );

    expect(
      screen.getByRole("heading", { name: "Notifications" })
    ).toBeInTheDocument();
  });
});
