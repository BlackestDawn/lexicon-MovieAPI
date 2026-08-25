import Link from "next/link";
import { Star } from "lucide-react";
import { fetchMyReviews } from "@/lib/actions/review";
import PaginationControls from "../general/paginationControls";
import { metaClass, panelClass, sectionHeadingClass } from "@/lib/data/consts/styles";

export default async function MyReviewsList({
  page,
  otherParams,
}: {
  page?: number;
  // Other lists' page numbers on the same /user page (e.g. favoritesPage) -
  // passed through so paging this list doesn't reset them.
  otherParams?: Record<string, string | number | undefined>;
}) {
  const { reviews, pagination } = await fetchMyReviews(page);

  return (
    <div className="space-y-4">
      <h3 className={sectionHeadingClass}>My reviews</h3>
      {reviews.length === 0 ? (
        <p className="text-sm text-muted-foreground">
          You haven&apos;t posted any reviews yet.
        </p>
      ) : (
        <div className="space-y-3">
          {reviews.map((review) => (
            <Link key={review.id} href={`/movies/${review.movieId}/${review.id}`}>
              <div className={`${panelClass} space-y-1`}>
                <div className="flex items-center justify-between gap-4">
                  <h4 className="font-medium">{review.movieTitle}</h4>
                  <span className={metaClass}>
                    <Star className="w-4 h-4 text-primary" />
                    {review.score}/10
                  </span>
                </div>
                <p className="text-sm text-muted-foreground line-clamp-2">
                  {review.body}
                </p>
              </div>
            </Link>
          ))}
        </div>
      )}
      {pagination && (
        <PaginationControls
          pagination={pagination}
          basePath="/user"
          pageParam="reviewsPage"
          queryParams={otherParams}
        />
      )}
    </div>
  );
}
