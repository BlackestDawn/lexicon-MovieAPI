import Link from "next/link";
import { Clock, Star } from "lucide-react";
import GenreBadge from "../genres/genreBadge";
import { minsToDisplayRuntime } from "@/lib/data/utils/converters";
import RestrictedComponent from "../auth/restrictedComponent";
import SimpleDeleteButton from "../general/buttons/simpleDeleteButton";
import { removeMovie } from "@/lib/actions/movie";
import { MovieDto } from "@/lib/data/models/movieTypes";
import { cardClass, metaClass } from "@/lib/data/consts/styles";

export default function MovieCard({
  movie,
  manageable = false,
}: {
  movie: MovieDto;
  manageable?: boolean;
}) {
  return (
    <Link href={`/movies/${movie.id}`}>
      <div className={`${cardClass} flex flex-col gap-3 p-5`}>
        <div className="flex justify-between items-start gap-2">
          <h3 className="text-xl font-medium">
            {movie.title}{" "}
            <span className="text-muted-foreground font-normal">
              ({movie.releaseDate.getFullYear()})
            </span>
          </h3>
          {manageable && (
            <RestrictedComponent accessLevel="ModeratorAndAbove">
              <SimpleDeleteButton id={movie.id} onDelete={removeMovie} />
            </RestrictedComponent>
          )}
        </div>
        <div className="flex gap-4">
          <span className={metaClass}>
            <Clock className="w-4 h-4" />
            {minsToDisplayRuntime(movie.runtimeMinutes)}
          </span>
          <span className={metaClass}>
            <Star className="w-4 h-4 text-primary" />
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
  );
}
