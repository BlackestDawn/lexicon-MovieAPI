import Form from "next/form";
import Link from "next/link";
import { fetchGenres } from "@/lib/actions/genre";
import { inputClass, labelClass } from "@/lib/data/consts/styles";

export default async function PersonFilters({
  name,
  genre,
  year,
}: {
  name?: string;
  genre?: string;
  year?: number;
}) {
  const genres = await fetchGenres();
  const hasFilters = !!name || !!genre || !!year;

  return (
    <Form
      action="/persons"
      className="flex flex-col sm:flex-row sm:items-end gap-4 mb-6 px-4"
    >
      <div className="flex-1">
        <label htmlFor="name" className={labelClass}>
          Name
        </label>
        <input
          type="text"
          id="name"
          name="name"
          placeholder="Search by name"
          defaultValue={name ?? ""}
          className={inputClass}
        />
      </div>
      <div className="flex-1">
        <label htmlFor="genre" className={labelClass}>
          Genre
        </label>
        <select
          id="genre"
          name="genre"
          defaultValue={genre ?? ""}
          className={inputClass}
        >
          <option value="">Any genre</option>
          {genres.map((g) => (
            <option key={g.id} value={g.slug}>
              {g.name}
            </option>
          ))}
        </select>
      </div>
      <div className="flex-1">
        <label htmlFor="year" className={labelClass}>
          Movie release year
        </label>
        <input
          type="number"
          id="year"
          name="year"
          placeholder="e.g. 1999"
          defaultValue={year ?? ""}
          className={inputClass}
        />
      </div>
      <div className="flex gap-2">
        <button
          type="submit"
          className="px-4 py-2 bg-blue-400 dark:bg-blue-700 text-slate-700 dark:text-slate-200 rounded-md"
        >
          Search
        </button>
        {hasFilters && (
          <Link
            href="/persons"
            className="px-4 py-2 rounded-md border border-slate-400 dark:border-slate-600 flex items-center"
          >
            Clear
          </Link>
        )}
      </div>
    </Form>
  );
}
