import LoadingSpinner from "@/components/general/loadingSpinner";
import MovieList from "@/components/movies/movieList";
import { Suspense } from "react";

export default async function Page({
  searchParams,
}: {
  searchParams: Promise<{
    page?: string;
    search?: string;
    genre?: string;
    year?: string;
    minRating?: string;
  }>;
}) {
  const { page, search, genre, year, minRating } = await searchParams;

  return (
    <Suspense fallback={<LoadingSpinner />}>
      <MovieList
        page={page ? Number(page) : undefined}
        search={search}
        genre={genre}
        year={year ? Number(year) : undefined}
        minRating={minRating ? Number(minRating) : undefined}
      />
    </Suspense>
  );
}
