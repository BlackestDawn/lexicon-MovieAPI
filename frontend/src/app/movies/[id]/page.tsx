import LoadingSpinner from "@/components/general/loadingSpinner";
import MovieDetails from "@/components/movies/movieDetails";
import { getMovie } from "@/lib/actions/movie";
import { Suspense } from "react";

export default async function Page({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  const movie = await getMovie(id);

  return (
    <Suspense fallback={<LoadingSpinner />}>
      <MovieDetails movie={movie} />
    </Suspense>
  );
}
