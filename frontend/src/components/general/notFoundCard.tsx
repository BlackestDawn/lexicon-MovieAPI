import Link from "next/link";
import { SearchX } from "lucide-react";
import { panelClass, sectionHeadingClass } from "@/lib/data/consts/styles";

export function NotFoundCard({
  heading,
  message,
  linkHref,
  linkLabel,
}: {
  heading: string;
  message: string;
  linkHref: string;
  linkLabel: string;
}) {
  return (
    <div className={`${panelClass} max-w-md mx-auto text-center space-y-4`}>
      <SearchX className="mx-auto h-10 w-10 text-muted-foreground" />
      <h1 className={sectionHeadingClass}>{heading}</h1>
      <p className="text-muted-foreground">{message}</p>
      <div className="flex justify-center pt-2">
        <Link
          href={linkHref}
          className="px-4 py-2 bg-primary text-primary-foreground rounded-md hover:bg-primary-hover transition-colors"
        >
          {linkLabel}
        </Link>
      </div>
    </div>
  );
}
