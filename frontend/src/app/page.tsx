import Link from "next/link";
import { Clapperboard, ShieldCheck, Info } from "lucide-react";
import { BaseContainer } from "@/components/general/baseContainer";
import Welcome from "@/components/general/welcome";
import { cardClass } from "@/lib/data/consts/styles";

const features = [
  {
    icon: Clapperboard,
    title: "Catalog",
    body: (
      <>
        Full CRUD for movies, persons, genres, and reviews, with filtering,
        pagination, and versioned responses (<code>/api/v1</code>,{" "}
        <code>/api/v2</code>, <code>/api/v3</code>).
      </>
    ),
  },
  {
    icon: ShieldCheck,
    title: "Auth",
    body: "JWT access + refresh tokens, a four-tier role hierarchy, and per-request revocation via a security-stamp check.",
  },
  {
    icon: Info,
    title: "Status",
    body: "This frontend is a fresh skeleton — no backend integration yet.",
  },
];

export default function Home() {
  return (
    <BaseContainer className="py-8 space-y-12">
      <section className="rounded-2xl border border-border bg-linear-to-br from-primary/10 via-surface to-accent/10 px-8 py-16 sm:px-12 text-center sm:text-left">
        <div className="max-w-2xl mx-auto sm:mx-0 space-y-4">
          <h1 className="text-5xl font-bold tracking-tight text-foreground">
            MovieAPI
          </h1>
          <p className="text-lg leading-8 text-muted-foreground">
            A small-scale IMDB clone: browse and manage movies, persons,
            genres, and reviews behind a JWT-secured, role-based API.
          </p>
          <Welcome />
          <div className="flex flex-wrap gap-3 justify-center sm:justify-start pt-2">
            <Link
              href="/movies"
              className="px-5 py-2.5 bg-primary text-primary-foreground rounded-md font-medium hover:bg-primary-hover transition-colors"
            >
              Browse movies
            </Link>
            <Link
              href="/genres"
              className="px-5 py-2.5 border border-border rounded-md font-medium hover:bg-background transition-colors"
            >
              Explore genres
            </Link>
          </div>
        </div>
      </section>

      <section className="grid grid-cols-1 sm:grid-cols-3 gap-6">
        {features.map(({ icon: Icon, title, body }) => (
          <div key={title} className={`${cardClass} flex flex-col gap-3 p-6`}>
            <Icon className="w-6 h-6 text-primary" />
            <h2 className="font-medium text-foreground">{title}</h2>
            <p className="text-sm leading-6 text-muted-foreground">{body}</p>
          </div>
        ))}
      </section>
    </BaseContainer>
  );
}
