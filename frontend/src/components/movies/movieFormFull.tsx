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
import DialogBase from "@/components/general/dialogBase";

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

  const handleSubmit = async (data: FormData) => {
    setError("");
    setIssues([]);

    // If a person + role is selected but "Add" was never clicked, include it
    // anyway rather than silently dropping it from the save.
    const pendingPerson = newPersonId
      ? persons.find((p) => p.id === newPersonId)
      : undefined;
    const effectiveCastCrew = pendingPerson
      ? [...castCrew, { personId: pendingPerson.id, role: newRole, label: personLabel(pendingPerson) }]
      : castCrew;
    data.set(
      "castCrewData",
      JSON.stringify(
        effectiveCastCrew.map(({ personId, role }) => ({ personId, role })),
      ),
    );

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
    <DialogBase onClose={onClose}>
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
                className="flex items-center gap-1 px-2 py-1 border border-border rounded-md text-sm"
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
              <p className="text-sm text-muted-foreground">
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
              className="flex items-center justify-center gap-1 px-3 py-2 bg-primary text-primary-foreground rounded-md hover:bg-primary-hover transition-colors disabled:opacity-50 whitespace-nowrap"
            >
              <Plus className="w-4 h-4" />
              Add
            </button>
          </div>

          <ul className="mt-3 space-y-2">
            {castCrew.map((c) => (
              <li
                key={c.personId}
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
                  onClick={() => removeCastCrew(c.personId)}
                  disabled={isPending}
                  aria-label={`Remove ${c.label}`}
                  className="text-danger hover:opacity-80 transition-opacity"
                >
                  <X className="w-4 h-4" />
                </button>
              </li>
            ))}
            {castCrew.length === 0 && (
              <li className="text-sm text-muted-foreground">
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
              : existingMovie
                ? "Save changes"
                : "Create movie"}
          </button>
        </div>
      </Form>
    </DialogBase>
  );
}
