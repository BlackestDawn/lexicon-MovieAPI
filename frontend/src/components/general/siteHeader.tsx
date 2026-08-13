import { BaseContainer } from "./baseContainer";
import MobileNav from "./mobileNav";
import { AccountMenu } from "./accountMenu";

export function SiteHeader() {
  return (
    <BaseContainer className="py-4">
      {/* justify-between spreads MobileNav/AccountMenu apart on mobile; at
          md+, MobileNav is display:none, so justify-between would collapse
          to a single left-aligned child without the md:justify-end override. */}
      <div className="relative flex items-center justify-between md:justify-end">
        <MobileNav />
        <AccountMenu />
      </div>
    </BaseContainer>
  );
}
