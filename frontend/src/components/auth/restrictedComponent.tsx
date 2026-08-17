"use client";

import { useAuth } from "@/context/commonContext";
import { AccessLevel } from "@/lib/data/interfaces/auth";
import { ReactNode } from "react";

export default function RestrictedComponent({
  children,
  accessLevel,
  id,
}: {
  children: ReactNode;
  accessLevel: AccessLevel;
  id?: string | null;
}) {
  const { user, hasAccess } = useAuth();
  const isOwner = id != null && user?.id === id;
  if (!isOwner && !hasAccess(accessLevel)) return null;

  return <>{children}</>;
}
