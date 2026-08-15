import LoadingSpinner from "@/components/general/loadingSpinner";
import GenreList from "@/components/genres/genreList";
import { Suspense } from "react";

export default function Page() {
  return (
    <Suspense fallback={<LoadingSpinner />}>
      <GenreList />
    </Suspense>
  );
}
