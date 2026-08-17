import { MovieExtendedDto } from "@/lib/data/models/movieTypes";
import GenreBadge from "../genres/genreBadge";
import { minsToDisplayRuntime } from "@/lib/data/utils/converters";
import RestrictedComponent from "../auth/restrictedComponent";
import SimpleDeleteButton from "../general/buttons/simpleDeleteButton";
import { notFound } from "next/navigation";
import { getMovie, removeMovie } from "@/lib/actions/movie";
import MovieEditButton from "./movieEditButton";
import Link from "next/link";
import { personRoleLabels } from "@/lib/data/models/personRoleTypes";
import ReviewCreateButton from "../reviews/reviewCreateButton";

export default async function MovieDetails({ id }: { id: string }) {
  const movie: MovieExtendedDto = await getMovie(id);
  if (!movie) notFound();

  return (
    <div className="w-full m-4 space-y-6">
      <div className="flex justify-between">
        <h3 className="text-center text-4xl text-slate-800 dark:text-slate-200">
          {movie.title}
        </h3>
        <div className="space-x-4">
          <RestrictedComponent accessLevel="PowerUserAndAbove">
            <MovieEditButton movie={movie} />
          </RestrictedComponent>
          <RestrictedComponent accessLevel="ModeratorAndAbove">
            <SimpleDeleteButton
              id={movie.id}
              redirectTo="/movies"
              onDelete={removeMovie}
            />
          </RestrictedComponent>
        </div>
      </div>
      <div className="w-full flex flex-col md:flex-row justify-evenly">
        <p>Release date: {movie.releaseDate.toDateString()}</p>
        <p>Runtime: {minsToDisplayRuntime(movie.runtimeMinutes)}</p>
      </div>
      <div className="flex gap-2">
        {movie.genres.map((g) => (
          <GenreBadge key={g.id} name={g.name} />
        ))}
      </div>
      <div>
        <p className="mb-4">Summery:</p>
        <p>{movie.details?.synopsis ?? movie.plotSummery}</p>
      </div>
      <div className="p-4">
        <p className="mb-4">Cast & Crew</p>
        <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-4 mb-6">
          {movie.castCrews ? (
            movie.castCrews
              .sort((a, b) => a.role - b.role)
              .map((p) => (
                <Link key={p.personId} href={`/persons/${p.personId}`}>
                  <div className="p-4 border border-slate-600 dark:border-slate-400 rounded-md text-center">
                    <p>
                      {personRoleLabels[p.role]}: {p.givenName}{" "}
                      {p.middleName && p.middleName[0] + ". "}
                      {p.lastName}
                    </p>
                  </div>
                </Link>
              ))
          ) : (
            <p>No cast or crew registered</p>
          )}
        </div>
      </div>
      <div>
        <div className="flex justify-between items-center">
          <p>Average rating: {movie.averageRating}/10</p>
          <RestrictedComponent accessLevel="LoggedIn">
            <ReviewCreateButton movieId={movie.id} />
          </RestrictedComponent>
        </div>
        <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 p-4 gap-4">
          {movie.reviews.length > 0 ? (
            movie.reviews.map((r) => (
              <Link key={r.id} href={`/movies/${movie.id}/${r.id}`}>
                <div className="border border-slate-600 dark:border-slate-400 rounded-md text-center p-4">
                  <p>
                    {r.authorName} {r.score} / 10
                  </p>
                </div>
              </Link>
            ))
          ) : (
            <div></div>
          )}
        </div>
      </div>
    </div>
  );
}
