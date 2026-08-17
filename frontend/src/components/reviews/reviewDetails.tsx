import { getReview, removeReview } from "@/lib/actions/review";
import { notFound } from "next/navigation";
import RestrictedComponent from "../auth/restrictedComponent";
import SimpleDeleteButton from "../general/buttons/simpleDeleteButton";
import ReviewEditButton from "./reviewEditButton";

export default async function ReviewDetails({
  movieId,
  id,
}: {
  movieId: string;
  id: string;
}) {
  const review = await getReview(movieId, id);
  if (!review) notFound();

  return (
    <div className="w-full m-4 space-y-6">
      <div className="flex justify-between items-start">
        <div>
          <h3 className="text-3xl text-slate-800 dark:text-slate-200">
            {review.authorName}
          </h3>
          <p>Score: {review.score}/10</p>
        </div>
        <RestrictedComponent accessLevel="ModeratorAndAbove" id={review.userId}>
          <div className="space-x-4">
            <ReviewEditButton movieId={movieId} review={review} />
            <SimpleDeleteButton
              id={review.id}
              redirectTo={`/movies/${movieId}`}
              onDelete={async (reviewId) => {
                "use server";
                removeReview(movieId, reviewId);
              }}
            />
          </div>
        </RestrictedComponent>
      </div>
      <p>{review.body}</p>
      <div className="text-sm text-slate-500 dark:text-slate-400">
        <p>Posted: {review.createdAt.toDateString()}</p>
        {review.updatedAt.getTime() !== review.createdAt.getTime() && (
          <p>Last updated: {review.updatedAt.toDateString()}</p>
        )}
      </div>
    </div>
  );
}
