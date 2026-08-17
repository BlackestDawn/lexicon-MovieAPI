"use client";

import { createReview, updateReview } from "@/lib/actions/review";
import { ReviewDto } from "@/lib/data/models/reviewTypes";
import { X } from "lucide-react";
import Form from "next/form";
import { useEffect, useState, useTransition } from "react";
import { inputClass, labelClass } from "@/lib/data/consts/styles";

export default function ReviewForm({
  onClose,
  movieId,
  existingReview,
}: {
  onClose: () => void;
  movieId: string;
  existingReview?: ReviewDto;
}) {
  const [isPending, startTransition] = useTransition();
  const [error, setError] = useState<string>("");
  const [issues, setIssues] = useState<string[]>([]);

  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape") onClose();
    };
    document.addEventListener("keydown", handleKeyDown);
    return () => document.removeEventListener("keydown", handleKeyDown);
  }, [onClose]);

  const handleSubmit = async (data: FormData) => {
    setError("");
    setIssues([]);

    startTransition(async () => {
      const result = existingReview
        ? await updateReview(movieId, existingReview.id, data)
        : await createReview(movieId, data);

      if (result.success) {
        onClose();
        return;
      }
      setError(result.error ?? "Something went wrong");
      setIssues(result.issues ?? []);
    });
  };

  return (
    <>
      <div
        className="fixed inset-0 z-40 bg-black/50"
        onClick={onClose}
        aria-hidden="true"
      />
      <div
        className="fixed inset-0 z-50 flex items-center justify-center p-4"
        onClick={onClose}
      >
        <div
          onClick={(e) => e.stopPropagation()}
          role="dialog"
          aria-modal="true"
          className="relative w-full max-w-md p-6 border border-slate-600 dark:border-slate-400 rounded-2xl bg-white dark:bg-gray-800 shadow-xl"
        >
          <button
            type="button"
            onClick={onClose}
            aria-label="Close"
            className="absolute top-4 right-4 text-slate-500 hover:text-slate-800 dark:hover:text-slate-200"
          >
            <X className="w-5 h-5" />
          </button>

          <h3 id="dialog-title" className="w-full text-center text-2xl mb-4">
            {existingReview ? "Edit review" : "Write a review"}
          </h3>

          <Form action={handleSubmit} className="space-y-4">
            <div>
              <label htmlFor="authorName" className={labelClass}>
                Name
              </label>
              <input
                type="text"
                id="authorName"
                name="authorName"
                placeholder="Enter your name"
                defaultValue={existingReview?.authorName ?? ""}
                disabled={isPending}
                className={inputClass}
              />
            </div>
            <div>
              <label htmlFor="body" className={labelClass}>
                Review
              </label>
              <textarea
                id="body"
                name="body"
                rows={5}
                placeholder="Share your thoughts"
                defaultValue={existingReview?.body ?? ""}
                disabled={isPending}
                className={inputClass}
              />
            </div>
            <div>
              <label htmlFor="score" className={labelClass}>
                Score
              </label>
              <input
                type="number"
                id="score"
                name="score"
                min={1}
                max={10}
                step={1}
                placeholder="1-10"
                defaultValue={existingReview?.score ?? ""}
                disabled={isPending}
                className={inputClass}
              />
            </div>

            {(error || issues.length > 0) && (
              <div className="rounded-md bg-red-50 dark:bg-red-900 p-4 space-y-1">
                {error && (
                  <p className="text-sm text-red-800 dark:text-red-200">
                    {error}
                  </p>
                )}
                {issues.map((issue, i) => (
                  <p key={i} className="text-sm text-red-800 dark:text-red-200">
                    {issue}
                  </p>
                ))}
              </div>
            )}

            <div className="flex justify-end gap-4 pt-2">
              <button
                type="button"
                onClick={onClose}
                disabled={isPending}
                className="px-4 py-2 rounded-md border border-slate-400 dark:border-slate-600 disabled:opacity-50"
              >
                Cancel
              </button>
              <button
                type="submit"
                disabled={isPending}
                className="px-4 py-2 bg-blue-400 dark:bg-blue-700 text-slate-700 dark:text-slate-200 rounded-md disabled:opacity-50"
              >
                {isPending
                  ? "Saving..."
                  : existingReview
                    ? "Save changes"
                    : "Post review"}
              </button>
            </div>
          </Form>
        </div>
      </div>
    </>
  );
}
