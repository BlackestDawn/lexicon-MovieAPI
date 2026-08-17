"use client";

import { useState } from "react";
import ReviewForm from "./reviewForm";

export default function ReviewCreateButton({ movieId }: { movieId: string }) {
  const [isOpen, setIsOpen] = useState<boolean>(false);

  return (
    <>
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="p-2 bg-green-400 dark:bg-green-700 text-slate-700 dark:text-slate-200 rounded-md"
      >
        Write a review
      </button>

      {isOpen && (
        <ReviewForm onClose={() => setIsOpen(false)} movieId={movieId} />
      )}
    </>
  );
}
