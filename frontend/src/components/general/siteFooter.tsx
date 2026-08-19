import Link from "next/link";

export function SiteFooter() {
  const currentYear = new Date().getUTCFullYear();

  return (
    <>
      <div className="pb-4">
        For the full picture — endpoints, auth flows, versioning, and how to run
        the API — see the project{" "}
        <Link
          href={"https://github.com/BlackestDawn/lexicon-MovieAPI"}
          className="text-muted-foreground hover:text-foreground transition-colors"
        >
          repository
        </Link>
        .
      </div>
      <div className="flex flex-col md:flex-row justify-evenly items-center gap-4">
        <div>
          <p>© {currentYear} Alexander Stauch. All rights reserved.</p>
          <p className="mt-1 text-xs">
            Licensed under{" "}
            <Link
              href="/licensing"
              className="text-primary hover:underline"
            >
              Creative commons Attribution 4.0 International
            </Link>
          </p>
        </div>

        <div className="flex gap-6 text-sm">
          <Link
            href="/cookie-policy"
            className="text-muted-foreground hover:text-foreground transition-colors"
          >
            Cookie Policy
          </Link>
        </div>
      </div>
    </>
  );
}
