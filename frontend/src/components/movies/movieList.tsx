import { MovieDto } from "@/lib/data/models/movieTypes";
import Link from "next/link";
import GenreBadge from "../genres/genreBadge";
import { minsToDisplayRuntime } from "@/lib/data/utils/converters";
import RestrictedComponent from "../auth/restrictedComponent";
import MovieDeleteButton from "./movieDeleteButton";
import { fetchMovies } from "@/lib/actions/movie";
import MovieCreateButton from "./movieCreateButton";

export default async function MovieList() {
  const movies: MovieDto[] = await fetchMovies();

  return (
    <div className="my-8 space-y-4">
      <div className="w-full flex justify-center">
        <RestrictedComponent accessLevel="PowerUserAndAbove">
          <MovieCreateButton />
        </RestrictedComponent>
      </div>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 p-4">
        {movies.map((movie) => (
          <Link key={movie.id} href={`/movies/${movie.id}`}>
            <div className="h-full p-4 border border-slate-600 dark:border-slate-300 rounded-lg space-y-2">
              <div className="flex justify-between">
                <h3 className="text-xl">
                  {movie.title} ({movie.releaseDate.getFullYear()})
                </h3>
                <div className="space-x-4">
                  <RestrictedComponent accessLevel="PowerUserAndAbove">
                    <div></div> {/** edit form */}
                  </RestrictedComponent>
                  <RestrictedComponent accessLevel="ModeratorAndAbove">
                    <MovieDeleteButton id={movie.id} />
                  </RestrictedComponent>
                </div>
              </div>
              <p className="text-sm text-slate-500 dark:text-slate-300">
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
