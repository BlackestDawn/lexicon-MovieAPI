import RestrictedPage from "@/components/auth/restrictedPage";
import ProfileSummaryCard from "@/components/user/profileSummaryCard";
import MyReviewsList from "@/components/reviews/myReviewsList";
import { sectionHeadingClass } from "@/lib/data/consts/styles";

export default async function Page({
  searchParams,
}: {
  searchParams: Promise<{ page?: string }>;
}) {
  const { page } = await searchParams;

  return (
    <RestrictedPage>
      <div className="my-8 space-y-6">
        <h3 className={sectionHeadingClass}>Your account</h3>
        <ProfileSummaryCard />
        <MyReviewsList page={page ? Number(page) : undefined} />
      </div>
    </RestrictedPage>
  );
}
