import { getGenre, removeGenre } from "@/lib/actions/genre";
import Link from "next/link";
import { Clock, Star, Film } from "lucide-react";
import PaginationControls from "../general/paginationControls";
import { minsToDisplayRuntime } from "@/lib/data/utils/converters";
import RestrictedComponent from "../auth/restrictedComponent";
import SimpleDeleteButton from "../general/buttons/simpleDeleteButton";
import GenreEditButton from "./genreEditButton";
import { cardClass, metaClass, sectionHeadingClass } from "@/lib/data/consts/styles";

export default async function GenreDetails({
  id,
  page,
}: {
  id: string;
  page?: number;
}) {
  const { genre, pagination } = await getGenre(id, { page });

  return (
    <div className="my-8 space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-center gap-4">
        <h3 className={sectionHeadingClass}>Movies in {genre.name}</h3>
        <div className="flex gap-3">
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
        <div className="space-y-3">
          {genre.movies.map((m) => (
            <Link key={m.id} href={`/movies/${m.id}`}>
              <div
                className={`${cardClass} flex flex-col sm:flex-row sm:items-center justify-between gap-2 p-4`}
              >
                <span className="font-medium">
                  {m.title}{" "}
                  <span className="text-muted-foreground font-normal">
                    ({m.releaseDate.getFullYear()})
                  </span>
                </span>
                <div className="flex gap-4">
                  <span className={metaClass}>
                    <Clock className="w-4 h-4" />
                    {minsToDisplayRuntime(m.runtimeMinutes)}
                  </span>
                  <span className={metaClass}>
                    <Star className="w-4 h-4" />
                    {m.averageRating}/10
                  </span>
                </div>
              </div>
            </Link>
          ))}
        </div>
      ) : (
        <div className="flex flex-col items-center gap-2 py-12 text-muted-foreground">
          <Film className="w-8 h-8" />
          <p>No movies found</p>
        </div>
      )}
    </div>
  );
}
