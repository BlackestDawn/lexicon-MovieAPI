import { fetchPersons } from "@/lib/actions/people";
import PaginationControls from "../general/paginationControls";
import Link from "next/link";

export default async function PersonsList({ page }: { page?: number }) {
  const { persons, pagination } = await fetchPersons({ page });

  return (
    <div>
      <div className="w-full flex  justify-center my-6">
        {pagination && (
          <PaginationControls pagination={pagination} basePath="/persons" />
        )}
      </div>
      <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-4 mb-6">
        {persons.map((p) => (
          <Link key={p.id} href={`/persons/${p.id}`}>
            <div className="p-4 border border-slate-600 dark:border-slate-400 rounded-md text-center">
              <p>
                {p.givenName} {p.middleName && p.middleName[0] + ". "}
                {p.lastName}
              </p>
              <p>Born: {p.dateOfBirth.toDateString()}</p>
            </div>
          </Link>
        ))}
      </div>
    </div>
  );
}
