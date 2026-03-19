import createMiddleware from "next-intl/middleware";
import { type NextRequest, NextResponse } from "next/server";
import { routing } from "./i18n/routing";

const intlMiddleware = createMiddleware(routing);

/** Route segments that belong to the (admin) layout group. */
const ADMIN_SEGMENTS = new Set([
  "dashboard",
  "clients",
  "documents",
  "communications",
  "tasks",
  "settings",
]);

/**
 * Returns true when the pathname resolves to an (admin) route.
 * Pattern: /{locale}/{admin-segment}[/…]
 */
function isAdminRoute(pathname: string): boolean {
  const segments = pathname.split("/").filter(Boolean);
  return segments.length >= 2 && ADMIN_SEGMENTS.has(segments[1]);
}

export default function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl;

  // Auth routes do not need locale rewriting or protection
  if (pathname.startsWith("/auth")) {
    return NextResponse.next();
  }

  // Protect admin routes — redirect to login when unauthenticated
  if (isAdminRoute(pathname)) {
    const isAuthenticated =
      request.cookies.get("msal-authenticated")?.value === "true";

    if (!isAuthenticated) {
      const loginUrl = new URL("/auth/login", request.url);
      loginUrl.searchParams.set("returnUrl", pathname);
      return NextResponse.redirect(loginUrl);
    }
  }

  return intlMiddleware(request);
}

export const config = {
  matcher: ["/((?!api|_next|_vercel|.*\\..*).*)"],
};
