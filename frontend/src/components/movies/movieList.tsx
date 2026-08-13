import { MovieDto } from "@/lib/data/models/movieTypes";
import Link from "next/link";

export default function MovieList({ movies }: { movies: MovieDto[] }) {
  return (
    <div>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 p-4 my-8">
        {movies.map((movie) => (
          <Link key={movie.id} href={`/movies/${movie.id}`}>
            <div className="h-full p-4 border border-slate-600 dark:border-slate-300 rounded-lg space-y-2">
              <h3 className="">
                {movie.title} ({movie.releaseDate.getFullYear()})
              </h3>
              <p>Release date: {movie.releaseDate.toDateString()}</p>
              <p>Run time: {movie.runtimeMinutes} min</p>
              <p>{movie.plotSummery}</p>
              <div className="flex gap-2">
                {movie.genres.map((g) => (
                  <span key={g.id}>{g.name}</span>
                ))}
              </div>
            </div>
          </Link>
        ))}
      </div>
    </div>
  );
}
