import Link from "next/link";
import { Clock, Star, Film } from "lucide-react";
import GenreBadge from "../genres/genreBadge";
import { minsToDisplayRuntime } from "@/lib/data/utils/converters";
import RestrictedComponent from "../auth/restrictedComponent";
import SimpleDeleteButton from "../general/buttons/simpleDeleteButton";
import { fetchMovies, removeMovie } from "@/lib/actions/movie";
import MovieCreateButton from "./movieCreateButton";
import PaginationControls from "../general/paginationControls";
import MovieFilters from "./movieFilters";
import { cardClass, metaClass, sectionHeadingClass } from "@/lib/data/consts/styles";

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
    <div className="my-8 space-y-6">
      <div className="text-center space-y-4">
        <h3 className={sectionHeadingClass}>Browse movies</h3>
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
        <div className="flex flex-col items-center gap-2 py-12 text-muted-foreground">
          <Film className="w-8 h-8" />
          <p>No movies found matching your filters.</p>
        </div>
      )}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        {movies.map((movie) => (
          <Link key={movie.id} href={`/movies/${movie.id}`}>
            <div className={`${cardClass} flex flex-col gap-3 p-5`}>
              <div className="flex justify-between items-start gap-2">
                <h3 className="text-xl font-medium">
                  {movie.title}{" "}
                  <span className="text-muted-foreground font-normal">
                    ({movie.releaseDate.getFullYear()})
                  </span>
                </h3>
                <RestrictedComponent accessLevel="ModeratorAndAbove">
                  <SimpleDeleteButton id={movie.id} onDelete={removeMovie} />
                </RestrictedComponent>
              </div>
              <div className="flex gap-4">
                <span className={metaClass}>
                  <Clock className="w-4 h-4" />
                  {minsToDisplayRuntime(movie.runtimeMinutes)}
                </span>
                <span className={metaClass}>
                  <Star className="w-4 h-4" />
                  {movie.averageRating}/10
                </span>
              </div>
              <p className="text-sm text-muted-foreground line-clamp-2">
                {movie.plotSummery}
              </p>
              <div className="flex flex-wrap gap-2 mt-auto pt-1">
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
