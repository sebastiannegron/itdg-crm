import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";

vi.mock("next-intl", () => ({
  useLocale: () => "en-pr",
}));

vi.mock("@/i18n/routing", () => ({
  useRouter: () => ({
    push: vi.fn(),
  }),
  usePathname: () => "/settings/integrations",
  Link: ({
    href,
    children,
    ...props
  }: {
    href: string;
    children: React.ReactNode;
  }) => (
    <a href={href} {...props}>
      {children}
    </a>
  ),
}));

vi.mock(
  "@/app/[locale]/(admin)/settings/integrations/actions",
  () => ({
    getGoogleConnectionStatusAction: vi.fn().mockResolvedValue({
      success: true,
      data: { is_connected: false, connected_at: null },
    }),
    disconnectGoogleAction: vi.fn().mockResolvedValue({
      success: true,
      message: "Disconnected",
    }),
    getGmailConnectionStatusAction: vi.fn().mockResolvedValue({
      success: true,
      data: { is_connected: false, connected_at: null },
    }),
    disconnectGmailAction: vi.fn().mockResolvedValue({
      success: true,
      message: "Disconnected",
    }),
    getCalendarConnectionStatusAction: vi.fn().mockResolvedValue({
      success: true,
      data: { is_connected: false, connected_at: null },
    }),
    getMsGraphConnectionStatusAction: vi.fn().mockResolvedValue({
      success: true,
      data: { is_configured: false },
    }),
    getAzureOpenAiConnectionStatusAction: vi.fn().mockResolvedValue({
      success: true,
      data: { is_configured: false },
    }),
  }),
);

vi.mock("@/server/Services/integrationService", () => ({
  getGoogleAuthUrl: () => "http://localhost:5000/api/v1/Integrations/Google/Auth",
  getGmailAuthUrl: () => "http://localhost:5000/api/v1/Integrations/Gmail/Auth",
}));

import IntegrationsView from "@/app/[locale]/(admin)/settings/integrations/IntegrationsView";

const defaultProps = {
  initialGoogleDrive: { is_connected: false, connected_at: null },
  initialGmail: { is_connected: false, connected_at: null },
  initialCalendar: { is_connected: false, connected_at: null },
  initialMsGraph: { is_configured: false },
  initialAzureOpenAi: { is_configured: false },
};

const connectedProps = {
  initialGoogleDrive: {
    is_connected: true,
    connected_at: "2025-06-15T10:30:00Z",
  },
  initialGmail: {
    is_connected: true,
    connected_at: "2025-06-14T09:00:00Z",
  },
  initialCalendar: {
    is_connected: true,
    connected_at: "2025-06-13T08:00:00Z",
  },
  initialMsGraph: { is_configured: true },
  initialAzureOpenAi: { is_configured: true },
};

describe("IntegrationsView", () => {
  it("renders the page header with Integrations title", () => {
    render(<IntegrationsView {...defaultProps} />);
    expect(
      screen.getByRole("heading", { level: 1 }),
    ).toHaveTextContent("Integrations");
  });

  it("renders breadcrumbs with Settings and Integrations", () => {
    render(<IntegrationsView {...defaultProps} />);
    expect(screen.getByText("Settings")).toBeInTheDocument();
    expect(
      screen.getByRole("heading", { level: 1 }),
    ).toHaveTextContent("Integrations");
  });

  it("renders Google Workspace section heading", () => {
    render(<IntegrationsView {...defaultProps} />);
    expect(screen.getByText("Google Workspace")).toBeInTheDocument();
  });

  it("renders Gmail card with not connected status", () => {
    render(<IntegrationsView {...defaultProps} />);
    expect(screen.getByText("Gmail")).toBeInTheDocument();
    const notConnectedBadges = screen.getAllByText("Not connected");
    expect(notConnectedBadges.length).toBeGreaterThanOrEqual(1);
  });

  it("renders Google Calendar card", () => {
    render(<IntegrationsView {...defaultProps} />);
    expect(screen.getByText("Google Calendar")).toBeInTheDocument();
  });

  it("renders Google Drive card", () => {
    render(<IntegrationsView {...defaultProps} />);
    expect(screen.getByText("Google Drive")).toBeInTheDocument();
  });

  it("renders Microsoft Graph card", () => {
    render(<IntegrationsView {...defaultProps} />);
    expect(screen.getByText("Microsoft Graph")).toBeInTheDocument();
  });

  it("renders Azure OpenAI card", () => {
    render(<IntegrationsView {...defaultProps} />);
    expect(screen.getByText("Azure OpenAI")).toBeInTheDocument();
  });

  it("shows Connect buttons when services are not connected", () => {
    render(<IntegrationsView {...defaultProps} />);
    const connectButtons = screen.getAllByRole("button", {
      name: /Connect/i,
    });
    expect(connectButtons.length).toBe(2);
  });

  it("shows connected status when services are connected", () => {
    render(<IntegrationsView {...connectedProps} />);
    const connectedBadges = screen.getAllByText("Connected");
    expect(connectedBadges.length).toBe(3);
  });

  it("shows Disconnect buttons when OAuth services are connected", () => {
    render(<IntegrationsView {...connectedProps} />);
    const disconnectButtons = screen.getAllByRole("button", {
      name: /Disconnect/i,
    });
    expect(disconnectButtons.length).toBe(2);
  });

  it("shows connected date for Gmail when connected", () => {
    render(<IntegrationsView {...connectedProps} />);
    const connectedAtTexts = screen.getAllByText(/Connected on/);
    expect(connectedAtTexts.length).toBeGreaterThanOrEqual(1);
  });

  it("shows configured status for system services when configured", () => {
    render(<IntegrationsView {...connectedProps} />);
    const configuredBadges = screen.getAllByText("Configured");
    expect(configuredBadges.length).toBe(2);
  });

  it("shows not configured status for system services when not configured", () => {
    render(<IntegrationsView {...defaultProps} />);
    const notConfiguredBadges = screen.getAllByText("Not configured");
    expect(notConfiguredBadges.length).toBe(2);
  });

  it("renders subtitle description", () => {
    render(<IntegrationsView {...defaultProps} />);
    expect(
      screen.getByText(
        "Manage third-party service connections for your organization.",
      ),
    ).toBeInTheDocument();
  });

  it("renders system service section", () => {
    render(<IntegrationsView {...defaultProps} />);
    expect(screen.getByText("System service")).toBeInTheDocument();
  });
});
