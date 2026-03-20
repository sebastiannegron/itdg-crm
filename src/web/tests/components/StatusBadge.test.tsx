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
    expect(screen.getByText("Active")).toHaveClass("bg-[#ECFDF5]");
    expect(screen.getByText("Active")).toHaveClass("text-[#065F46]");
  });

  it("applies amber classes for Pending Docs status", () => {
    render(<StatusBadge status="Pending Docs" />);
    expect(screen.getByText("Pending Docs")).toHaveClass("bg-[#FFFBEB]");
    expect(screen.getByText("Pending Docs")).toHaveClass("text-[#92400E]");
  });

  it("applies red classes for Awaiting Payment status", () => {
    render(<StatusBadge status="Awaiting Payment" />);
    expect(screen.getByText("Awaiting Payment")).toHaveClass("bg-[#FEF2F2]");
    expect(screen.getByText("Awaiting Payment")).toHaveClass("text-[#991B1B]");
  });

  it("applies blue classes for In Progress status", () => {
    render(<StatusBadge status="In Progress" />);
    expect(screen.getByText("In Progress")).toHaveClass("bg-[#EFF6FF]");
    expect(screen.getByText("In Progress")).toHaveClass("text-[#1E40AF]");
  });

  it("applies default classes for unknown status", () => {
    render(<StatusBadge status="Unknown Status" />);
    expect(screen.getByText("Unknown Status")).toHaveClass("bg-[#F9FAFB]");
    expect(screen.getByText("Unknown Status")).toHaveClass("text-[#374151]");
  });

  it("falls back to default for unrecognized status", () => {
    render(<StatusBadge status="active" />);
    // Lowercase "active" doesn't match "Active" key, falls back to default
    expect(screen.getByText("active")).toHaveClass("bg-[#F9FAFB]");
  });

  it("applies custom className", () => {
    render(<StatusBadge status="Active" className="ml-2" />);
    expect(screen.getByText("Active")).toHaveClass("ml-2");
  });
});
