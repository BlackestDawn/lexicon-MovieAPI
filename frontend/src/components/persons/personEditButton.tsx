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
        className="p-2 bg-accent text-accent-foreground rounded-md hover:bg-accent-hover transition-colors"
      >
        <FilePen className="w-5 h-5" />
      </button>

      {isOpen && (
        <PersonFullForm onClose={() => setIsOpen(false)} existingPerson={person} />
      )}
    </>
  );
}
