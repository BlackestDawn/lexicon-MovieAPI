"use client";

import { useState } from "react";
import ReviewForm from "./reviewForm";

export default function ReviewCreateButton({ movieId }: { movieId: string }) {
  const [isOpen, setIsOpen] = useState<boolean>(false);

  return (
    <>
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="p-2 bg-primary text-primary-foreground rounded-md hover:bg-primary-hover transition-colors"
      >
        Write a review
      </button>

      {isOpen && (
        <ReviewForm onClose={() => setIsOpen(false)} movieId={movieId} />
      )}
    </>
  );
}
