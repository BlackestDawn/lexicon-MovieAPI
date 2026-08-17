import LoadingSpinner from "@/components/general/loadingSpinner";
import PersonDetails from "@/components/persons/personDetails";
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
      <PersonDetails id={id} />
    </Suspense>
  );
}
