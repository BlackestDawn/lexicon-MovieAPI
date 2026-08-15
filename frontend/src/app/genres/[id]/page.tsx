import LoadingSpinner from "@/components/general/loadingSpinner";
import GenreDetails from "@/components/genres/genreDetails";
import { notFound } from "next/navigation";
import { Suspense } from "react";

export default async function Page({
  params,
  searchParams,
}: {
  params: Promise<{ id: string }>;
  searchParams: Promise<{ page?: string }>;
}) {
  const { id } = await params;
  if (!id) notFound();

  const { page } = await searchParams;

  return (
    <Suspense fallback={<LoadingSpinner />}>
      <GenreDetails id={id} page={page ? Number(page) : undefined} />
    </Suspense>
  );
}
