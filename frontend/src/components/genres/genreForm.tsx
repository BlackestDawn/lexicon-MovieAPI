"use client";

import { createGenre, updateGenre } from "@/lib/actions/genre";
import { GenreExtendedDto } from "@/lib/data/models/genreTypes";
import Form from "next/form";
import { useState, useTransition } from "react";
import { inputClass, labelClass } from "@/lib/data/consts/styles";
import DialogBase from "@/components/general/dialogBase";

export default function GenreForm({
  onClose,
  existingGenre,
}: {
  onClose: () => void;
  existingGenre?: GenreExtendedDto;
}) {
  const [isPending, startTransition] = useTransition();
  const [error, setError] = useState<string>("");
  const [issues, setIssues] = useState<string[]>([]);

  const handleSubmit = async (data: FormData) => {
    setError("");
    setIssues([]);

    startTransition(async () => {
      const result = existingGenre
        ? await updateGenre(existingGenre.id, data)
        : await createGenre(data);

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
        {existingGenre ? "Edit genre" : "Create new genre"}
      </h3>

      <Form action={handleSubmit} className="space-y-4">
        <div>
          <label htmlFor="name" className={labelClass}>
            Name
          </label>
          <input
            type="text"
            id="name"
            name="name"
            placeholder="Enter a name"
            defaultValue={existingGenre?.name ?? ""}
            disabled={isPending}
            className={inputClass}
          />
        </div>
        <div>
          <label htmlFor="slug" className={labelClass}>
            Slug
          </label>
          <input
            type="text"
            id="slug"
            name="slug"
            placeholder="Enter a slug"
            defaultValue={existingGenre?.slug ?? ""}
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
              : existingGenre
                ? "Save changes"
                : "Create genre"}
          </button>
        </div>
      </Form>
    </DialogBase>
  );
}
