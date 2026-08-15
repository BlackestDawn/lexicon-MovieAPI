"use client";

import { useState } from "react";
import MovieFormFull from "./movieFormFull";

export default function MovieCreateButton() {
  const [isOpen, setIsOpen] = useState<boolean>(false);

  return (
    <>
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="p-2 bg-green-400 dark:bg-green-700 text-slate-700 dark:text-slate-200 rounded-md"
      >
        Create new movie
      </button>

      {isOpen && <MovieFormFull onClose={() => setIsOpen(false)} />}
    </>
  );
}
