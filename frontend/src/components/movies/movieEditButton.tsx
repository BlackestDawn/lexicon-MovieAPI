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
        className="p-2 bg-blue-400 dark:bg-blue-700 text-slate-700 dark:text-slate-200 rounded-md"
      >
        <FilePen className="w-5 h-5" />
      </button>

      {isOpen && <MovieFormFull onClose={() => setIsOpen(false)} existingMovie={movie} />}
    </>
  );
}
