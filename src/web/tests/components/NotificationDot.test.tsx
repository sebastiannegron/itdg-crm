import { render, screen } from "@testing-library/react";
import { describe, it, expect } from "vitest";
import {
  NotificationDot,
  type NotificationType,
} from "@/app/_components/NotificationDot";

describe("NotificationDot", () => {
  const types: NotificationType[] = [
    "document",
    "payment",
    "task",
    "escalation",
    "message",
    "system",
  ];

  it.each(types)("renders a dot for type '%s'", (type) => {
    const { container } = render(<NotificationDot type={type} />);
    const dot = container.querySelector("span");
    expect(dot).toBeInTheDocument();
    expect(dot).toHaveClass("rounded-full");
  });

  it("applies correct color class for document type", () => {
    const { container } = render(<NotificationDot type="document" />);
    const dot = container.querySelector("span");
    expect(dot).toHaveClass("bg-primary");
  });

  it("applies correct color class for payment type", () => {
    const { container } = render(<NotificationDot type="payment" />);
    const dot = container.querySelector("span");
    expect(dot).toHaveClass("bg-accent");
  });

  it("applies correct color class for task type", () => {
    const { container } = render(<NotificationDot type="task" />);
    const dot = container.querySelector("span");
    expect(dot).toHaveClass("bg-warning");
  });

  it("applies correct color class for escalation type", () => {
    const { container } = render(<NotificationDot type="escalation" />);
    const dot = container.querySelector("span");
    expect(dot).toHaveClass("bg-destructive");
  });

  it("applies correct color class for system type", () => {
    const { container } = render(<NotificationDot type="system" />);
    const dot = container.querySelector("span");
    expect(dot).toHaveClass("bg-muted-foreground");
  });

  it("is hidden from assistive technology", () => {
    const { container } = render(<NotificationDot type="document" />);
    const dot = container.querySelector("span");
    expect(dot).toHaveAttribute("aria-hidden", "true");
  });

  it("accepts custom className", () => {
    const { container } = render(
      <NotificationDot type="document" className="h-4 w-4" />
    );
    const dot = container.querySelector("span");
    expect(dot).toHaveClass("h-4", "w-4");
  });
});
