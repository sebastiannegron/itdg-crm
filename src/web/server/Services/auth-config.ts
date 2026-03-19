import type { Configuration } from "@azure/msal-browser";

/**
 * MSAL.js configuration for Microsoft Entra ID authentication.
 *
 * Requires the following environment variables (NEXT_PUBLIC_ prefix
 * because MSAL runs in the browser):
 *
 *   NEXT_PUBLIC_AZURE_AD_CLIENT_ID  – Entra ID application (client) ID
 *   NEXT_PUBLIC_AZURE_AD_TENANT_ID  – Entra ID directory (tenant) ID
 */
export const msalConfig: Configuration = {
  auth: {
    clientId: process.env.NEXT_PUBLIC_AZURE_AD_CLIENT_ID ?? "",
    authority: `https://login.microsoftonline.com/${process.env.NEXT_PUBLIC_AZURE_AD_TENANT_ID ?? "common"}`,
    redirectUri: "/auth/login",
    postLogoutRedirectUri: "/",
    navigateToLoginRequestUrl: false,
  },
  cache: {
    cacheLocation: "sessionStorage",
    storeAuthStateInCookie: false,
  },
};

/**
 * Scopes requested during interactive login.
 */
export const loginRequest = {
  scopes: ["openid", "profile", "email"],
};
