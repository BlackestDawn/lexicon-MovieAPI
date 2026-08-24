"use client";

import { Heart } from "lucide-react";
import { MouseEvent, useState, useTransition } from "react";
import { addFavorite, removeFavorite } from "@/lib/actions/favorite";

export default function FavoriteButton({
  movieId,
  initiallyFavorited,
}: {
  movieId: string;
  initiallyFavorited: boolean;
}) {
  const [isFavorited, setIsFavorited] = useState(initiallyFavorited);
  const [isPending, startTransition] = useTransition();

  const handleClick = (e: MouseEvent) => {
    // Cards using this button wrap it in their own Link - stop that
    // navigation from firing when the user meant to toggle the favorite.
    e.preventDefault();

    startTransition(async () => {
      const result = isFavorited
        ? await removeFavorite(movieId)
        : await addFavorite(movieId);

      if (result.success) {
        setIsFavorited(!isFavorited);
      }
    });
  };

  return (
    <button
      onClick={handleClick}
      disabled={isPending}
      aria-label={isFavorited ? "Remove from favorites" : "Add to favorites"}
      aria-pressed={isFavorited}
      className={`p-2 rounded-md border transition-colors disabled:opacity-50 ${
        isFavorited
          ? "bg-danger/10 border-danger/30 text-danger hover:bg-danger/20"
          : "border-border text-muted-foreground hover:text-danger hover:border-danger/30"
      }`}
    >
      <Heart className={`h-5 w-5 ${isFavorited ? "fill-current" : ""}`} />
    </button>
  );
}
