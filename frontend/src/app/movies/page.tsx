import { BaseContainer } from "@/components/general/baseContainer";
import LoadingSpinner from "@/components/general/loadingSpinner";
import MovieList from "@/components/movies/movieList";
import { fetchMovies } from "@/lib/actions/movie";
import { Suspense } from "react";

export default async function Page() {
  const movieList = await fetchMovies();

  return (
    <Suspense fallback={<LoadingSpinner />}>
      <BaseContainer>
        <MovieList movies={movieList} />
      </BaseContainer>
    </Suspense>
  );
}
