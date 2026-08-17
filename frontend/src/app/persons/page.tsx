import LoadingSpinner from "@/components/general/loadingSpinner";
import PersonsList from "@/components/persons/personList";
import { Suspense } from "react";

export default async function Page({
  searchParams,
}: {
  searchParams: Promise<{
    page?: string;
    name?: string;
    genre?: string;
    year?: string;
  }>;
}) {
  const { page, name, genre, year } = await searchParams;

  return (
    <Suspense fallback={<LoadingSpinner />}>
      <PersonsList
        page={page ? Number(page) : undefined}
        name={name}
        genre={genre}
        year={year ? Number(year) : undefined}
      />
    </Suspense>
  );
}
