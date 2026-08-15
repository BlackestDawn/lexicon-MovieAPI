import LoadingSpinner from "@/components/general/loadingSpinner";
import PersonsList from "@/components/persons/personList";
import { Suspense } from "react";

export default async function Page({
  searchParams,
}: {
  searchParams: Promise<{ page?: string }>;
}) {
  const { page } = await searchParams;

  return (
    <Suspense fallback={<LoadingSpinner />}>
      <PersonsList page={page ? Number(page) : undefined} />
    </Suspense>
  );
}
