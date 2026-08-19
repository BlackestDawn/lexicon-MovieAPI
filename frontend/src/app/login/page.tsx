import LoginForm from "@/components/auth/loginForm";
import { BaseContainer } from "@/components/general/baseContainer";

export default async function Page({
  searchParams,
}: {
  searchParams: Promise<{ redirectTo?: string }>;
}) {
  const { redirectTo } = await searchParams;
  return (
    <BaseContainer>
      <div className="mt-8">
        <LoginForm fromPage={true} redirectTo={redirectTo} />
      </div>
    </BaseContainer>
  );
}
