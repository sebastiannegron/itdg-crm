"use client";

import { useState } from "react";
import {
  LayoutDashboard,
  Users,
  FileText,
  MessageSquare,
  Settings,
  Menu,
  PanelLeftClose,
  PanelLeftOpen,
} from "lucide-react";
import { useLocale } from "next-intl";
import { Link, usePathname } from "@/i18n/routing";
import { cn } from "@/lib/utils";
import { fieldnames, type Locale } from "@/app/[locale]/_shared/app-fieldnames";
import { Button } from "@/app/_components/ui/button";
import {
  NotificationPanel,
  type NotificationItem,
} from "@/app/_components/NotificationPanel";
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
  SheetTrigger,
} from "@/app/_components/ui/sheet";
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/app/_components/ui/tooltip";


interface NavItem {
  href: string;
  labelKey: keyof (typeof fieldnames)["en-pr"];
  icon: React.ComponentType<{ className?: string }>;
}

const navItems: NavItem[] = [
  { href: "/dashboard", labelKey: "nav_dashboard", icon: LayoutDashboard },
  { href: "/clients", labelKey: "nav_clients", icon: Users },
  { href: "/documents", labelKey: "nav_documents", icon: FileText },
  {
    href: "/communications",
    labelKey: "nav_communications",
    icon: MessageSquare,
  },
  { href: "/settings", labelKey: "nav_settings", icon: Settings },
];

function NavLink({
  item,
  locale,
  pathname,
  collapsed,
}: {
  item: NavItem;
  locale: Locale;
  pathname: string;
  collapsed: boolean;
}) {
  const label = fieldnames[locale][item.labelKey];
  const isActive =
    pathname === item.href || pathname.startsWith(`${item.href}/`);

  const linkContent = (
    <Link
      href={item.href}
      className={cn(
        "flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors",
        isActive
          ? "bg-primary/10 text-primary"
          : "text-muted-foreground hover:bg-secondary hover:text-foreground",
        collapsed && "justify-center px-2"
      )}
      aria-current={isActive ? "page" : undefined}
    >
      <item.icon className="h-5 w-5 shrink-0" />
      {!collapsed && <span>{label}</span>}
    </Link>
  );

  if (collapsed) {
    return (
      <Tooltip>
        <TooltipTrigger asChild>{linkContent}</TooltipTrigger>
        <TooltipContent side="right">{label}</TooltipContent>
      </Tooltip>
    );
  }

  return linkContent;
}

function MobileNavLink({
  item,
  locale,
  pathname,
}: {
  item: NavItem;
  locale: Locale;
  pathname: string;
}) {
  const label = fieldnames[locale][item.labelKey];
  const isActive =
    pathname === item.href || pathname.startsWith(`${item.href}/`);

  return (
    <Link
      href={item.href}
      className={cn(
        "flex flex-col items-center gap-1 px-1 py-1 text-xs font-medium transition-colors",
        isActive
          ? "text-primary"
          : "text-muted-foreground hover:text-foreground"
      )}
      aria-current={isActive ? "page" : undefined}
    >
      <item.icon className="h-5 w-5 shrink-0" />
      <span className="truncate">{label}</span>
    </Link>
  );
}

function SheetNavLink({
  item,
  locale,
  pathname,
  onNavigate,
}: {
  item: NavItem;
  locale: Locale;
  pathname: string;
  onNavigate: () => void;
}) {
  const label = fieldnames[locale][item.labelKey];
  const isActive =
    pathname === item.href || pathname.startsWith(`${item.href}/`);

  return (
    <Link
      href={item.href}
      onClick={onNavigate}
      className={cn(
        "flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors",
        isActive
          ? "bg-primary/10 text-primary"
          : "text-muted-foreground hover:bg-secondary hover:text-foreground"
      )}
      aria-current={isActive ? "page" : undefined}
    >
      <item.icon className="h-5 w-5 shrink-0" />
      <span>{label}</span>
    </Link>
  );
}

