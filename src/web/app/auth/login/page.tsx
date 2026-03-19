"use client";

import { useEffect, useRef, useState } from "react";
import { PublicClientApplication } from "@azure/msal-browser";
import {
  msalConfig,
  loginRequest,
  AUTH_COOKIE_SET,
} from "@/server/Services/auth-config";

const DEFAULT_REDIRECT = "/en-pr";

/**
 * Login page — handles the MSAL redirect flow.
 *
 * 1. On mount, calls `handleRedirectPromise` to process a returning
 *    redirect from Microsoft Entra ID.
 * 2. If a valid account is found, sets the `msal-authenticated` cookie
 *    (read by middleware) and redirects to the return URL.
 * 3. Otherwise, renders a "Sign in with Microsoft" button that starts
 *    the redirect flow.
 */
export default function LoginPage() {
  const msalRef = useRef<PublicClientApplication | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const instance = new PublicClientApplication(msalConfig);
    instance.initialize().then(async () => {
      msalRef.current = instance;

      try {
        const response = await instance.handleRedirectPromise();

        if (response?.account) {
          instance.setActiveAccount(response.account);
          document.cookie = AUTH_COOKIE_SET;
          const returnUrl =
            sessionStorage.getItem("login-return-url") ?? DEFAULT_REDIRECT;
          sessionStorage.removeItem("login-return-url");
          window.location.href = returnUrl;
          return;
        }

        const accounts = instance.getAllAccounts();
        if (accounts.length > 0) {
          document.cookie = AUTH_COOKIE_SET;
          window.location.href = DEFAULT_REDIRECT;
          return;
        }
      } catch (err) {
        console.error("MSAL authentication error:", err);
        setError("Authentication failed. Please try again.");
      }

      setIsLoading(false);
    });
  }, []);

  const handleLogin = () => {
    if (!msalRef.current) return;

    const params = new URLSearchParams(window.location.search);
    const returnUrl = params.get("returnUrl");
    if (returnUrl) {
      sessionStorage.setItem("login-return-url", returnUrl);
    }

    msalRef.current.loginRedirect(loginRequest);
  };

  if (isLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-background">
        <p className="text-sm text-muted-foreground">Loading…</p>
      </div>
    );
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-background">
      <div className="w-full max-w-sm rounded-lg border border-border bg-card p-8 text-center shadow-sm">
        <h1 className="text-2xl font-semibold text-primary">R&A CRM</h1>
        <p className="mt-1 text-xs uppercase tracking-wider text-muted-foreground">
          Tax Consulting
        </p>

        {error && (
          <p className="mt-4 text-sm text-destructive" role="alert">
            {error}
          </p>
        )}

        <button
          type="button"
          onClick={handleLogin}
          className="mt-6 inline-flex w-full items-center justify-center rounded-md bg-primary px-4 py-2.5 text-sm font-medium text-primary-foreground transition-colors hover:bg-primary/90"
        >
          Sign in with Microsoft
        </button>
      </div>
    </div>
  );
}
