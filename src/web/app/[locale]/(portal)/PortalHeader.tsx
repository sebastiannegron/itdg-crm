"use client";

import { useState } from "react";
import { useLocale } from "next-intl";
import { Link, usePathname } from "@/i18n/routing";
import { MessageSquare, FileText, CreditCard, Menu, X } from "lucide-react";
import { fieldnames, type Locale } from "../_shared/app-fieldnames";

const navItems = [
  { key: "portal_nav_messages" as const, href: "/portal/messages", icon: MessageSquare },
  { key: "portal_nav_documents" as const, href: "/portal/documents", icon: FileText },
  { key: "portal_nav_payments" as const, href: "/portal/payments", icon: CreditCard },
];

export default function PortalHeader() {
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const locale = useLocale() as Locale;
  const pathname = usePathname();
  const t = fieldnames[locale];

  return (
    <header className="border-b border-border bg-card">
      <div className="mx-auto max-w-4xl px-4">
        <div className="flex h-14 items-center justify-between">
          <Link href="/portal" className="flex items-center gap-2">
            <div
              className="flex h-8 w-8 items-center justify-center rounded-md bg-primary text-primary-foreground text-sm font-bold"
              aria-hidden="true"
            >
              T
            </div>
            <span className="text-sm font-semibold text-foreground">
              {t.portal_name}
            </span>
          </Link>

          <nav className="hidden md:flex md:items-center md:gap-1" aria-label={t.portal_name}>
            {navItems.map((item) => {
              const Icon = item.icon;
              const isActive = pathname.startsWith(item.href);
              return (
                <Link
                  key={item.key}
                  href={item.href}
                  className={`flex items-center gap-1.5 rounded-md px-3 py-2 text-sm font-medium transition-colors ${
                    isActive
                      ? "bg-primary/10 text-primary"
                      : "text-muted-foreground hover:bg-secondary hover:text-foreground"
                  }`}
                >
                  <Icon className="h-4 w-4" />
                  {t[item.key]}
                </Link>
              );
            })}
          </nav>

          <button
            type="button"
            className="inline-flex items-center justify-center rounded-md p-2 text-muted-foreground hover:bg-secondary hover:text-foreground md:hidden"
            onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
            aria-expanded={mobileMenuOpen}
            aria-label={mobileMenuOpen ? t.portal_menu_close : t.portal_menu_open}
          >
            {mobileMenuOpen ? (
              <X className="h-5 w-5" />
            ) : (
              <Menu className="h-5 w-5" />
            )}
          </button>
        </div>
      </div>

      {mobileMenuOpen && (
        <nav className="border-t border-border md:hidden" aria-label={t.portal_name}>
          <div className="mx-auto max-w-4xl px-4 py-2">
            {navItems.map((item) => {
              const Icon = item.icon;
              const isActive = pathname.startsWith(item.href);
              return (
                <Link
                  key={item.key}
                  href={item.href}
                  className={`flex items-center gap-2 rounded-md px-3 py-2.5 text-sm font-medium transition-colors ${
                    isActive
                      ? "bg-primary/10 text-primary"
                      : "text-muted-foreground hover:bg-secondary hover:text-foreground"
                  }`}
                  onClick={() => setMobileMenuOpen(false)}
                >
                  <Icon className="h-4 w-4" />
                  {t[item.key]}
                </Link>
              );
            })}
          </div>
        </nav>
      )}
    </header>
  );
}
