import Link from "next/link";
import { ChevronLeft, ChevronRight } from "lucide-react";
import { PaginationMetadata } from "@/lib/data/models/paginationTypes";

export default function PaginationControls({
  pagination,
  basePath,
  queryParams,
}: {
  pagination: PaginationMetadata;
  basePath: string;
  queryParams?: Record<string, string | number | undefined>;
}) {
  const { CurrentPage, TotalPageCount } = pagination;

  if (TotalPageCount <= 1) return null;

  const hasPrev = CurrentPage > 1;
  const hasNext = CurrentPage < TotalPageCount;

  const hrefForPage = (page: number) => {
    const params = new URLSearchParams();
    for (const [key, value] of Object.entries(queryParams ?? {})) {
      if (value !== undefined && value !== "") params.set(key, String(value));
    }
    params.set("page", String(page));
    return `${basePath}?${params.toString()}`;
  };

  return (
    <div className="flex items-center justify-center gap-4">
      <Link
        href={hrefForPage(CurrentPage - 1)}
        aria-disabled={!hasPrev}
        tabIndex={hasPrev ? undefined : -1}
        className={`p-2 rounded-md border border-slate-600 dark:border-slate-300 ${
          hasPrev ? "" : "pointer-events-none opacity-40"
        }`}
      >
        <ChevronLeft className="h-5 w-5" />
      </Link>
      <span className="text-sm text-slate-500 dark:text-slate-300">
        Page {CurrentPage} of {TotalPageCount}
      </span>
      <Link
        href={hrefForPage(CurrentPage + 1)}
        aria-disabled={!hasNext}
        tabIndex={hasNext ? undefined : -1}
        className={`p-2 rounded-md border border-slate-600 dark:border-slate-300 ${
          hasNext ? "" : "pointer-events-none opacity-40"
        }`}
      >
        <ChevronRight className="h-5 w-5" />
      </Link>
    </div>
  );
}
