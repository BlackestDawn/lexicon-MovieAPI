"use client";

import { useAuth } from "@/context/commonContext";
import { panelClass } from "@/lib/data/consts/styles";

export default function ProfileSummaryCard() {
  const { user } = useAuth();

  if (!user) return null;

  return (
    <div className={`${panelClass} max-w-md space-y-3`}>
      <div className="flex items-center justify-between">
        <h3 className="text-lg font-semibold">{user.name}</h3>
        <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-medium bg-primary/10 text-primary border border-primary/20">
          {user.role}
        </span>
      </div>
      <p className="text-sm text-muted-foreground">{user.email}</p>
    </div>
  );
}
