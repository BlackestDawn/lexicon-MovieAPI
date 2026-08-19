"use client";

import { ReviewDto } from "@/lib/data/models/reviewTypes";
import { FilePen } from "lucide-react";
import { useState } from "react";
import ReviewForm from "./reviewForm";

export default function ReviewEditButton({
  movieId,
  review,
}: {
  movieId: string;
  review: ReviewDto;
}) {
  const [isOpen, setIsOpen] = useState<boolean>(false);

  return (
    <>
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="p-2 bg-accent text-accent-foreground rounded-md hover:bg-accent-hover transition-colors"
      >
        <FilePen className="w-5 h-5" />
      </button>

      {isOpen && (
        <ReviewForm
          onClose={() => setIsOpen(false)}
          movieId={movieId}
          existingReview={review}
        />
      )}
    </>
  );
}
