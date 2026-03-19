import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { fieldnames } from "@/app/[locale]/_shared/app-fieldnames";

const mockPathname = vi.fn().mockReturnValue("/portal");

vi.mock("next-intl", () => ({
  useLocale: () => "en-pr",
}));

vi.mock("@/i18n/routing", () => ({
  Link: ({
    href,
    children,
    ...props
  }: {
    href: string;
    children: React.ReactNode;
    onClick?: () => void;
    className?: string;
  }) => (
    <a href={href} {...props}>
      {children}
    </a>
  ),
  usePathname: () => mockPathname(),
}));

import PortalHeader from "@/app/[locale]/(portal)/PortalHeader";

describe("PortalHeader", () => {
  beforeEach(() => {
    mockPathname.mockReturnValue("/portal");
  });

  it("renders tenant logo placeholder and portal name", () => {
    render(<PortalHeader />);
    expect(screen.getByText("Client Portal")).toBeInTheDocument();
    expect(screen.getByText("T")).toBeInTheDocument();
  });

  it("renders navigation links for Messages, Documents, and Payments", () => {
    render(<PortalHeader />);
    const messages = screen.getAllByText("Messages");
    expect(messages.length).toBeGreaterThanOrEqual(1);
    const documents = screen.getAllByText("Documents");
    expect(documents.length).toBeGreaterThanOrEqual(1);
    const payments = screen.getAllByText("Payments");
    expect(payments.length).toBeGreaterThanOrEqual(1);
  });

  it("renders mobile menu toggle button", () => {
    render(<PortalHeader />);
    const buttons = screen.getAllByRole("button", { name: "Open menu" });
    expect(buttons.length).toBeGreaterThanOrEqual(1);
  });

  it("toggles mobile menu on button click", async () => {
    const user = userEvent.setup();
    render(<PortalHeader />);

    const openButton = screen.getAllByRole("button", { name: "Open menu" })[0];
    await user.click(openButton);

    const closeButton = screen.getAllByRole("button", {
      name: "Close menu",
    })[0];
    expect(closeButton).toBeInTheDocument();

    await user.click(closeButton);

    const openButtons = screen.getAllByRole("button", { name: "Open menu" });
    expect(openButtons.length).toBeGreaterThanOrEqual(1);
  });

  it("applies active styles to current navigation link", () => {
    mockPathname.mockReturnValue("/portal/messages");
    const { container } = render(<PortalHeader />);

    const messagesLink = container.querySelector(
      'a[href="/portal/messages"]'
    );
    expect(messagesLink).not.toBeNull();
    expect(messagesLink?.className).toContain("bg-primary");
  });

  it("includes correct portal fieldnames for both locales", () => {
    expect(fieldnames["en-pr"].portal_nav_messages).toBe("Messages");
    expect(fieldnames["en-pr"].portal_nav_documents).toBe("Documents");
    expect(fieldnames["en-pr"].portal_nav_payments).toBe("Payments");
    expect(fieldnames["en-pr"].portal_name).toBe("Client Portal");
    expect(fieldnames["es-pr"].portal_nav_messages).toBe("Mensajes");
    expect(fieldnames["es-pr"].portal_nav_documents).toBe("Documentos");
    expect(fieldnames["es-pr"].portal_nav_payments).toBe("Pagos");
    expect(fieldnames["es-pr"].portal_name).toBe("Portal del Cliente");
  });
});
