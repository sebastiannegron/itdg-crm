import { cn } from "@/lib/utils";

type Tier = 1 | 2 | 3;

const tierStyles: Record<Tier, string> = {
  1: "text-tier-1-text bg-tier-1-bg border-tier-1-border",
  2: "text-tier-2-text bg-tier-2-bg border-tier-2-border",
  3: "text-tier-3-text bg-tier-3-bg border-tier-3-border",
};

export function TierBadge({
  tier,
  className,
}: {
  tier: Tier;
  className?: string;
}) {
  return (
    <span
      className={cn(
        "inline-flex items-center rounded-full border px-2 py-0.5 text-[10px] font-semibold",
        tierStyles[tier],
        className
      )}
    >
      Tier {tier}
    </span>
  );
}
