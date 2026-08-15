import LoadingSpinner from "@/components/general/loadingSpinner";
import MovieList from "@/components/movies/movieList";
import { Suspense } from "react";

export default async function Page({
  searchParams,
}: {
  searchParams: Promise<{ page?: string }>;
}) {
  const { page } = await searchParams;

  return (
    <Suspense fallback={<LoadingSpinner />}>
      <MovieList page={page ? Number(page) : undefined} />
    </Suspense>
  );
}
