import { getReview, removeReview } from "@/lib/actions/review";
import { notFound } from "next/navigation";
import { Star, Clock } from "lucide-react";
import RestrictedComponent from "../auth/restrictedComponent";
import SimpleDeleteButton from "../general/buttons/simpleDeleteButton";
import ReviewEditButton from "./reviewEditButton";
import { metaClass, panelClass } from "@/lib/data/consts/styles";

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
    <div className="w-full my-8 space-y-6">
      <div className="flex justify-between items-start gap-4">
        <div className="space-y-2">
          <h1 className="text-3xl font-bold tracking-tight text-foreground">
            {review.authorName}
          </h1>
          <span className={metaClass}>
            <Star className="w-4 h-4 text-primary" />
            {review.score}/10
          </span>
        </div>
        <RestrictedComponent accessLevel="ModeratorAndAbove" id={review.userId}>
          <div className="flex gap-3 shrink-0">
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
      <div className={`${panelClass} space-y-4`}>
        <p className="leading-7">{review.body}</p>
        <div className={`${metaClass} border-t border-border pt-3`}>
          <Clock className="w-4 h-4" />
          <span>
            Posted {review.createdAt.toDateString()}
            {review.updatedAt.getTime() !== review.createdAt.getTime() &&
              ` · updated ${review.updatedAt.toDateString()}`}
          </span>
        </div>
      </div>
    </div>
  );
}
