import { render, screen } from "@testing-library/react";
import { describe, it, expect } from "vitest";

function Placeholder() {
  return <h1>ITDG CRM Platform</h1>;
}

describe("Placeholder", () => {
  it("renders heading", () => {
    render(<Placeholder />);
    expect(screen.getByRole("heading", { level: 1 })).toHaveTextContent(
      "ITDG CRM Platform"
    );
  });
});
