"use client";

import {
  LayoutDashboard,
  Users,
  FileText,
  MessageSquare,
  CheckSquare,
  Settings,
  Menu,
  PanelLeftClose,
  PanelLeftOpen,
  Bell,
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
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/app/_components/ui/tooltip";

interface NavItem {
  href: string;
  labelKey: keyof (typeof fieldnames)["en-pr"];
  mobileLabelKey?: keyof (typeof fieldnames)["en-pr"];
  icon: React.ComponentType<{ className?: string }>;
}

const navItems: NavItem[] = [
  { href: "/dashboard", labelKey: "nav_dashboard", icon: LayoutDashboard },
  { href: "/clients", labelKey: "nav_clients", icon: Users },
  { href: "/documents", labelKey: "nav_documents", icon: FileText },
  {
    href: "/communications",
    labelKey: "nav_communications",
    mobileLabelKey: "nav_comms",
    icon: MessageSquare,
  },
  { href: "/tasks", labelKey: "nav_tasks", icon: CheckSquare },
];

const settingsItem: NavItem = {
  href: "/settings",
  labelKey: "nav_settings",
  icon: Settings,
};

function DesktopNavLink({
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
        "flex items-center gap-3 rounded-r-md py-2.5 pl-3 pr-3 text-sm font-medium transition-colors",
        isActive
          ? "border-l-[3px] border-primary bg-sidebar-accent text-white"
          : "border-l-[3px] border-transparent text-white/40 hover:bg-white/5 hover:text-white/70"
      )}
      aria-current={isActive ? "page" : undefined}
    >
      <item.icon
        className={cn("h-[18px] w-[18px] shrink-0", isActive && "text-primary")}
      />
      <span>{label}</span>
    </Link>
  );
}

