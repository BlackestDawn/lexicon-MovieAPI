import Link from "next/link";
import { ArrowRight, ShieldCheck, Users, Star } from "lucide-react";
import { BaseContainer } from "@/components/general/baseContainer";
import Welcome from "@/components/general/welcome";
import MovieCard from "@/components/movies/movieCard";
import GenreBadge from "@/components/genres/genreBadge";
import { fetchMovies } from "@/lib/actions/movie";
import { fetchGenres } from "@/lib/actions/genre";
import { sectionHeadingClass } from "@/lib/data/consts/styles";

const about = [
  {
    icon: Star,
    text: "Full CRUD for movies, persons, genres, and reviews, with filtering, pagination, and versioned responses.",
  },
  {
    icon: ShieldCheck,
    text: "JWT access + refresh tokens with a four-tier role hierarchy and per-request revocation.",
  },
  {
    icon: Users,
    text: "Built as a portfolio project — an IMDB-style catalog backed by a role-based .NET API.",
  },
];

export default async function Home() {
  const [{ movies }, genres] = await Promise.all([
    fetchMovies({ pageSize: 6 }),
    fetchGenres(),
  ]);

  return (
    <BaseContainer className="py-8 space-y-14">
      <section className="rounded-2xl border border-border bg-linear-to-br from-primary/10 via-surface to-accent/10 px-8 py-16 sm:px-12 text-center sm:text-left">
        <div className="max-w-2xl mx-auto sm:mx-0 space-y-4">
          <h1 className="text-5xl font-bold tracking-tight text-foreground">
            Discover your next favorite film
          </h1>
          <p className="text-lg leading-8 text-muted-foreground">
            Browse movies, explore cast &amp; crew, and read what other
            viewers think — all in one catalog.
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

      {genres.length > 0 && (
        <section className="space-y-4">
          <h2 className={sectionHeadingClass}>Browse by genre</h2>
          <div className="flex flex-wrap gap-2">
            {genres.map((g) => (
              <Link key={g.id} href={`/genres/${g.id}`}>
                <GenreBadge name={g.name} />
              </Link>
            ))}
          </div>
        </section>
      )}

      {movies.length > 0 && (
        <section className="space-y-4">
          <div className="flex justify-between items-end gap-4">
            <h2 className={sectionHeadingClass}>From the catalog</h2>
            <Link
              href="/movies"
              className="flex items-center gap-1 text-sm font-medium text-primary hover:text-primary-hover transition-colors whitespace-nowrap"
            >
              Browse all
              <ArrowRight className="w-4 h-4" />
            </Link>
          </div>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {movies.map((movie) => (
              <MovieCard key={movie.id} movie={movie} />
            ))}
          </div>
        </section>
      )}

      <section className="border-t border-border pt-8">
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-6">
          {about.map(({ icon: Icon, text }) => (
            <div key={text} className="flex items-start gap-3">
              <Icon className="w-5 h-5 text-primary shrink-0 mt-0.5" />
              <p className="text-sm text-muted-foreground leading-6">{text}</p>
            </div>
          ))}
        </div>
      </section>
    </BaseContainer>
  );
}
