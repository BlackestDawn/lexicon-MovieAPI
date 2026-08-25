import { fetchMyFavorites } from "@/lib/actions/favorite";
import MovieCard from "./movieCard";
import FavoriteButton from "./favoriteButton";
import PaginationControls from "../general/paginationControls";
import { sectionHeadingClass } from "@/lib/data/consts/styles";

export default async function MyFavoritesList({
  page,
  otherParams,
}: {
  page?: number;
  // Other lists' page numbers on the same /user page (e.g. reviewsPage) -
  // passed through so paging this list doesn't reset them.
  otherParams?: Record<string, string | number | undefined>;
}) {
  const { movies, pagination } = await fetchMyFavorites(page);

  return (
    <div className="space-y-4">
      <h3 className={sectionHeadingClass}>My favorites</h3>
      {movies.length === 0 ? (
        <p className="text-sm text-muted-foreground">
          You haven&apos;t favorited any movies yet.
        </p>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          {movies.map((movie) => (
            <MovieCard
              key={movie.id}
              movie={movie}
              actions={<FavoriteButton movieId={movie.id} initiallyFavorited />}
            />
          ))}
        </div>
      )}
      {pagination && (
        <PaginationControls
          pagination={pagination}
          basePath="/user"
          pageParam="favoritesPage"
          queryParams={otherParams}
        />
      )}
    </div>
  );
}
