import LoadingSpinner from "@/components/general/loadingSpinner";
import MovieList from "@/components/movies/movieList";
import { Suspense } from "react";

export default async function Page() {
  return (
    <Suspense fallback={<LoadingSpinner />}>
      <MovieList />
    </Suspense>
  );
}
