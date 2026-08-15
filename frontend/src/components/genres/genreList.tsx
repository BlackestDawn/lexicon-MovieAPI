import { fetchGenres } from "@/lib/actions/genre";
import Link from "next/link";
import GenreBadge from "./genreBadge";

export default async function GenreList() {
  const genres = await fetchGenres();

  return (
    <div>
      <div className="my-6">
        <h3 className="w-full text-center text-3xl">All genres</h3>
      </div>
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-center mb-8">
        {genres.map((g) => (
          <Link key={g.id} href={`/genres/${g.id}`}>
            <GenreBadge name={g.name} />
          </Link>
        ))}
      </div>
    </div>
  );
}
