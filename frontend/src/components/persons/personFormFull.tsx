"use client";

import { createPerson, updatePerson } from "@/lib/actions/person";
import { fetchMovies } from "@/lib/actions/movie";
import { MovieDto } from "@/lib/data/models/movieTypes";
import { PersonExtendedDto } from "@/lib/data/models/personTypes";
import {
  PersonRole,
  personRoleLabels,
} from "@/lib/data/models/personRoleTypes";
import { Plus, X } from "lucide-react";
import Form from "next/form";
import { useEffect, useState, useTransition } from "react";
import { inputClass, labelClass } from "@/lib/data/consts/styles";
import DialogBase from "@/components/general/dialogBase";

interface MovieRoleEntry {
  movieId: string;
  role: PersonRole;
  label: string;
}

export default function PersonFullForm({
  onClose,
  existingPerson,
}: {
  onClose: () => void;
  existingPerson?: PersonExtendedDto;
}) {
  const [isPending, startTransition] = useTransition();
  const [error, setError] = useState<string>("");
  const [issues, setIssues] = useState<string[]>([]);

  const [movies, setMovies] = useState<MovieDto[]>([]);

  const [movieRoles, setMovieRoles] = useState<MovieRoleEntry[]>(
    () =>
      existingPerson?.movieRoles?.map((mr) => ({
        movieId: mr.movieId,
        role: mr.role,
        label: mr.title,
      })) ?? [],
  );
  const [newMovieId, setNewMovieId] = useState("");
  const [newRole, setNewRole] = useState<PersonRole>(PersonRole.Cast);

  useEffect(() => {
    fetchMovies()
      .then((r) => setMovies(r.movies))
      .catch((e) => console.error("Failed to load movies:", e));
  }, []);

  const handleSubmit = async (data: FormData) => {
    setError("");
    setIssues([]);

    // If a movie + role is selected but "Add" was never clicked, include it
    // anyway rather than silently dropping it from the save.
    const pendingMovie = newMovieId
      ? movies.find((m) => m.id === newMovieId)
      : undefined;
    const effectiveMovieRoles = pendingMovie
      ? [...movieRoles, { movieId: pendingMovie.id, role: newRole, label: pendingMovie.title }]
      : movieRoles;
    data.set(
      "movieRolesData",
      JSON.stringify(
        effectiveMovieRoles.map(({ movieId, role }) => ({ movieId, role })),
      ),
    );

    startTransition(async () => {
      const result = existingPerson
        ? await updatePerson(existingPerson.id, data)
        : await createPerson(data);

      if (result.success) {
        onClose();
        return;
      }
      setError(result.error ?? "Something went wrong");
      setIssues(result.issues ?? []);
    });
  };

  const addMovieRole = () => {
    if (!newMovieId) return;
    const movie = movies.find((m) => m.id === newMovieId);
    if (!movie) return;

    setMovieRoles((prev) => [
      ...prev,
      { movieId: movie.id, role: newRole, label: movie.title },
    ]);
    setNewMovieId("");
  };

  const removeMovieRole = (movieId: string) => {
    setMovieRoles((prev) => prev.filter((c) => c.movieId !== movieId));
  };

  const availableMovies = movies.filter(
    (m) => !movieRoles.some((c) => c.movieId === m.id),
  );

  const dateOfBirthDefault = existingPerson
    ? existingPerson.dateOfBirth.toISOString().slice(0, 10)
    : "";

  return (
    <DialogBase onClose={onClose}>
      <h3 id="dialog-title" className="w-full text-center text-2xl mb-4">
        {existingPerson ? "Edit person" : "Create new Person"}
      </h3>

      <Form action={handleSubmit} className="space-y-4">
        <div>
          <label htmlFor="givenName" className={labelClass}>
            Given name
          </label>
          <input
            type="text"
            id="givenName"
            name="givenName"
            placeholder="Enter a given name"
            defaultValue={existingPerson?.givenName ?? ""}
            disabled={isPending}
            className={inputClass}
          />
        </div>
        <div>
          <label htmlFor="middleName" className={labelClass}>
            Middle name
          </label>
          <input
            type="text"
            id="middleName"
            name="middleName"
            placeholder="Enter a middle name"
            defaultValue={existingPerson?.middleName ?? ""}
            disabled={isPending}
            className={inputClass}
          />
        </div>
        <div>
          <label htmlFor="lastName" className={labelClass}>
            Last name
          </label>
          <input
            type="text"
            id="lastName"
            name="lastName"
            placeholder="Enter a last name"
            defaultValue={existingPerson?.lastName ?? ""}
            disabled={isPending}
            className={inputClass}
          />
        </div>
        <div>
          <label htmlFor="dateOfBirth" className={labelClass}>
            Date of birth
          </label>
          <input
            type="date"
            id="dateOfBirth"
            name="dateOfBirth"
            defaultValue={dateOfBirthDefault}
            disabled={isPending}
            className={inputClass}
          />
        </div>

        <div>
          <p className={labelClass}>Movie roles</p>
          <div className="flex flex-col sm:flex-row gap-2">
            <select
              value={newMovieId}
              onChange={(e) => setNewMovieId(e.target.value)}
              disabled={isPending}
              className={inputClass}
            >
              <option value="">Select a movie...</option>
              {availableMovies.map((m) => (
                <option key={m.id} value={m.id}>
                  {m.title}
                </option>
              ))}
            </select>
            <select
              value={String(newRole)}
              onChange={(e) =>
                setNewRole(Number(e.target.value) as PersonRole)
              }
              disabled={isPending}
              className={inputClass}
            >
              {Object.entries(personRoleLabels).map(([value, label]) => (
                <option key={value} value={value}>
                  {label}
                </option>
              ))}
            </select>
            <button
              type="button"
              onClick={addMovieRole}
              disabled={isPending || !newMovieId}
              className="flex items-center justify-center gap-1 px-3 py-2 bg-primary text-primary-foreground rounded-md hover:bg-primary-hover transition-colors disabled:opacity-50 whitespace-nowrap"
            >
              <Plus className="w-4 h-4" />
              Add
            </button>
          </div>

          <ul className="mt-3 space-y-2">
            {movieRoles.map((c) => (
              <li
                key={c.movieId}
                className="flex items-center justify-between px-3 py-2 border border-border rounded-md"
              >
                <span>
                  {c.label}{" "}
                  <span className="text-muted-foreground">
                    — {personRoleLabels[c.role]}
                  </span>
                </span>
                <button
                  type="button"
                  onClick={() => removeMovieRole(c.movieId)}
                  disabled={isPending}
                  aria-label={`Remove ${c.label}`}
                  className="text-danger hover:opacity-80 transition-opacity"
                >
                  <X className="w-4 h-4" />
                </button>
              </li>
            ))}
            {movieRoles.length === 0 && (
              <li className="text-sm text-muted-foreground">
                No movie roles added yet.
              </li>
            )}
          </ul>
          <input
            type="hidden"
            name="movieRolesData"
            value={JSON.stringify(
              movieRoles.map(({ movieId, role }) => ({ movieId, role })),
            )}
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
              : existingPerson
                ? "Save changes"
                : "Create person"}
          </button>
        </div>
      </Form>
    </DialogBase>
  );
}
