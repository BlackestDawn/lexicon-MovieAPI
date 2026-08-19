import Form from "next/form";
import Link from "next/link";
import { inputClass, labelClass, panelClass } from "@/lib/data/consts/styles";

export default function ReviewFilters({
  movieId,
  search,
  minScore,
  maxScore,
}: {
  movieId: string;
  search?: string;
  minScore?: number;
  maxScore?: number;
}) {
  const hasFilters = !!search || !!minScore || !!maxScore;

  return (
    <Form
      action={`/movies/${movieId}`}
      className={`flex flex-col sm:flex-row sm:items-end gap-4 ${panelClass}`}
    >
      <div className="flex-1">
        <label htmlFor="reviewSearch" className={labelClass}>
          Search
        </label>
        <input
          type="text"
          id="reviewSearch"
          name="search"
          placeholder="Search review text"
          defaultValue={search ?? ""}
          className={inputClass}
        />
      </div>
      <div className="flex-1">
        <label htmlFor="minScore" className={labelClass}>
          Min score
        </label>
        <input
          type="number"
          id="minScore"
          name="minScore"
          min={1}
          max={10}
          placeholder="e.g. 5"
          defaultValue={minScore ?? ""}
          className={inputClass}
        />
      </div>
      <div className="flex-1">
        <label htmlFor="maxScore" className={labelClass}>
          Max score
        </label>
        <input
          type="number"
          id="maxScore"
          name="maxScore"
          min={1}
          max={10}
          placeholder="e.g. 10"
          defaultValue={maxScore ?? ""}
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
            href={`/movies/${movieId}`}
            className="px-4 py-2 rounded-md border border-border hover:bg-background transition-colors flex items-center"
          >
            Clear
          </Link>
        )}
      </div>
    </Form>
  );
}
