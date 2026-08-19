import { getPerson, removePerson } from "@/lib/actions/person";
import { notFound } from "next/navigation";
import RestrictedComponent from "../auth/restrictedComponent";
import Link from "next/link";
import { Calendar, Clapperboard } from "lucide-react";
import { personRoleLabels } from "@/lib/data/models/personRoleTypes";
import SimpleDeleteButton from "../general/buttons/simpleDeleteButton";
import PersonEditButton from "./personEditButton";
import { cardClass, metaClass } from "@/lib/data/consts/styles";

export default async function PersonDetails({ id }: { id: string }) {
  const person = await getPerson(id);
  if (!person) notFound();

  return (
    <div className="w-full my-8 space-y-8">
      <div className="flex flex-col sm:flex-row justify-between items-start gap-4">
        <div className="space-y-3">
          <h1 className="text-4xl font-bold tracking-tight text-foreground">
            {person.givenName}{" "}
            {person.middleName ? `${person.middleName} ` : ""}
            {person.lastName}
          </h1>
          <span className={metaClass}>
            <Calendar className="w-4 h-4" />
            Born {person.dateOfBirth.toDateString()}
          </span>
        </div>
        <div className="flex gap-3 shrink-0">
          <RestrictedComponent accessLevel="PowerUserAndAbove">
            <PersonEditButton person={person} />
          </RestrictedComponent>
          <RestrictedComponent accessLevel="ModeratorAndAbove">
            <SimpleDeleteButton
              id={person.id}
              redirectTo="/persons"
              onDelete={removePerson}
            />
          </RestrictedComponent>
        </div>
      </div>

      <section className="space-y-4">
        <h2 className="flex items-center gap-2 text-xl font-semibold text-foreground">
          <Clapperboard className="w-5 h-5 text-primary" />
          Filmography
        </h2>
        {person.movieRoles.length > 0 ? (
          <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-4">
            {person.movieRoles.map((mr) => (
              <Link key={mr.movieId} href={`/movies/${mr.movieId}`}>
                <div className={`${cardClass} p-4 text-center space-y-1`}>
                  <p className="text-xs text-primary font-medium uppercase tracking-wide">
                    {personRoleLabels[mr.role]}
                  </p>
                  <p className="font-medium">{mr.title}</p>
                </div>
              </Link>
            ))}
          </div>
        ) : (
          <p className="text-muted-foreground">No movie roles registered</p>
        )}
      </section>
    </div>
  );
}
