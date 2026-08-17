import LoadingSpinner from "@/components/general/loadingSpinner";
import MovieDetails from "@/components/movies/movieDetails";
import { notFound } from "next/navigation";
import { Suspense } from "react";

export default async function Page({
  params,
  searchParams,
}: {
  params: Promise<{ id: string }>;
  searchParams: Promise<{
    page?: string;
    search?: string;
    minScore?: string;
    maxScore?: string;
  }>;
}) {
  const { id } = await params;
  if (!id) notFound();

  const { page, search, minScore, maxScore } = await searchParams;

  return (
    <Suspense fallback={<LoadingSpinner />}>
      <MovieDetails
        id={id}
        page={page ? Number(page) : undefined}
        search={search}
        minScore={minScore ? Number(minScore) : undefined}
        maxScore={maxScore ? Number(maxScore) : undefined}
      />
    </Suspense>
  );
}
