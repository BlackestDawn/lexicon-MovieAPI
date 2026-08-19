import { fetchGenres } from "@/lib/actions/genre";
import Link from "next/link";
import { Clapperboard } from "lucide-react";
import RestrictedComponent from "../auth/restrictedComponent";
import GenreCreateButton from "./genreCreateButton";
import { cardClass, sectionHeadingClass } from "@/lib/data/consts/styles";

export default async function GenreList() {
  const genres = await fetchGenres();

  return (
    <div className="my-8 space-y-6">
      <div className="text-center space-y-4">
        <h3 className={sectionHeadingClass}>All genres</h3>
        <RestrictedComponent accessLevel="ModeratorAndAbove">
          <GenreCreateButton />
        </RestrictedComponent>
      </div>
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        {genres.map((g) => (
          <Link key={g.id} href={`/genres/${g.id}`}>
            <div
              className={`${cardClass} flex flex-col items-center gap-2 p-6 text-center`}
            >
              <Clapperboard className="w-6 h-6 text-primary" />
              <span className="font-medium">{g.name}</span>
            </div>
          </Link>
        ))}
      </div>
    </div>
  );
}
