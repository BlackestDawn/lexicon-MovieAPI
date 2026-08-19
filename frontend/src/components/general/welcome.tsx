"use client";

import { useAuth } from "@/context/commonContext";

export default function Welcome() {
  const { user } = useAuth();
  if (!user) return null;

  return <p className="font-medium text-primary">Welcome, {user.name}</p>;
}
