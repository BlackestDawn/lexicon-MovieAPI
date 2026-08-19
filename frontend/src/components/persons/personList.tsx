import { fetchPersons } from "@/lib/actions/person";
import PaginationControls from "../general/paginationControls";
import Link from "next/link";
import { UserRound, Calendar, Users } from "lucide-react";
import RestrictedComponent from "../auth/restrictedComponent";
import PersonCreateButton from "./personCreateButton";
import PersonFilters from "./personFilters";
import { cardClass, metaClass, sectionHeadingClass } from "@/lib/data/consts/styles";

export default async function PersonsList({
  page,
  name,
  genre,
  year,
}: {
  page?: number;
  name?: string;
  genre?: string;
  year?: number;
}) {
  const { persons, pagination } = await fetchPersons({
    page,
    name,
    genre,
    year,
  });

  return (
    <div className="my-8 space-y-6">
      <div className="text-center space-y-4">
        <h3 className={sectionHeadingClass}>Browse persons</h3>
        <RestrictedComponent accessLevel="PowerUserAndAbove">
          <PersonCreateButton />
        </RestrictedComponent>
      </div>
      <PersonFilters name={name} genre={genre} year={year} />
      <div className="w-full flex justify-center">
        {pagination && (
          <PaginationControls
            pagination={pagination}
            basePath="/persons"
            queryParams={{ name, genre, year }}
          />
        )}
      </div>
      {persons.length === 0 && (
        <div className="flex flex-col items-center gap-2 py-12 text-muted-foreground">
          <Users className="w-8 h-8" />
          <p>No persons found matching your filters.</p>
        </div>
      )}
      <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-4">
        {persons.map((p) => (
          <Link key={p.id} href={`/persons/${p.id}`}>
            <div className={`${cardClass} flex flex-col items-center gap-2 p-5 text-center`}>
              <UserRound className="w-6 h-6 text-primary" />
              <p className="font-medium">
                {p.givenName} {p.middleName && p.middleName[0] + ". "}
                {p.lastName}
              </p>
              <span className={metaClass}>
                <Calendar className="w-4 h-4" />
                {p.dateOfBirth.toDateString()}
              </span>
            </div>
          </Link>
        ))}
      </div>
    </div>
  );
}
