import ProfileForm from "@/components/auth/profileForm";
import RestrictedPage from "@/components/auth/restrictedPage";
import { sectionHeadingClass } from "@/lib/data/consts/styles";

export default function Page() {
  return (
    <RestrictedPage>
      <div className="my-8 space-y-6">
        <h3 className={sectionHeadingClass}>Settings</h3>
        <ProfileForm />
      </div>
    </RestrictedPage>
  );
}
