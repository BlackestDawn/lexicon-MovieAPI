import { Film } from "lucide-react";
import RestrictedComponent from "../auth/restrictedComponent";
import { fetchMovies } from "@/lib/actions/movie";
import MovieCreateButton from "./movieCreateButton";
import MovieCard from "./movieCard";
import PaginationControls from "../general/paginationControls";
import MovieFilters from "./movieFilters";
import { sectionHeadingClass } from "@/lib/data/consts/styles";

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
          <MovieCard key={movie.id} movie={movie} manageable />
        ))}
      </div>
    </div>
  );
}
