"use client";

import { useEffect, useState } from "react";
import { PublicClientApplication } from "@azure/msal-browser";
import { msalConfig, AUTH_COOKIE_CLEAR } from "@/server/Services/auth-config";

/**
 * Logout page — clears the MSAL session and auth cookie, then
 * redirects to the post-logout URI configured in auth-config.
 */
export default function LogoutPage() {
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    document.cookie = AUTH_COOKIE_CLEAR;

    const instance = new PublicClientApplication(msalConfig);
    instance
      .initialize()
      .then(() => {
        const account = instance.getAllAccounts()[0];
        return instance.logoutRedirect({
          account: account ?? undefined,
          postLogoutRedirectUri: msalConfig.auth.postLogoutRedirectUri,
        });
      })
      .catch((err) => {
        console.error("MSAL logout error:", err);
        setError("Sign-out failed. You may close this window.");
      });
  }, []);

  return (
    <div className="flex min-h-screen items-center justify-center bg-background">
      {error ? (
        <p className="text-sm text-destructive" role="alert">
          {error}
        </p>
      ) : (
        <p className="text-sm text-muted-foreground">Signing out…</p>
      )}
    </div>
  );
}
