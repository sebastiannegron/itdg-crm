"use client";

import { type ReactNode, useEffect, useState } from "react";
import { PublicClientApplication } from "@azure/msal-browser";
import { MsalProvider } from "@azure/msal-react";
import { msalConfig } from "@/server/Services/auth-config";

/**
 * Initialises MSAL and wraps the component tree with MsalProvider.
 *
 * While MSAL is initialising (client-side only), children render
 * without the provider so the first server-rendered frame is never
 * blocked.
 */
export default function AuthProvider({ children }: { children: ReactNode }) {
  const [msalInstance, setMsalInstance] =
    useState<PublicClientApplication | null>(null);

  useEffect(() => {
    const instance = new PublicClientApplication(msalConfig);
    instance.initialize().then(() => {
      const accounts = instance.getAllAccounts();
      if (accounts.length > 0) {
        instance.setActiveAccount(accounts[0]);
      }
      setMsalInstance(instance);
    });
  }, []);

  if (!msalInstance) {
    return <>{children}</>;
  }

  return <MsalProvider instance={msalInstance}>{children}</MsalProvider>;
}
