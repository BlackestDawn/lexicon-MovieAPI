"use client";

import { PersonExtendedDto } from "@/lib/data/models/personTypes";
import { FilePen } from "lucide-react";
import { useState } from "react";
import PersonFullForm from "./personFormFull";

export default function PersonEditButton({
  person,
}: {
  person: PersonExtendedDto;
}) {
  const [isOpen, setIsOpen] = useState<boolean>(false);

  return (
    <>
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="p-2 bg-blue-400 dark:bg-blue-700 text-slate-700 dark:text-slate-200 rounded-md"
      >
        <FilePen className="w-5 h-5" />
      </button>

      {isOpen && (
        <PersonFullForm onClose={() => setIsOpen(false)} existingPerson={person} />
      )}
    </>
  );
}
