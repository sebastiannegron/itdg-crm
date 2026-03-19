import { Badge } from "@/app/_components/ui/badge";
import { cn } from "@/lib/utils";

type Tier = 1 | 2 | 3;

interface TierBadgeProps {
  tier: Tier;
  className?: string;
}

const tierConfig: Record<Tier, { label: string; className: string }> = {
  1: {
    label: "Tier 1",
    className: "bg-tier-1 text-white border-transparent",
  },
  2: {
    label: "Tier 2",
    className: "bg-tier-2 text-white border-transparent",
  },
  3: {
    label: "Tier 3",
    className: "bg-tier-3 text-white border-transparent",
  },
};

export function TierBadge({ tier, className }: TierBadgeProps) {
  const config = tierConfig[tier];

  return (
    <Badge className={cn(config.className, className)}>{config.label}</Badge>
  );
}
