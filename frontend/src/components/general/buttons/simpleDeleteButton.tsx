"use client";

import { Trash2 } from "lucide-react";
import { useRouter } from "next/navigation";
import { MouseEvent, useTransition } from "react";

export default function SimpleDeleteButton({
  id,
  redirectTo,
  onDelete,
}: {
  id: string;
  redirectTo?: string;
  onDelete: (id: string) => Promise<void>;
}) {
  const [isPending, startTransition] = useTransition();
  const router = useRouter();

  const handleClick = (e: MouseEvent) => {
    e.preventDefault();
    startTransition(async () => {
      await onDelete(id);
      if (redirectTo) router.push(redirectTo);
    });
  };

  return (
    <button
      onClick={handleClick}
      disabled={isPending}
      className="p-2 bg-danger text-danger-foreground rounded-md hover:opacity-90 transition-opacity disabled:opacity-50"
    >
      <Trash2 className="h-5 w-5" />
    </button>
  );
}