function TabletNavLink({
  item,
  locale,
  pathname,
}: {
  item: NavItem;
  locale: Locale;
  pathname: string;
}) {
  const label = fieldnames[locale][item.mobileLabelKey ?? item.labelKey];
  const isActive =
    pathname === item.href || pathname.startsWith(`${item.href}/`);

  return (
    <Tooltip>
      <TooltipTrigger asChild>
        <Link
          href={item.href}
          className={cn(
            "flex h-11 items-center justify-center transition-colors",
            isActive
              ? "border-l-[3px] border-primary bg-sidebar-accent"
              : "border-l-[3px] border-transparent hover:bg-white/5"
          )}
          aria-current={isActive ? "page" : undefined}
          aria-label={label}
        >
          <item.icon
            className={cn(
              "h-[18px] w-[18px]",
              isActive ? "text-primary" : "text-white/35"
            )}
          />
        </Link>
      </TooltipTrigger>
      <TooltipContent side="right">{label}</TooltipContent>
    </Tooltip>
  );
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
  const labelKey = item.mobileLabelKey ?? item.labelKey;
  const label = fieldnames[locale][labelKey];
  const isActive =
    pathname === item.href || pathname.startsWith(`${item.href}/`);

  return (
    <Link
      href={item.href}
      className={cn(
        "flex flex-1 flex-col items-center justify-center gap-0.5 py-1 text-[9px] font-medium transition-colors",
        isActive
          ? "border-t-2 border-primary text-primary"
          : "border-t-2 border-transparent text-white/35 hover:text-white/60"
      )}
      aria-current={isActive ? "page" : undefined}
    >
      <item.icon className="h-4 w-4 shrink-0" />
      <span className="truncate">{label}</span>
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
                  <SheetDescription className="text-left text-xs text-muted-foreground">
                    {labels.app_subtitle}
                  </SheetDescription>
                </SheetHeader>
                <nav className="flex flex-1 flex-col gap-1 p-3" aria-label="Main">
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
                <div className="border-t border-border p-3">
                  <SheetNavLink
                    item={settingsItem}
                    locale={locale}
                    pathname={pathname}
                    onNavigate={() => setSheetOpen(false)}
                  />
                </div>
              </SheetContent>
            </Sheet>
          </div>

          {/* App name in header (mobile + tablet) */}
          <span className="font-semibold text-primary lg:hidden">
            {labels.app_name_short}
          </span>

          {/* Breadcrumb area (desktop) */}
          <span className="hidden text-xs text-muted-foreground lg:inline">
            Raposo &amp; Associates &middot; 2025
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
            {/* Notification bell */}
            <Button
              variant="ghost"
              size="icon"
              className="relative h-[34px] w-[34px] rounded-lg bg-background"
              aria-label={labels.nav_notifications}
            >
              <Bell className="h-4 w-4" />
              {notificationCount > 0 && (
                <Badge className="absolute -right-1 -top-1 flex h-4 min-w-4 items-center justify-center rounded-full px-1 text-[8px]">
                  {notificationCount}
                </Badge>
              )}
            </Button>

            {/* User avatar */}
            <div className="flex h-[30px] w-[30px] items-center justify-center rounded-full bg-sidebar text-[10px] font-bold text-white">
              MR
            </div>
          </div>
        </header>

        <div className="flex flex-1">
          {/* Desktop sidebar (lg+) — 210px, navy, not collapsible */}
          <aside className="hidden lg:flex lg:w-[210px] lg:flex-col lg:bg-sidebar">
            {/* Brand */}
            <div className="border-b border-white/[0.08] px-4 pb-3.5 pt-[18px]">
              <div className="text-[17px] font-extrabold text-primary">
                {labels.app_name_short}
              </div>
              <div className="mt-0.5 text-[9px] uppercase tracking-[1.2px] text-white/30">
                {labels.app_subtitle}
              </div>
            </div>

            {/* Nav links */}
            <nav
              className="flex flex-1 flex-col gap-0.5 px-2 py-1.5"
              aria-label="Main"
            >
              {navItems.map((item) => (
                <DesktopNavLink
                  key={item.href}
                  item={item}
                  locale={locale}
                  pathname={pathname}
                />
              ))}
            </nav>

            {/* Settings gear in footer */}
            <div className="border-t border-white/[0.08] px-2 py-2">
              <Link
                href="/settings"
                className={cn(
                  "flex items-center gap-3 rounded-r-md py-2 pl-3 pr-3 text-sm font-medium transition-colors",
                  pathname === "/settings" ||
                    pathname.startsWith("/settings/")
                    ? "border-l-[3px] border-primary bg-sidebar-accent text-white"
                    : "border-l-[3px] border-transparent text-white/40 hover:bg-white/5 hover:text-white/70"
                )}
              >
                <Settings className="h-[18px] w-[18px] shrink-0" />
                <span>{labels.nav_settings}</span>
              </Link>
            </div>
          </aside>

          {/* Tablet sidebar (md–lg) — 52px icon-only, persistent */}
          <aside className="hidden md:flex md:w-[52px] md:flex-col md:bg-sidebar lg:hidden">
            {/* Brand */}
            <div className="flex items-center justify-center border-b border-white/[0.08] py-3.5">
              <span className="text-[17px] font-extrabold text-primary">
                {labels.app_name_short.charAt(0)}
              </span>
            </div>

            {/* Nav icons */}
            <nav className="flex flex-1 flex-col gap-0.5 py-1.5" aria-label="Main">
              {navItems.map((item) => (
                <TabletNavLink
                  key={item.href}
                  item={item}
                  locale={locale}
                  pathname={pathname}
                />
              ))}
            </nav>

            {/* Settings gear in footer */}
            <div className="border-t border-white/[0.08] py-2">
              <Tooltip>
                <TooltipTrigger asChild>
                  <Link
                    href="/settings"
                    className={cn(
                      "flex h-11 items-center justify-center transition-colors",
                      pathname === "/settings" ||
                        pathname.startsWith("/settings/")
                        ? "border-l-[3px] border-primary bg-sidebar-accent"
                        : "border-l-[3px] border-transparent hover:bg-white/5"
                    )}
                    aria-label={labels.nav_settings}
                  >
                    <Settings
                      className={cn(
                        "h-[18px] w-[18px]",
                        pathname === "/settings" ||
                          pathname.startsWith("/settings/")
                          ? "text-primary"
                          : "text-white/35"
                      )}
                    />
                  </Link>
                </TooltipTrigger>
                <TooltipContent side="right">
                  {labels.nav_settings}
                </TooltipContent>
              </Tooltip>
            </div>
          </aside>

          {/* Main content */}
          <main className="flex-1 overflow-y-auto pb-16 md:pb-0">
            {children}
          </main>
        </div>

        {/* Mobile bottom nav */}
        <nav
          className="fixed bottom-0 left-0 right-0 z-40 flex h-14 items-stretch bg-sidebar md:hidden"
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
