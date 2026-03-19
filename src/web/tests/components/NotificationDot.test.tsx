import { render, screen } from "@testing-library/react";
import { describe, it, expect } from "vitest";
import { NotificationDot } from "@/app/_components/NotificationDot";

describe("NotificationDot", () => {
  it("renders a doc notification dot with blue color", () => {
    render(<NotificationDot type="doc" />);
    const dot = screen.getByRole("status");
    expect(dot).toHaveClass("bg-blue-500");
  });

  it("renders an alert notification dot with red color", () => {
    render(<NotificationDot type="alert" />);
    const dot = screen.getByRole("status");
    expect(dot).toHaveClass("bg-red-500");
  });

  it("renders a task notification dot with green color", () => {
    render(<NotificationDot type="task" />);
    const dot = screen.getByRole("status");
    expect(dot).toHaveClass("bg-emerald-500");
  });

  it("renders a msg notification dot with purple color", () => {
    render(<NotificationDot type="msg" />);
    const dot = screen.getByRole("status");
    expect(dot).toHaveClass("bg-purple-500");
  });

  it("renders as 8px dot (h-2 w-2)", () => {
    render(<NotificationDot type="doc" />);
    const dot = screen.getByRole("status");
    expect(dot).toHaveClass("h-2", "w-2", "rounded-full");
  });

  it("has accessible aria-label", () => {
    render(<NotificationDot type="alert" />);
    expect(screen.getByRole("status")).toHaveAttribute(
      "aria-label",
      "alert notification"
    );
  });

  it("applies custom className", () => {
    render(<NotificationDot type="doc" className="ml-1" />);
    expect(screen.getByRole("status")).toHaveClass("ml-1");
  });
});
