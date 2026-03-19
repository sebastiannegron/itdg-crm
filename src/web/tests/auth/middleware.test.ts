import { describe, it, expect, vi, beforeEach } from "vitest";
import { NextRequest, NextResponse } from "next/server";

// Stub next-intl/middleware to pass through
vi.mock("next-intl/middleware", () => ({
  default: () => () => NextResponse.next(),
}));

vi.mock("@/i18n/routing", () => ({
  routing: {
    locales: ["en-pr", "es-pr"],
    defaultLocale: "en-pr",
    localePrefix: "always",
    localeDetection: false,
  },
}));

import middleware from "@/middleware";

function createRequest(pathname: string, cookies?: Record<string, string>) {
  const url = new URL(pathname, "http://localhost:3000");
  const req = new NextRequest(url);
  if (cookies) {
    for (const [key, value] of Object.entries(cookies)) {
      req.cookies.set(key, value);
    }
  }
  return req;
}

describe("middleware", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("passes through auth routes without redirect", () => {
    const res = middleware(createRequest("/auth/login"));
    expect(res.status).not.toBe(307);
  });

  it("redirects unauthenticated users from admin routes to login", () => {
    const res = middleware(createRequest("/en-pr/dashboard"));
    expect(res.status).toBe(307);
    const location = res.headers.get("location") ?? "";
    expect(location).toContain("/auth/login");
    expect(location).toContain("returnUrl=%2Fen-pr%2Fdashboard");
  });

  it("allows authenticated users to access admin routes", () => {
    const res = middleware(
      createRequest("/en-pr/dashboard", { "msal-authenticated": "true" })
    );
    expect(res.status).not.toBe(307);
  });

  it("protects all known admin segments", () => {
    const segments = [
      "dashboard",
      "clients",
      "documents",
      "communications",
      "tasks",
      "settings",
    ];

    for (const seg of segments) {
      const res = middleware(createRequest(`/en-pr/${seg}`));
      expect(res.status).toBe(307);
    }
  });

  it("does not protect the locale root", () => {
    const res = middleware(createRequest("/en-pr"));
    expect(res.status).not.toBe(307);
  });
});
