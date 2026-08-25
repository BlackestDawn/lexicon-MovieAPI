"use client";

import { useEffect } from "react";
import Link from "next/link";
import "./globals.css";

export default function GlobalError({
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
    <html lang="en">
      <body className="min-h-full flex items-center justify-center bg-background antialiased">
        <div className="bg-surface border border-border rounded-xl shadow-sm p-8 max-w-md mx-4 text-center space-y-4">
          <h1 className="text-2xl font-semibold text-foreground">
            Something went wrong
          </h1>
          <p className="text-muted-foreground">
            The application ran into an unexpected error. You can try again,
            or head back to the homepage.
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
      </body>
    </html>
  );
}
