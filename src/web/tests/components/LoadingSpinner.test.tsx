import { render, screen } from "@testing-library/react";
import { describe, it, expect } from "vitest";
import { LoadingSpinner } from "@/app/_components/LoadingSpinner";

describe("LoadingSpinner", () => {
  it("renders with default aria label", () => {
    render(<LoadingSpinner />);
    expect(screen.getByRole("status")).toHaveAttribute(
      "aria-label",
      "Loading"
    );
  });

  it("renders with custom message", () => {
    render(<LoadingSpinner message="Loading clients..." />);
    expect(screen.getByText("Loading clients...")).toBeInTheDocument();
    expect(screen.getByRole("status")).toHaveAttribute(
      "aria-label",
      "Loading clients..."
    );
  });

  it("applies custom className", () => {
    render(<LoadingSpinner className="mt-8" />);
    expect(screen.getByRole("status")).toHaveClass("mt-8");
  });
});
