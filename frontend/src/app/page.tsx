import { BaseContainer } from "@/components/base/baseContainer";
import { SiteFooter } from "@/components/base/siteFooter";
import { SiteHeader } from "@/components/base/siteHeader";

export default function Home() {
  return (
    <>
      <div className="bg-gray-700">
        <SiteHeader />
      </div>

      <BaseContainer>
        <section className="grid grid-cols-1 divide-y divide-black/[.08] border-y border-black/[.08] text-left sm:grid-cols-3 sm:divide-x sm:divide-y-0 dark:divide-white/[.145] dark:border-white/[.145]">
          <div className="flex flex-col gap-2 px-8 py-8">
            <h2 className="font-medium text-black dark:text-zinc-50">
              Catalog
            </h2>
            <p className="text-sm leading-6 text-zinc-600 dark:text-zinc-400">
              Full CRUD for movies, people, genres, and reviews, with
              filtering, pagination, and versioned responses (
              <code>/api/v1</code>, <code>/api/v2</code>).
            </p>
          </div>
          <div className="flex flex-col gap-2 px-8 py-8">
            <h2 className="font-medium text-black dark:text-zinc-50">Auth</h2>
            <p className="text-sm leading-6 text-zinc-600 dark:text-zinc-400">
              JWT access + refresh tokens, a four-tier role hierarchy, and
              per-request revocation via a security-stamp check.
            </p>
          </div>
          <div className="flex flex-col gap-2 px-8 py-8">
            <h2 className="font-medium text-black dark:text-zinc-50">
              Status
            </h2>
            <p className="text-sm leading-6 text-zinc-600 dark:text-zinc-400">
              This frontend is a fresh skeleton — no backend integration yet.
            </p>
          </div>
        </section>
      </BaseContainer>

      <div className="bg-gray-700">
        <SiteFooter />
      </div>
    </>
  );
}
