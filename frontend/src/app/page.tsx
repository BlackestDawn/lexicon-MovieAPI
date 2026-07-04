import Link from "next/link";

export default function Home() {
  return (
    <div className="flex flex-1 flex-col items-center bg-zinc-50 font-sans dark:bg-black">
      <main className="flex w-full max-w-3xl flex-1 flex-col">
        <header className="flex flex-col items-center gap-4 px-8 py-24 text-center">
          <h1 className="text-4xl font-semibold tracking-tight text-black dark:text-zinc-50">
            MovieAPI
          </h1>
          <p className="max-w-xl text-lg leading-8 text-zinc-600 dark:text-zinc-400">
            A small-scale IMDB clone: browse and manage movies, people,
            genres, and reviews behind a JWT-secured, role-based API.
          </p>
        </header>

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

        <footer className="px-8 py-8 text-center text-sm text-zinc-600 dark:text-zinc-400">
          For the full picture — endpoints, auth flows, versioning, and how
          to run the API — see the project <Link href={"https://github.com/BlackestDawn/lexicon-MovieAPI"}>repository</Link>.
        </footer>
      </main>
    </div>
  );
}
