import ChangePasswordForm from "@/components/auth/changePasswordForm";
import RestrictedPage from "@/components/auth/restrictedPage";
import { sectionHeadingClass } from "@/lib/data/consts/styles";

export default function Page() {
  return (
    <RestrictedPage>
      <div className="my-8 space-y-6">
        <h3 className={sectionHeadingClass}>Security</h3>
        <ChangePasswordForm />
      </div>
    </RestrictedPage>
  );
}
