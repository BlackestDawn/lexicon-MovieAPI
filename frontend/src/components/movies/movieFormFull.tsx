"use client";

import { createMovie, updateMovie } from "@/lib/actions/movie";
import { fetchGenres } from "@/lib/actions/genre";
import { fetchPersons } from "@/lib/actions/person";
import { defaultBudget, defaultRuntime } from "@/lib/data/consts/general";
import { GenreDto } from "@/lib/data/models/genreTypes";
import { MovieExtendedDto } from "@/lib/data/models/movieTypes";
import {
  PersonRole,
  personRoleLabels,
} from "@/lib/data/models/personRoleTypes";
import { PersonDto } from "@/lib/data/models/personTypes";
import { Plus, X } from "lucide-react";
import Form from "next/form";
import { useEffect, useState, useTransition } from "react";
import { inputClass, labelClass } from "@/lib/data/consts/styles";

interface CastCrewEntry {
  personId: string;
  role: PersonRole;
  label: string;
}

function personLabel(person: {
  givenName: string;
  middleName?: string | null;
  lastName: string;
}) {
  return [person.givenName, person.middleName, person.lastName]
    .filter(Boolean)
    .join(" ");
}

export default function MovieFormFull({
  onClose,
  existingMovie,
}: {
  onClose: () => void;
  existingMovie?: MovieExtendedDto;
}) {
  const [isPending, startTransition] = useTransition();
  const [error, setError] = useState<string>("");
  const [issues, setIssues] = useState<string[]>([]);

  const [genres, setGenres] = useState<GenreDto[]>([]);
  const [persons, setPersons] = useState<PersonDto[]>([]);

  const [castCrew, setCastCrew] = useState<CastCrewEntry[]>(
    () =>
      existingMovie?.castCrews?.map((cc) => ({
        personId: cc.personId,
        role: cc.role,
        label: personLabel(cc),
      })) ?? [],
  );
  const [newPersonId, setNewPersonId] = useState("");
  const [newRole, setNewRole] = useState<PersonRole>(PersonRole.Cast);

  useEffect(() => {
    fetchGenres()
      .then(setGenres)
      .catch((e) => console.error("Failed to load genres:", e));
    fetchPersons()
      .then(r => setPersons(r.persons))
      .catch((e) => console.error("Failed to load persons:", e));
  }, []);

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
      const result = existingMovie
        ? await updateMovie(existingMovie.id, data)
        : await createMovie(data);

      if (result.success) {
        onClose();
        return;
      }
      setError(result.error ?? "Something went wrong");
      setIssues(result.issues ?? []);
    });
  };

  const addCastCrew = () => {
    if (!newPersonId) return;
    const person = persons.find((p) => p.id === newPersonId);
    if (!person) return;

    setCastCrew((prev) => [
      ...prev,
      { personId: person.id, role: newRole, label: personLabel(person) },
    ]);
    setNewPersonId("");
  };

  const removeCastCrew = (personId: string) => {
    setCastCrew((prev) => prev.filter((c) => c.personId !== personId));
  };

  const availablePersons = persons.filter(
    (p) => !castCrew.some((c) => c.personId === p.id),
  );

  const releaseDateDefault = existingMovie
    ? existingMovie.releaseDate.toISOString().slice(0, 10)
    : "";

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

          className="relative w-full max-w-2xl max-h-[90vh] overflow-y-auto p-6 border border-slate-600 dark:border-slate-400 rounded-2xl bg-white dark:bg-gray-800 shadow-xl"
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
            {existingMovie ? "Edit movie" : "Create new Movie"}
          </h3>

          <Form action={handleSubmit} className="space-y-4">
            <div>
              <label htmlFor="title" className={labelClass}>
                Title
              </label>
              <input
                type="text"
                id="title"
                name="title"
                placeholder="Enter a title"
                defaultValue={existingMovie?.title ?? ""}
                disabled={isPending}
                className={inputClass}
              />
            </div>
            <div className="flex flex-col md:flex-row gap-4 md:justify-evenly">
              <div>
                <label htmlFor="releaseDate" className={labelClass}>
                  Release date
                </label>
                <input
                  type="date"
                  id="releaseDate"
                  name="releaseDate"
                  defaultValue={releaseDateDefault}
                  disabled={isPending}
                  className={inputClass}
                />
              </div>
              <div className="flex flex-col gap-4">
                <div>
                  <label htmlFor="budget" className={labelClass}>
                    Budget
                  </label>
                  <input
                    type="number"
                    id="budget"
                    name="budget"
                    placeholder="Enter the movie's budget"
                    defaultValue={existingMovie?.details?.budget ?? defaultBudget}
                    disabled={isPending}
                    className={inputClass}
                  />
                </div>
                <div>
                  <label htmlFor="runtimeMinutes" className={labelClass}>
                    Runtime minutes
                  </label>
                  <input
                    type="number"
                    id="runtimeMinutes"
                    name="runtimeMinutes"
                    placeholder="Enter runtime in minutes"
                    defaultValue={existingMovie?.runtimeMinutes ?? defaultRuntime}
                    disabled={isPending}
                    className={inputClass}
                  />
                </div>
              </div>
            </div>
            <div>
              <label htmlFor="language" className={labelClass}>
                Language
              </label>
              <input
                type="text"
                id="language"
                name="language"
                placeholder="e.g. English"
                defaultValue={existingMovie?.details?.language ?? ""}
                disabled={isPending}
                className={inputClass}
              />
            </div>
            <div>
              <label htmlFor="plotSummery" className={labelClass}>
                Plot summary
              </label>
              <input
                type="text"
                id="plotSummery"
                name="plotSummery"
                placeholder="Enter a short plot summary"
                defaultValue={existingMovie?.plotSummery ?? ""}
                disabled={isPending}
                className={inputClass}
              />
            </div>
            <div>
              <label htmlFor="synopsis" className={labelClass}>
                Synopsis
              </label>
              <input
                type="text"
                id="synopsis"
                name="synopsis"
                placeholder="Enter a longer synopsis"
                defaultValue={existingMovie?.details?.synopsis ?? ""}
                disabled={isPending}
                className={inputClass}
              />
            </div>

            <div>
              <p className={labelClass}>Genres</p>
              <div className="flex flex-wrap gap-3">
                {genres.map((g) => (
                  <label
                    key={g.id}
                    className="flex items-center gap-1 px-2 py-1 border border-slate-600 dark:border-slate-400 rounded-md text-sm"
                  >
                    <input
                      type="checkbox"
                      name="genres"
                      value={g.id}
                      defaultChecked={existingMovie?.genres.some(
                        (mg) => mg.id === g.id,
                      )}
                      disabled={isPending}
                    />
                    {g.name}
                  </label>
                ))}
                {genres.length === 0 && (
                  <p className="text-sm text-slate-500 dark:text-slate-400">
                    Loading genres...
                  </p>
                )}
              </div>
            </div>

            <div>
              <p className={labelClass}>Cast & Crew</p>
              <div className="flex flex-col sm:flex-row gap-2">
                <select
                  value={newPersonId}
                  onChange={(e) => setNewPersonId(e.target.value)}
                  disabled={isPending}
                  className={inputClass}
                >
                  <option value="">Select a person...</option>
                  {availablePersons.map((p) => (
                    <option key={p.id} value={p.id}>
                      {personLabel(p)}
                    </option>
                  ))}
                </select>
                <select
                  value={String(newRole)}
                  onChange={(e) => setNewRole(Number(e.target.value) as PersonRole)}
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
                  onClick={addCastCrew}
                  disabled={isPending || !newPersonId}
                  className="flex items-center justify-center gap-1 px-3 py-2 bg-blue-400 dark:bg-blue-700 text-slate-700 dark:text-slate-200 rounded-md disabled:opacity-50 whitespace-nowrap"
                >
                  <Plus className="w-4 h-4" />
                  Add
                </button>
              </div>

              <ul className="mt-3 space-y-2">
                {castCrew.map((c) => (
                  <li
                    key={c.personId}
                    className="flex items-center justify-between px-3 py-2 border border-slate-600 dark:border-slate-400 rounded-md"
                  >
                    <span>
                      {c.label}{" "}
                      <span className="text-slate-500 dark:text-slate-400">
                        — {personRoleLabels[c.role]}
                      </span>
                    </span>
                    <button
                      type="button"
                      onClick={() => removeCastCrew(c.personId)}
                      disabled={isPending}
                      aria-label={`Remove ${c.label}`}
                      className="text-red-600 hover:text-red-800 dark:text-red-400 dark:hover:text-red-300"
                    >
                      <X className="w-4 h-4" />
                    </button>
                  </li>
                ))}
                {castCrew.length === 0 && (
                  <li className="text-sm text-slate-500 dark:text-slate-400">
                    No cast or crew added yet.
                  </li>
                )}
              </ul>
              <input
                type="hidden"
                name="castCrewData"
                value={JSON.stringify(
                  castCrew.map(({ personId, role }) => ({ personId, role })),
                )}
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
                  : existingMovie
                    ? "Save changes"
                    : "Create movie"}
              </button>
            </div>
          </Form>
        </div>
      </div>
    </>
  );
}
