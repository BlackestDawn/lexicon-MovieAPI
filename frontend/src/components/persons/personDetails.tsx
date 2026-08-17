import { getPerson, removePerson } from "@/lib/actions/person";
import { notFound } from "next/navigation";
import RestrictedComponent from "../auth/restrictedComponent";
import Link from "next/link";
import { personRoleLabels } from "@/lib/data/models/personRoleTypes";
import SimpleDeleteButton from "../general/buttons/simpleDeleteButton";

export default async function PersonDetails({ id }: { id: string }) {
  const person = await getPerson(id);
  if (!person) notFound();

  return (
    <div className="w-full m-4 space-y-6">
      <div className="flex justify-between">
        <h3>
          {person.givenName} {person.middleName ? ` ${person.middleName}` : ""}
          {person.lastName}
        </h3>
        <div className="space-x-4">
          <RestrictedComponent accessLevel="PowerUserAndAbove">
            <div></div> {/** TODO: add edit button */}
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
      <div>
        <p>Date of birth: {person.dateOfBirth.toDateString()}</p>
      </div>
      <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-4">
        {person.movieRoles.map((mr) => (
          <Link key={mr.movieId} href={`/movies/${mr.movieId}`}>
            <div className="p-4 border border-slate-600 dark:border-slate-400 rounded-md text-center">
              <p>
                {mr.title}: {personRoleLabels[mr.role]}
              </p>
            </div>
          </Link>
        ))}
      </div>
    </div>
  );
}
