import Link from "next/link";
import { BaseContainer } from "./baseContainer";

export function SiteFooter() {
  return (
    <BaseContainer className="py-8">
      <div className="px-8 py-8 text-center text-sm text-zinc-600 dark:text-zinc-400">
        For the full picture — endpoints, auth flows, versioning, and how
        to run the API — see the project <Link href={"https://github.com/BlackestDawn/lexicon-MovieAPI"}>repository</Link>.
      </div>
    </BaseContainer>
  );
}