import PortalHeader from "./PortalHeader";

export default function PortalLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <div className="min-h-screen bg-background">
      <PortalHeader />
      <main className="mx-auto max-w-4xl px-4 py-8">{children}</main>
    </div>
  );
}
