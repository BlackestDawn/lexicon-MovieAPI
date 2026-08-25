import RestrictedPage from "@/components/auth/restrictedPage";
import ProfileSummaryCard from "@/components/user/profileSummaryCard";
import MyReviewsList from "@/components/reviews/myReviewsList";
import MyFavoritesList from "@/components/movies/myFavoritesList";
import { sectionHeadingClass } from "@/lib/data/consts/styles";

export default async function Page({
  searchParams,
}: {
  searchParams: Promise<{ reviewsPage?: string; favoritesPage?: string }>;
}) {
  const { reviewsPage, favoritesPage } = await searchParams;
  const reviewsPageNum = reviewsPage ? Number(reviewsPage) : undefined;
  const favoritesPageNum = favoritesPage ? Number(favoritesPage) : undefined;

  return (
    <RestrictedPage>
      <div className="my-8 space-y-6">
        <h3 className={sectionHeadingClass}>Your account</h3>
        <ProfileSummaryCard />
        <MyReviewsList
          page={reviewsPageNum}
          otherParams={{ favoritesPage: favoritesPageNum }}
        />
        <MyFavoritesList
          page={favoritesPageNum}
          otherParams={{ reviewsPage: reviewsPageNum }}
        />
      </div>
    </RestrictedPage>
  );
}
