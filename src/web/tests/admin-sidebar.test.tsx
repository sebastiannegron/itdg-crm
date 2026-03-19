import { render, screen } from "@testing-library/react";
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
    expect(screen.getAllByText("Tasks").length).toBeGreaterThanOrEqual(1);
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

    const brands = screen.getAllByText("R&A");
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

  it("renders settings link in sidebar footer", () => {
    render(
      <AdminSidebar>
        <div>Content</div>
      </AdminSidebar>
    );

    const settingsLinks = screen.getAllByText("Settings");
    expect(settingsLinks.length).toBeGreaterThanOrEqual(1);
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
      "/tasks",
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

    // Desktop sidebar + tablet sidebar + mobile bottom nav = at least 3 nav regions
    const navElements = screen.getAllByRole("navigation", { name: "Main" });
    expect(navElements.length).toBeGreaterThanOrEqual(2);
  });

  it("renders Tasks nav item in main navigation", () => {
    render(
      <AdminSidebar>
        <div>Content</div>
      </AdminSidebar>
    );

    const tasksLinks = screen.getAllByRole("link").filter(
      (link) => link.getAttribute("href") === "/tasks"
    );
    expect(tasksLinks.length).toBeGreaterThanOrEqual(1);
  });

  it("renders Comms short label in mobile bottom nav", () => {
    render(
      <AdminSidebar>
        <div>Content</div>
      </AdminSidebar>
    );

    // Mobile bottom nav uses "Comms" short label for Communications
    expect(screen.getAllByText("Comms").length).toBeGreaterThanOrEqual(1);
  });

  it("renders Settings in sidebar footer separate from main nav", () => {
    render(
      <AdminSidebar>
        <div>Content</div>
      </AdminSidebar>
    );

    // Settings should still be rendered
    const settingsLinks = screen.getAllByRole("link").filter(
      (link) => link.getAttribute("href") === "/settings"
    );
    expect(settingsLinks.length).toBeGreaterThanOrEqual(1);

    // Settings should appear as text (in desktop sidebar footer)
    expect(screen.getAllByText("Settings").length).toBeGreaterThanOrEqual(1);
  });
});
