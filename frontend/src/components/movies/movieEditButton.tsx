"use client";

import { MovieExtendedDto } from "@/lib/data/models/movieTypes";
import { FilePen } from "lucide-react";
import { useState } from "react";
import MovieFormFull from "./movieFormFull";

export default function MovieEditButton({
  movie,
}: {
  movie: MovieExtendedDto;
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

      {isOpen && <MovieFormFull onClose={() => setIsOpen(false)} existingMovie={movie} />}
    </>
  );
}
