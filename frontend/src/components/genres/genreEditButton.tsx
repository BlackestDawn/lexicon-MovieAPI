"use client";

import { GenreExtendedDto } from "@/lib/data/models/genreTypes";
import { FilePen } from "lucide-react";
import { useState } from "react";
import GenreForm from "./genreForm";

export default function GenreEditButton({
  genre,
}: {
  genre: GenreExtendedDto;
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
        <GenreForm onClose={() => setIsOpen(false)} existingGenre={genre} />
      )}
    </>
  );
}
