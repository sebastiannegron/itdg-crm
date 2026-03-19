export default function AdminLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <div className="flex min-h-screen">
      {/* Sidebar placeholder - will be implemented in Feature 0.6 */}
      <aside className="hidden lg:flex lg:w-64 lg:flex-col lg:border-r lg:border-border lg:bg-card">
        <div className="p-4 font-semibold text-primary">ITDG CRM</div>
      </aside>
      <main className="flex-1">{children}</main>
    </div>
  );
}
