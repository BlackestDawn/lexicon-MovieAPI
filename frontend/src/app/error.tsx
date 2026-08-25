"use client";

import { useEffect } from "react";
import Link from "next/link";
import { TriangleAlert } from "lucide-react";
import { BaseContainer } from "@/components/general/baseContainer";
import { panelClass, sectionHeadingClass } from "@/lib/data/consts/styles";

export default function Error({
  error,
  retry,
}: {
  error: Error & { digest?: string };
  retry: () => void;
}) {
  useEffect(() => {
    console.error(error);
  }, [error]);

  return (
    <BaseContainer className="py-12">
      <div className={`${panelClass} max-w-md mx-auto text-center space-y-4`}>
        <TriangleAlert className="mx-auto h-10 w-10 text-danger" />
        <h1 className={sectionHeadingClass}>Something went wrong</h1>
        <p className="text-muted-foreground">
          An unexpected error occurred while loading this page. You can try
          again, or head back to the homepage.
        </p>
        {error.digest && (
          <p className="text-xs text-muted-foreground">
            Reference: {error.digest}
          </p>
        )}
        <div className="flex justify-center gap-3 pt-2">
          <button
            onClick={() => retry()}
            className="px-4 py-2 bg-primary text-primary-foreground rounded-md hover:bg-primary-hover transition-colors"
          >
            Try again
          </button>
          <Link
            href="/"
            className="px-4 py-2 border border-border rounded-md text-foreground hover:bg-surface transition-colors"
          >
            Go home
          </Link>
        </div>
      </div>
    </BaseContainer>
  );
}
