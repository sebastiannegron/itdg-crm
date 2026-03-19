import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi, beforeEach } from "vitest";

// Mock next-intl
vi.mock("next-intl", () => ({
  useLocale: () => "en-pr",
}));

// Mock i18n routing
const mockPathname = vi.fn(() => "/dashboard");
vi.mock("@/i18n/routing", () => ({
  usePathname: () => mockPathname(),
  Link: ({
    href,
    children,
    className,
    onClick,
    ...props
  }: {
    href: string;
    children: React.ReactNode;
    className?: string;
    onClick?: () => void;
    [key: string]: unknown;
  }) => (
    <a href={href} className={className} onClick={onClick} {...props}>
      {children}
    </a>
  ),
}));

import AdminSidebar from "@/app/[locale]/(admin)/AdminSidebar";

describe("AdminSidebar", () => {
  beforeEach(() => {
    mockPathname.mockReturnValue("/dashboard");
  });

  it("renders all navigation links", () => {
    render(
      <AdminSidebar>
        <div>Page content</div>
      </AdminSidebar>
    );

    // Desktop sidebar + mobile bottom nav both render nav items
    expect(screen.getAllByText("Dashboard").length).toBeGreaterThanOrEqual(1);
    expect(screen.getAllByText("Clients").length).toBeGreaterThanOrEqual(1);
    expect(screen.getAllByText("Documents").length).toBeGreaterThanOrEqual(1);
    expect(
      screen.getAllByText("Communications").length
    ).toBeGreaterThanOrEqual(1);
    expect(screen.getAllByText("Settings").length).toBeGreaterThanOrEqual(1);
  });

  it("renders the children content", () => {
    render(
      <AdminSidebar>
        <div>Test page content</div>
      </AdminSidebar>
    );

    expect(screen.getByText("Test page content")).toBeInTheDocument();
  });

  it("renders the app brand name", () => {
    render(
      <AdminSidebar>
        <div>Content</div>
      </AdminSidebar>
    );

    const brands = screen.getAllByText("ITDG");
    expect(brands.length).toBeGreaterThanOrEqual(1);
  });

  it("renders the notification bell button", () => {
    render(
      <AdminSidebar>
        <div>Content</div>
      </AdminSidebar>
    );

    const bellButtons = screen.getAllByRole("button", {
      name: "Notifications",
    });
    expect(bellButtons.length).toBeGreaterThanOrEqual(1);
  });

  it("renders collapse sidebar button", () => {
    render(
      <AdminSidebar>
        <div>Content</div>
      </AdminSidebar>
    );

    const collapseButtons = screen.getAllByRole("button", {
      name: "Collapse sidebar",
    });
    expect(collapseButtons.length).toBeGreaterThanOrEqual(1);
  });

  it("toggles sidebar collapse state on button click", async () => {
    const user = userEvent.setup();
    render(
      <AdminSidebar>
        <div>Content</div>
      </AdminSidebar>
    );

    // Initially should show "Collapse sidebar"
    const collapseButtons = screen.getAllByRole("button", {
      name: "Collapse sidebar",
    });
    expect(collapseButtons.length).toBeGreaterThanOrEqual(1);

    // Click to collapse
    await user.click(collapseButtons[0]);

    // Should now show "Expand sidebar"
    const expandButtons = screen.getAllByRole("button", {
      name: "Expand sidebar",
    });
    expect(expandButtons.length).toBeGreaterThanOrEqual(1);
  });

  it("marks the active navigation link with aria-current", () => {
    mockPathname.mockReturnValue("/clients");
    render(
      <AdminSidebar>
        <div>Content</div>
      </AdminSidebar>
    );

    // Find links with aria-current="page"
    const activeLinks = screen.getAllByRole("link", { current: "page" });
    expect(activeLinks.length).toBeGreaterThanOrEqual(1);

    // At least one active link should point to /clients
    const clientLinks = activeLinks.filter(
      (link) => link.getAttribute("href") === "/clients"
    );
    expect(clientLinks.length).toBeGreaterThanOrEqual(1);
  });

  it("renders hamburger menu button for tablet view", () => {
    render(
      <AdminSidebar>
        <div>Content</div>
      </AdminSidebar>
    );

    const menuButtons = screen.getAllByRole("button", { name: "Open menu" });
    expect(menuButtons.length).toBeGreaterThanOrEqual(1);
  });

  it("renders a sticky header", () => {
    const { container } = render(
      <AdminSidebar>
        <div>Content</div>
      </AdminSidebar>
    );

    const header = container.querySelector("header");
    expect(header).not.toBeNull();
    expect(header!.classList.contains("sticky")).toBe(true);
  });

  it("renders navigation links with correct hrefs", () => {
    render(
      <AdminSidebar>
        <div>Content</div>
      </AdminSidebar>
    );

    const expectedHrefs = [
      "/dashboard",
      "/clients",
      "/documents",
      "/communications",
      "/settings",
    ];

    const allLinks = screen.getAllByRole("link");
    for (const href of expectedHrefs) {
      const matchingLinks = allLinks.filter(
        (link) => link.getAttribute("href") === href
      );
      expect(matchingLinks.length).toBeGreaterThanOrEqual(1);
    }
  });

  it("renders multiple navigation regions", () => {
    render(
      <AdminSidebar>
        <div>Content</div>
      </AdminSidebar>
    );

    // Desktop sidebar + mobile bottom nav = at least 2 nav regions
    const navElements = screen.getAllByRole("navigation", { name: "Main" });
    expect(navElements.length).toBeGreaterThanOrEqual(2);
  });
});

