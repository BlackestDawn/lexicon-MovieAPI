import { MovieExtendedDto } from "@/lib/data/models/movieTypes";
import { Calendar, Clock, Star, MessageSquareText, Users } from "lucide-react";
import GenreBadge from "../genres/genreBadge";
import { minsToDisplayRuntime } from "@/lib/data/utils/converters";
import RestrictedComponent from "../auth/restrictedComponent";
import SimpleDeleteButton from "../general/buttons/simpleDeleteButton";
import { notFound } from "next/navigation";
import { getMovie, removeMovie } from "@/lib/actions/movie";
import { fetchReviews } from "@/lib/actions/review";
import MovieEditButton from "./movieEditButton";
import Link from "next/link";
import { personRoleLabels } from "@/lib/data/models/personRoleTypes";
import ReviewCreateButton from "../reviews/reviewCreateButton";
import ReviewFilters from "../reviews/reviewFilters";
import PaginationControls from "../general/paginationControls";
import { cardClass, metaClass } from "@/lib/data/consts/styles";

export default async function MovieDetails({
  id,
  page,
  search,
  minScore,
  maxScore,
}: {
  id: string;
  page?: number;
  search?: string;
  minScore?: number;
  maxScore?: number;
}) {
  const movie: MovieExtendedDto = await getMovie(id);
  if (!movie) notFound();

  const { reviews, pagination: reviewPagination } = await fetchReviews(
    movie.id,
    { page, search, minScore, maxScore },
  );

  return (
    <div className="w-full my-8 space-y-10">
      <div className="flex flex-col sm:flex-row justify-between items-start gap-4">
        <div className="space-y-3">
          <h1 className="text-4xl font-bold tracking-tight text-foreground">
            {movie.title}
          </h1>
          <div className="flex flex-wrap gap-4">
            <span className={metaClass}>
              <Calendar className="w-4 h-4" />
              {movie.releaseDate.toDateString()}
            </span>
            <span className={metaClass}>
              <Clock className="w-4 h-4" />
              {minsToDisplayRuntime(movie.runtimeMinutes)}
            </span>
            <span className={metaClass}>
              <Star className="w-4 h-4 text-primary" />
              {movie.averageRating}/10
            </span>
          </div>
          <div className="flex flex-wrap gap-2">
            {movie.genres.map((g) => (
              <GenreBadge key={g.id} name={g.name} />
            ))}
          </div>
        </div>
        <div className="flex gap-3 shrink-0">
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

      <section className="space-y-3">
        <h2 className="text-xl font-semibold text-foreground">Synopsis</h2>
        <p className="text-muted-foreground leading-7">
          {movie.details?.synopsis ?? movie.plotSummery}
        </p>
      </section>

      <section className="space-y-4">
        <h2 className="flex items-center gap-2 text-xl font-semibold text-foreground">
          <Users className="w-5 h-5 text-primary" />
          Cast &amp; Crew
        </h2>
        <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-4">
          {movie.castCrews && movie.castCrews.length > 0 ? (
            movie.castCrews
              .sort((a, b) => a.role - b.role)
              .map((p) => (
                <Link key={p.personId} href={`/persons/${p.personId}`}>
                  <div className={`${cardClass} p-4 text-center space-y-1`}>
                    <p className="text-xs text-primary font-medium uppercase tracking-wide">
                      {personRoleLabels[p.role]}
                    </p>
                    <p className="font-medium">
                      {p.givenName} {p.middleName && p.middleName[0] + ". "}
                      {p.lastName}
                    </p>
                  </div>
                </Link>
              ))
          ) : (
            <p className="text-muted-foreground">No cast or crew registered</p>
          )}
        </div>
      </section>

      <section className="space-y-4">
        <div className="flex justify-between items-center gap-4">
          <h2 className="flex items-center gap-2 text-xl font-semibold text-foreground">
            <MessageSquareText className="w-5 h-5 text-primary" />
            Reviews
          </h2>
          <RestrictedComponent accessLevel="LoggedIn">
            <ReviewCreateButton movieId={movie.id} />
          </RestrictedComponent>
        </div>
        <ReviewFilters
          movieId={movie.id}
          search={search}
          minScore={minScore}
          maxScore={maxScore}
        />
        {reviewPagination && (
          <PaginationControls
            pagination={reviewPagination}
            basePath={`/movies/${movie.id}`}
            queryParams={{ search, minScore, maxScore }}
          />
        )}
        {reviews.length > 0 ? (
          <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-4">
            {reviews.map((r) => (
              <Link key={r.id} href={`/movies/${movie.id}/${r.id}`}>
                <div className={`${cardClass} p-4 text-center space-y-1`}>
                  <p className="font-medium">{r.authorName}</p>
                  <span className={`${metaClass} justify-center`}>
                    <Star className="w-4 h-4 text-primary" />
                    {r.score}/10
                  </span>
                </div>
              </Link>
            ))}
          </div>
        ) : (
          <p className="text-muted-foreground">
            No reviews found matching your filters.
          </p>
        )}
      </section>
    </div>
  );
}
