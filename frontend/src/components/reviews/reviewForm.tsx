"use client";

import { createReview, updateReview } from "@/lib/actions/review";
import { ReviewDto } from "@/lib/data/models/reviewTypes";
import Form from "next/form";
import { useState, useTransition } from "react";
import { inputClass, labelClass } from "@/lib/data/consts/styles";
import DialogBase from "@/components/general/dialogBase";

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
    <DialogBase onClose={onClose} size="md">
      <h3 id="dialog-title" className="w-full text-center text-2xl mb-4">
        {existingReview ? "Edit review" : "Write a review"}
      </h3>

      <Form action={handleSubmit} className="space-y-4">
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
          <div className="rounded-md bg-danger/10 border border-danger/30 p-4 space-y-1">
            {error && <p className="text-sm text-danger">{error}</p>}
            {issues.map((issue, i) => (
              <p key={i} className="text-sm text-danger">
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
            className="px-4 py-2 rounded-md border border-border hover:bg-background transition-colors disabled:opacity-50"
          >
            Cancel
          </button>
          <button
            type="submit"
            disabled={isPending}
            className="px-4 py-2 bg-primary text-primary-foreground rounded-md hover:bg-primary-hover transition-colors disabled:opacity-50"
          >
            {isPending
              ? "Saving..."
              : existingReview
                ? "Save changes"
                : "Post review"}
          </button>
        </div>
      </Form>
    </DialogBase>
  );
}
