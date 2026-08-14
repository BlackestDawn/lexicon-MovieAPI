"use client";

import { useAuth } from "@/context/commonContext";
import { AccessLevel } from "@/lib/data/interfaces/auth";
import { ReactNode } from "react";

export default function RestrictedComponent({
  children,
  accessLevel,
}: {
  children: ReactNode;
  accessLevel: AccessLevel;
}) {
  const { hasAccess } = useAuth();
  if (!hasAccess(accessLevel)) return null;

  return <>{children}</>;
}
