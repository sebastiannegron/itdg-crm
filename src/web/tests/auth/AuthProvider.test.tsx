import { render, screen, waitFor } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";

const mockInitialize = vi.fn().mockResolvedValue(undefined);
const mockGetAllAccounts = vi.fn().mockReturnValue([]);
const mockSetActiveAccount = vi.fn();

vi.mock("@azure/msal-browser", () => ({
  PublicClientApplication: class MockPublicClientApplication {
    initialize = mockInitialize;
    getAllAccounts = mockGetAllAccounts;
    setActiveAccount = mockSetActiveAccount;
  },
}));

vi.mock("@azure/msal-react", () => ({
  MsalProvider: ({
    children,
  }: {
    children: React.ReactNode;
  }) => <div data-testid="msal-provider">{children}</div>,
}));

import AuthProvider from "@/app/_components/AuthProvider";

describe("AuthProvider", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockGetAllAccounts.mockReturnValue([]);
  });

  it("renders children immediately before MSAL initialises", () => {
    render(
      <AuthProvider>
        <div>Test Content</div>
      </AuthProvider>
    );
    expect(screen.getByText("Test Content")).toBeInTheDocument();
  });

  it("wraps children with MsalProvider after initialisation", async () => {
    render(
      <AuthProvider>
        <div>Test Content</div>
      </AuthProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId("msal-provider")).toBeInTheDocument();
    });
    expect(screen.getByText("Test Content")).toBeInTheDocument();
  });

  it("sets active account when accounts exist", async () => {
    const fakeAccount = { homeAccountId: "123", username: "test@example.com" };
    mockGetAllAccounts.mockReturnValue([fakeAccount]);

    render(
      <AuthProvider>
        <div>Content</div>
      </AuthProvider>
    );

    await waitFor(() => {
      expect(mockSetActiveAccount).toHaveBeenCalledWith(fakeAccount);
    });
  });

  it("does not set active account when no accounts", async () => {
    mockGetAllAccounts.mockReturnValue([]);

    render(
      <AuthProvider>
        <div>Content</div>
      </AuthProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId("msal-provider")).toBeInTheDocument();
    });
    expect(mockSetActiveAccount).not.toHaveBeenCalled();
  });
});
