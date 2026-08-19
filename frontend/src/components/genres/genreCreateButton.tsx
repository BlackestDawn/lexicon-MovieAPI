"use client";

import { useState } from "react";
import GenreForm from "./genreForm";

export default function GenreCreateButton() {
  const [isOpen, setIsOpen] = useState<boolean>(false);

  return (
    <>
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="p-2 bg-primary text-primary-foreground rounded-md hover:bg-primary-hover transition-colors"
      >
        Create new genre
      </button>

      {isOpen && <GenreForm onClose={() => setIsOpen(false)} />}
    </>
  );
}
