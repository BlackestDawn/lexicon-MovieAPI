import Form from "next/form";
import Link from "next/link";
import { fetchGenres } from "@/lib/actions/genre";
import { inputClass, labelClass, panelClass } from "@/lib/data/consts/styles";

export default async function MovieFilters({
  search,
  genre,
  year,
  minRating,
  maxRating,
}: {
  search?: string;
  genre?: string;
  year?: number;
  minRating?: number;
  maxRating?: number;
}) {
  const genres = await fetchGenres();
  const hasFilters =
    !!search || !!genre || !!year || !!minRating || !!maxRating;

  return (
    <Form
      action="/movies"
      className={`flex flex-col sm:flex-row sm:items-end gap-4 ${panelClass}`}
    >
      <div className="flex-1">
        <label htmlFor="search" className={labelClass}>
          Search
        </label>
        <input
          type="text"
          id="search"
          name="search"
          placeholder="Search title or synopsis"
          defaultValue={search ?? ""}
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
          Release year
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
      <div className="flex-1">
        <label htmlFor="minRating" className={labelClass}>
          Min rating
        </label>
        <input
          type="number"
          id="minRating"
          name="minRating"
          min={0}
          max={10}
          step={0.1}
          placeholder="e.g. 7"
          defaultValue={minRating ?? ""}
          className={inputClass}
        />
      </div>
      <div className="flex-1">
        <label htmlFor="maxRating" className={labelClass}>
          Max rating
        </label>
        <input
          type="number"
          id="maxRating"
          name="maxRating"
          min={0}
          max={10}
          step={0.1}
          placeholder="e.g. 9"
          defaultValue={maxRating ?? ""}
          className={inputClass}
        />
      </div>
      <div className="flex gap-2">
        <button
          type="submit"
          className="px-4 py-2 bg-primary text-primary-foreground rounded-md hover:bg-primary-hover transition-colors"
        >
          Search
        </button>
        {hasFilters && (
          <Link
            href="/movies"
            className="px-4 py-2 rounded-md border border-border hover:bg-background transition-colors flex items-center"
          >
            Clear
          </Link>
        )}
      </div>
    </Form>
  );
}
