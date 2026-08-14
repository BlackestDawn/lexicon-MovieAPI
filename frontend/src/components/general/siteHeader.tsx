import MobileNav from "./mobileNav";
import { AccountMenu } from "./accountMenu";

export function SiteHeader() {
  return (
    <div className="relative flex items-center justify-between md:justify-end">
      {/* justify-between spreads MobileNav/AccountMenu apart on mobile; at
          md+, MobileNav is display:none, so justify-between would collapse
          to a single left-aligned child without the md:justify-end override. */}
      <MobileNav />
      <AccountMenu />
    </div>
  );
}
