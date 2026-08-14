import { MovieExtendedDto } from "@/lib/data/models/movieTypes";
import GenreBadge from "../genres/genreBadge";
import { minsToDisplayRuntime } from "@/lib/data/utils/converters";
import RestrictedComponent from "../auth/restrictedComponent";
import MovieDeleteButton from "./movieDeleteButton";

export default function MovieDetails({ movie }: { movie: MovieExtendedDto }) {
  return (
    <div className="w-full m-4 space-y-6">
      <div className="flex justify-between">
        <h3 className="text-center text-4xl text-slate-800 dark:text-slate-200">
          {movie.title}
        </h3>
        <div className="space-x-4">
          <RestrictedComponent accessLevel="PowerUserAndAbove">
            <div></div> {/** edit form */}
          </RestrictedComponent>
          <RestrictedComponent accessLevel="ModeratorAndAbove">
            <MovieDeleteButton id={movie.id} redirect={true} />
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
        <p>Summery:</p>
        <p>{movie.details?.synopsis ?? movie.plotSummery}</p>
      </div>
      {/** TODO: add in people cards */}
      <div>
        <p>Average rating: {movie.averageRating}/10</p>
        {/** TODO: add in reviews */}
      </div>
    </div>
  );
}
