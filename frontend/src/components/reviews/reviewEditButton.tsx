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
        className="p-2 bg-blue-400 dark:bg-blue-700 text-slate-700 dark:text-slate-200 rounded-md"
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
