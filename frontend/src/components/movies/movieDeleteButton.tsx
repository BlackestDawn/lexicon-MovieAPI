"use client";

import { removeMovie } from "@/lib/actions/movie";
import { Trash2 } from "lucide-react";
import { useRouter } from "next/navigation";
import { useTransition } from "react";

export default function MovieDeleteButton({
  id,
  redirect = false,
}: {
  id: string;
  redirect?: boolean;
}) {
  const [isPending, startTransition] = useTransition();
  const router = useRouter();

  const handleClick = () => {
    startTransition(async () => {
      await removeMovie(id);
      if (redirect) router.push("/movies");
    });
  };

  return (
    <button
      onClick={handleClick}
      disabled={isPending}
      className="p-2 bg-red-500 dark:bg-red-600 text-slate-900 dark:text-slate-100 rounded-md"
    >
      <Trash2 className="h-5 w-5" />
    </button>
  );
}
