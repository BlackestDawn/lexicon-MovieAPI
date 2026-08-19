"use client";

import { useState } from "react";
import PersonFullForm from "./personFormFull";

export default function PersonCreateButton() {
  const [isOpen, setIsOpen] = useState<boolean>(false);

  return (
    <>
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="p-2 bg-primary text-primary-foreground rounded-md hover:bg-primary-hover transition-colors"
      >
        Create new person
      </button>

      {isOpen && <PersonFullForm onClose={() => setIsOpen(false)} />}
    </>
  );
}
