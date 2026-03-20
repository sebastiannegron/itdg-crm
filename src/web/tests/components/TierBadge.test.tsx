import { render, screen } from "@testing-library/react";
import { describe, it, expect } from "vitest";
import { TierBadge } from "@/app/_components/TierBadge";

describe("TierBadge", () => {
  it("renders Tier 1 label", () => {
    render(<TierBadge tier={1} />);
    expect(screen.getByText("Tier 1")).toBeInTheDocument();
  });

  it("renders Tier 2 label", () => {
    render(<TierBadge tier={2} />);
    expect(screen.getByText("Tier 2")).toBeInTheDocument();
  });

  it("renders Tier 3 label", () => {
    render(<TierBadge tier={3} />);
    expect(screen.getByText("Tier 3")).toBeInTheDocument();
  });

  it("applies tier-specific color class for Tier 1", () => {
    render(<TierBadge tier={1} />);
    expect(screen.getByText("Tier 1")).toHaveClass("bg-tier-1-bg");
  });

  it("applies tier-specific color class for Tier 2", () => {
    render(<TierBadge tier={2} />);
    expect(screen.getByText("Tier 2")).toHaveClass("bg-tier-2-bg");
  });

  it("applies tier-specific color class for Tier 3", () => {
    render(<TierBadge tier={3} />);
    expect(screen.getByText("Tier 3")).toHaveClass("bg-tier-3-bg");
  });

  it("applies custom className", () => {
    render(<TierBadge tier={1} className="ml-2" />);
    expect(screen.getByText("Tier 1")).toHaveClass("ml-2");
  });
});
