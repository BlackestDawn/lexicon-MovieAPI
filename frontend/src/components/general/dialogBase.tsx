"use client";

import { X } from "lucide-react";
import { ReactNode, useEffect } from "react";

const sizeClass = {
  md: "max-w-md",
  "2xl": "max-w-2xl",
} as const;

export default function DialogBase({
  children,
  onClose,
  size = "2xl",
}: {
  children: ReactNode;
  onClose: () => void;
  size?: keyof typeof sizeClass;
}) {
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape") onClose();
    };
    document.addEventListener("keydown", handleKeyDown);
    return () => document.removeEventListener("keydown", handleKeyDown);
  }, [onClose]);

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4"
      onClick={onClose}
    >
      <div
        onClick={(e) => e.stopPropagation()}
        role="dialog"
        aria-modal="true"
        aria-labelledby="dialog-title"
        className={`relative w-full ${sizeClass[size]} max-h-[90vh] overflow-y-auto p-6 border border-slate-600 dark:border-slate-400 rounded-2xl bg-white dark:bg-gray-800 shadow-xl`}
      >
        <button
          type="button"
          onClick={onClose}
          aria-label="Close"
          className="absolute top-4 right-4 text-slate-500 hover:text-slate-800 dark:hover:text-slate-200"
        >
          <X className="w-5 h-5" />
        </button>
        {children}
      </div>
    </div>
  );
}
