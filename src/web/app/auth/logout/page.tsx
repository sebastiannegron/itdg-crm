"use client";

import { useEffect } from "react";
import { PublicClientApplication } from "@azure/msal-browser";
import { msalConfig } from "@/server/Services/auth-config";

/**
 * Logout page — clears the MSAL session and auth cookie, then
 * redirects to the post-logout URI configured in auth-config.
 */
export default function LogoutPage() {
  useEffect(() => {
    document.cookie =
      "msal-authenticated=; path=/; max-age=0; SameSite=Lax";

    const instance = new PublicClientApplication(msalConfig);
    instance.initialize().then(() => {
      const account = instance.getAllAccounts()[0];
      instance.logoutRedirect({
        account: account ?? undefined,
        postLogoutRedirectUri: msalConfig.auth.postLogoutRedirectUri,
      });
    });
  }, []);

  return (
    <div className="flex min-h-screen items-center justify-center bg-background">
      <p className="text-sm text-muted-foreground">Signing out…</p>
    </div>
  );
}
