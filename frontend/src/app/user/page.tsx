import RestrictedPage from "@/components/auth/restrictedPage";
import ProfileSummaryCard from "@/components/user/profileSummaryCard";
import { sectionHeadingClass } from "@/lib/data/consts/styles";

export default function Page() {
  return (
    <RestrictedPage>
      <div className="my-8 space-y-6">
        <h3 className={sectionHeadingClass}>Your account</h3>
        <ProfileSummaryCard />
      </div>
    </RestrictedPage>
  );
}
