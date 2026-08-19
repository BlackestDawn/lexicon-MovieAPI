"use client";

import { useState } from "react";
import MovieFormFull from "./movieFormFull";

export default function MovieCreateButton() {
  const [isOpen, setIsOpen] = useState<boolean>(false);

  return (
    <>
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="p-2 bg-primary text-primary-foreground rounded-md hover:bg-primary-hover transition-colors"
      >
        Create new movie
      </button>

      {isOpen && <MovieFormFull onClose={() => setIsOpen(false)} />}
    </>
  );
}
