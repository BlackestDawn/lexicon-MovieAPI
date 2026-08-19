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
        className="p-2 bg-accent text-accent-foreground rounded-md hover:bg-accent-hover transition-colors"
      >
        <FilePen className="w-5 h-5" />
      </button>

      {isOpen && (
        <GenreForm onClose={() => setIsOpen(false)} existingGenre={genre} />
      )}
    </>
  );
}
