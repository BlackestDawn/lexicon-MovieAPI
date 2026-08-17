"use client";

import { useState } from "react";
import GenreForm from "./genreForm";

export default function GenreCreateButton() {
  const [isOpen, setIsOpen] = useState<boolean>(false);

  return (
    <>
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="p-2 bg-green-400 dark:bg-green-700 text-slate-700 dark:text-slate-200 rounded-md"
      >
        Create new genre
      </button>

      {isOpen && <GenreForm onClose={() => setIsOpen(false)} />}
    </>
  );
}
