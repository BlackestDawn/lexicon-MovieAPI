import Link from "next/link";
import GenreBadge from "../genres/genreBadge";
import { minsToDisplayRuntime } from "@/lib/data/utils/converters";
import RestrictedComponent from "../auth/restrictedComponent";
import SimpleDeleteButton from "../general/buttons/simpleDeleteButton";
import { fetchMovies, removeMovie } from "@/lib/actions/movie";
import MovieCreateButton from "./movieCreateButton";
import PaginationControls from "../general/paginationControls";
import MovieFilters from "./movieFilters";

export default async function MovieList({
  page,
  search,
  genre,
  year,
  minRating,
  maxRating,
}: {
  page?: number;
  search?: string;
  genre?: string;
  year?: number;
  minRating?: number;
  maxRating?: number;
}) {
  const { movies, pagination } = await fetchMovies({
    page,
    search,
    genre,
    year,
    minRating,
    maxRating,
  });

  return (
    <div className="my-8 space-y-4">
      <div className="w-full flex justify-center">
        <RestrictedComponent accessLevel="PowerUserAndAbove">
          <MovieCreateButton />
        </RestrictedComponent>
      </div>
      <MovieFilters
        search={search}
        genre={genre}
        year={year}
        minRating={minRating}
        maxRating={maxRating}
      />
      {pagination && (
        <PaginationControls
          pagination={pagination}
          basePath="/movies"
          queryParams={{ search, genre, year, minRating, maxRating }}
        />
      )}
      {movies.length === 0 && (
        <p className="text-center text-muted-foreground">
          No movies found matching your filters.
        </p>
      )}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 p-4">
        {movies.map((movie) => (
          <Link key={movie.id} href={`/movies/${movie.id}`}>
            <div className="h-full p-4 border border-border rounded-lg space-y-2 hover:border-primary transition-colors">
              <div className="flex justify-between">
                <h3 className="text-xl">
                  {movie.title} ({movie.releaseDate.getFullYear()})
                </h3>
                <div className="space-x-4">
                  <RestrictedComponent accessLevel="ModeratorAndAbove">
                    <SimpleDeleteButton id={movie.id} onDelete={removeMovie} />
                  </RestrictedComponent>
                </div>
              </div>
              <p className="text-sm text-muted-foreground">
                Runtime: {minsToDisplayRuntime(movie.runtimeMinutes)}
              </p>
              <p>{movie.plotSummery}</p>
              <div className="flex gap-2">
                {movie.genres.map((g) => (
                  <GenreBadge key={g.id} name={g.name} />
                ))}
              </div>
            </div>
          </Link>
        ))}
      </div>
    </div>
  );
}
