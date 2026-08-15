import LoadingSpinner from "@/components/general/loadingSpinner";
import MovieDetails from "@/components/movies/movieDetails";
import { notFound } from "next/navigation";
import { Suspense } from "react";

export default async function Page({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  if (!id) notFound();

  return (
    <Suspense fallback={<LoadingSpinner />}>
      <MovieDetails id={id} />
    </Suspense>
  );
}
