import { X } from "lucide-react";
import { ReactNode } from "react";

export default function DialogBase({
  children,
  onClose,
}: {
  children: ReactNode;
  onClose: () => void;
}) {
  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center p-4"
      onClick={onClose}
    >
      <div
        onClick={(e) => e.stopPropagation()}
        role="dialog"
        aria-modal="true"
        aria-labelledby="dialog-title"
        className="relative w-full max-w-2xl max-h-[90vh] overflow-y-auto p-6 border border-slate-600 dark:border-slate-400 rounded-2xl bg-white dark:bg-gray-800 shadow-xl"
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
