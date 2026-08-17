import LoadingSpinner from "@/components/general/loadingSpinner";
import ReviewDetails from "@/components/reviews/reviewDetails";
import { notFound } from "next/navigation";
import { Suspense } from "react";

export default async function Page({
  params,
}: {
  params: Promise<{ id: string; reviewId: string }>;
}) {
  const { id, reviewId } = await params;
  if (!id || !reviewId) notFound();

  return (
    <Suspense fallback={<LoadingSpinner />}>
      <ReviewDetails movieId={id} id={reviewId} />
    </Suspense>
  );
}
