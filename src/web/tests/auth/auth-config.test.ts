import { describe, it, expect } from "vitest";
import { msalConfig, loginRequest } from "@/server/Services/auth-config";

describe("auth-config", () => {
  it("exports msalConfig with required auth properties", () => {
    expect(msalConfig.auth).toBeDefined();
    expect(msalConfig.auth.clientId).toBeDefined();
    expect(msalConfig.auth.authority).toContain(
      "https://login.microsoftonline.com/"
    );
    expect(msalConfig.auth.redirectUri).toBe("/auth/login");
    expect(msalConfig.auth.postLogoutRedirectUri).toBe("/");
  });

  it("exports loginRequest with openid scopes", () => {
    expect(loginRequest.scopes).toContain("openid");
    expect(loginRequest.scopes).toContain("profile");
    expect(loginRequest.scopes).toContain("email");
  });

  it("uses sessionStorage for cache", () => {
    expect(msalConfig.cache?.cacheLocation).toBe("sessionStorage");
  });

  it("disables navigateToLoginRequestUrl", () => {
    expect(msalConfig.auth.navigateToLoginRequestUrl).toBe(false);
  });
});
