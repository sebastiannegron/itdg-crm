import { render, screen } from "@testing-library/react";
import { describe, it, expect } from "vitest";
import { StatusBadge } from "@/app/_components/StatusBadge";

describe("StatusBadge", () => {
  it("renders the status text", () => {
    render(<StatusBadge status="Active" />);
    expect(screen.getByText("Active")).toBeInTheDocument();
  });

  it("applies green classes for Active status", () => {
    render(<StatusBadge status="Active" />);
    expect(screen.getByText("Active")).toHaveClass("bg-emerald-100");
    expect(screen.getByText("Active")).toHaveClass("text-emerald-800");
  });

  it("applies amber classes for Pending Docs status", () => {
    render(<StatusBadge status="Pending Docs" />);
    expect(screen.getByText("Pending Docs")).toHaveClass("bg-amber-100");
    expect(screen.getByText("Pending Docs")).toHaveClass("text-amber-800");
  });

  it("applies red classes for Awaiting Payment status", () => {
    render(<StatusBadge status="Awaiting Payment" />);
    expect(screen.getByText("Awaiting Payment")).toHaveClass("bg-red-100");
    expect(screen.getByText("Awaiting Payment")).toHaveClass("text-red-800");
  });

  it("applies blue classes for In Progress status", () => {
    render(<StatusBadge status="In Progress" />);
    expect(screen.getByText("In Progress")).toHaveClass("bg-blue-100");
    expect(screen.getByText("In Progress")).toHaveClass("text-blue-800");
  });

  it("applies default gray classes for unknown status", () => {
    render(<StatusBadge status="Unknown Status" />);
    expect(screen.getByText("Unknown Status")).toHaveClass("bg-gray-100");
    expect(screen.getByText("Unknown Status")).toHaveClass("text-gray-800");
  });

  it("is case-insensitive for status matching", () => {
    render(<StatusBadge status="active" />);
    expect(screen.getByText("active")).toHaveClass("bg-emerald-100");
  });

  it("applies custom className", () => {
    render(<StatusBadge status="Active" className="ml-2" />);
    expect(screen.getByText("Active")).toHaveClass("ml-2");
  });
});
