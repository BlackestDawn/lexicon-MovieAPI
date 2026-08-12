"use client";

import { useAuth } from "@/context/commonContext";

export default function Welcome() {
  const { user } = useAuth();

  return (
    <div>
      <p>Welcome {user && user.name}</p>
    </div>
  );
}
