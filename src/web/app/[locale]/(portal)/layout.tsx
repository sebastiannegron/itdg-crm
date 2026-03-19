export default function PortalLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <div className="min-h-screen">
      {/* Portal header placeholder - will be implemented in Feature 0.6 */}
      <header className="border-b border-border bg-card px-4 py-3">
        <div className="mx-auto max-w-4xl font-semibold text-primary">
          Client Portal
        </div>
      </header>
      <main className="mx-auto max-w-4xl px-4 py-8">{children}</main>
    </div>
  );
}
