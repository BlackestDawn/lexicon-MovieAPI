import LoadingSpinner from "@/components/general/loadingSpinner";
import MovieDetails from "@/components/movies/movieDetails";
import { getMovie } from "@/lib/actions/movie";
import { notFound } from "next/navigation";
import { Suspense } from "react";

export default async function Page({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  if (!id) notFound();

  const movie = await getMovie(id);
  if (!movie) notFound();

  return (
    <Suspense fallback={<LoadingSpinner />}>
      <MovieDetails movie={movie} />
    </Suspense>
  );
}
