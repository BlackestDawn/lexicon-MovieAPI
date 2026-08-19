import { BaseContainer } from "@/components/general/baseContainer";
import Welcome from "@/components/general/welcome";

export default function Home() {
  return (
    <BaseContainer className="py-8">
      <h1 className="text-4xl font-semibold tracking-tight text-foreground">
        MovieAPI
      </h1>
      <p className="mt-2 max-w-xl text-lg leading-8 text-muted-foreground">
        A small-scale IMDB clone: browse and manage movies, persons, genres,
        and reviews behind a JWT-secured, role-based API.
      </p>

      <div className="mt-8">
        <Welcome />
      </div>
      <section className="grid grid-cols-1 divide-y divide-border border-y border-border text-left sm:grid-cols-3 sm:divide-x sm:divide-y-0">
        <div className="flex flex-col gap-2 px-8 py-8">
          <h2 className="font-medium text-foreground">Catalog</h2>
          <p className="text-sm leading-6 text-muted-foreground">
            Full CRUD for movies, persons, genres, and reviews, with filtering,
            pagination, and versioned responses (<code>/api/v1</code>,{" "}
            <code>/api/v2</code>, <code>/api/v3</code>).
          </p>
        </div>
        <div className="flex flex-col gap-2 px-8 py-8">
          <h2 className="font-medium text-foreground">Auth</h2>
          <p className="text-sm leading-6 text-muted-foreground">
            JWT access + refresh tokens, a four-tier role hierarchy, and
            per-request revocation via a security-stamp check.
          </p>
        </div>
        <div className="flex flex-col gap-2 px-8 py-8">
          <h2 className="font-medium text-foreground">Status</h2>
          <p className="text-sm leading-6 text-muted-foreground">
            This frontend is a fresh skeleton — no backend integration yet.
          </p>
        </div>
      </section>
    </BaseContainer>
  );
}
