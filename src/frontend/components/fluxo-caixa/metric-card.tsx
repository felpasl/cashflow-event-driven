import { TrendingUp } from "lucide-react";

import { Card, CardContent } from "@/components/ui/card";
import { cn } from "@/lib/utils";

export function MetricCard({
  icon: Icon,
  label,
  value,
  tone
}: {
  icon: typeof TrendingUp;
  label: string;
  value: string;
  tone: "positive" | "negative";
}) {
  return (
    <Card size="sm" className="rounded-lg">
      <CardContent className="flex items-center gap-3 py-1">
        <span className={cn(
          "flex size-9 items-center justify-center rounded-lg",
          tone === "positive" ? "bg-emerald-50 text-emerald-700" : "bg-red-50 text-red-700"
        )}>
          <Icon className="size-4" />
        </span>
        <div className="min-w-0">
          <p className="text-xs text-muted-foreground">{label}</p>
          <p className={cn("truncate text-base font-semibold", tone === "positive" ? "text-emerald-700" : "text-red-700")}>{value}</p>
        </div>
      </CardContent>
    </Card>
  );
}
