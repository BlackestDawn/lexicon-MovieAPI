import { getGenre, removeGenre } from "@/lib/actions/genre";
import Link from "next/link";
import PaginationControls from "../general/paginationControls";
import { minsToDisplayRuntime } from "@/lib/data/utils/converters";
import RestrictedComponent from "../auth/restrictedComponent";
import SimpleDeleteButton from "../general/buttons/simpleDeleteButton";
import GenreEditButton from "./genreEditButton";

export default async function GenreDetails({
  id,
  page,
}: {
  id: string;
  page?: number;
}) {
  const { genre, pagination } = await getGenre(id, { page });

  return (
    <div>
      <div className="flex justify-between items-center my-8 px-4">
        <h3 className="text-3xl">All movies within genre {genre.name}</h3>
        <div className="space-x-4">
          <RestrictedComponent accessLevel="ModeratorAndAbove">
            <GenreEditButton genre={genre} />
          </RestrictedComponent>
          <RestrictedComponent accessLevel="Administrator">
            <SimpleDeleteButton
              id={genre.id}
              redirectTo="/genres"
              onDelete={removeGenre}
            />
          </RestrictedComponent>
        </div>
      </div>
      <div className="w-full flex justify-center">
        {pagination && (
          <PaginationControls
            pagination={pagination}
            basePath={`/genres/${genre.id}`}
          />
        )}
      </div>
      {genre.movies.length > 0 ? (
        <div className="space-y-4 mb-8">
          {genre.movies.map((m) => (
            <div
              key={m.id}
              className="border border-border rounded-lg hover:border-primary transition-colors"
            >
              <Link href={`/movies/${m.id}`}>
                <div className="p-4 flex flex-col sm:flex-row justify-evenly sm:justify-center items-center">
                  <span>
                    {m.title} ({m.releaseDate.getFullYear()})
                  </span>
                  <span>Runtime: {minsToDisplayRuntime(m.runtimeMinutes)}</span>
                  <span>Rating: {m.averageRating}/10</span>
                </div>
              </Link>
            </div>
          ))}
        </div>
      ) : (
        <div className="mb-8">
          <p>No movies found</p>
        </div>
      )}
    </div>
  );
}