export default function AdminSidebar({
  children,
}: {
  children: React.ReactNode;
}) {
  const locale = useLocale() as Locale;
  const pathname = usePathname();
  const [collapsed, setCollapsed] = useState(false);
  const [sheetOpen, setSheetOpen] = useState(false);
  const [notifications, setNotifications] = useState<NotificationItem[]>([]);
  const labels = fieldnames[locale];

  const handleMarkAllRead = () => {
    setNotifications((prev) => prev.map((n) => ({ ...n, read: true })));
  };

  return (
    <TooltipProvider delayDuration={0}>
      <div className="flex min-h-screen flex-col">
        {/* Header */}
        <header className="sticky top-0 z-40 flex h-14 items-center border-b border-border bg-card px-4">
          {/* Tablet hamburger (md only) */}
          <div className="hidden md:block lg:hidden">
            <Sheet open={sheetOpen} onOpenChange={setSheetOpen}>
              <SheetTrigger asChild>
                <Button
                  variant="ghost"
                  size="icon"
                  aria-label={labels.nav_open_menu}
                >
                  <Menu className="h-5 w-5" />
                </Button>
              </SheetTrigger>
              <SheetContent side="left" className="w-64 p-0">
                <SheetHeader className="border-b border-border px-4 py-4">
                  <SheetTitle className="text-left text-primary">
                    {labels.app_name_short}
                  </SheetTitle>
                  <SheetDescription className="sr-only">
                    {labels.app_name}
                  </SheetDescription>
                </SheetHeader>
                <nav className="flex flex-col gap-1 p-3" aria-label="Main">
                  {navItems.map((item) => (
                    <SheetNavLink
                      key={item.href}
                      item={item}
                      locale={locale}
                      pathname={pathname}
                      onNavigate={() => setSheetOpen(false)}
                    />
                  ))}
                </nav>
              </SheetContent>
            </Sheet>
          </div>

          {/* App name in header (mobile + tablet) */}
          <span className="font-semibold text-primary lg:hidden">
            {labels.app_name_short}
          </span>

          <div className="ml-auto flex items-center gap-2">
            {/* Notification panel */}
            <NotificationPanel
              notifications={notifications}
              onMarkAllRead={handleMarkAllRead}
              bellLabel={labels.nav_notifications}
              title={labels.nav_notifications}
              markAllReadLabel={labels.notifications_mark_all_read}
              emptyLabel={labels.notifications_empty}
            />
          </div>
        </header>

        <div className="flex flex-1">
          {/* Desktop sidebar (lg+) */}
          <aside
            className={cn(
              "hidden lg:flex lg:flex-col lg:border-r lg:border-border lg:bg-card",
              collapsed ? "lg:w-16" : "lg:w-64"
            )}
          >
            {/* Sidebar header with brand */}
            <div
              className={cn(
                "flex h-14 items-center border-b border-border px-4",
                collapsed ? "justify-center" : "justify-between"
              )}
            >
              {!collapsed && (
                <span className="font-semibold text-primary">
                  {labels.app_name_short}
                </span>
              )}
              <Tooltip>
                <TooltipTrigger asChild>
                  <Button
                    variant="ghost"
                    size="icon"
                    onClick={() => setCollapsed(!collapsed)}
                    aria-label={
                      collapsed
                        ? labels.nav_expand_sidebar
                        : labels.nav_collapse_sidebar
                    }
                    className="h-8 w-8"
                  >
                    {collapsed ? (
                      <PanelLeftOpen className="h-4 w-4" />
                    ) : (
                      <PanelLeftClose className="h-4 w-4" />
                    )}
                  </Button>
                </TooltipTrigger>
                <TooltipContent side="right">
                  {collapsed
                    ? labels.nav_expand_sidebar
                    : labels.nav_collapse_sidebar}
                </TooltipContent>
              </Tooltip>
            </div>

            {/* Desktop nav links */}
            <nav className="flex flex-1 flex-col gap-1 p-3" aria-label="Main">
              {navItems.map((item) => (
                <NavLink
                  key={item.href}
                  item={item}
                  locale={locale}
                  pathname={pathname}
                  collapsed={collapsed}
                />
              ))}
            </nav>
          </aside>

          {/* Main content */}
          <main className="flex-1 overflow-y-auto pb-16 md:pb-0">
            {children}
          </main>
        </div>

        {/* Mobile bottom nav (sm only) */}
        <nav
          className="fixed bottom-0 left-0 right-0 z-40 flex items-center justify-around border-t border-border bg-card md:hidden"
          aria-label="Main"
        >
          {navItems.map((item) => (
            <MobileNavLink
              key={item.href}
              item={item}
              locale={locale}
              pathname={pathname}
            />
          ))}
        </nav>
      </div>
    </TooltipProvider>
  );
}
