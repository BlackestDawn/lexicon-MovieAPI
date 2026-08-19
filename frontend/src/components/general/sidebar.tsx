import Link from "next/link";
import { DesktopNav } from "./desktopNav";

export function Sidebar() {
  return (
    <aside className="hidden md:flex md:w-44 md:shrink-0 md:flex-col bg-sidebar text-sidebar-foreground">
      <div className="px-6 py-6">
        <Link href="/" className="text-2xl font-semibold tracking-tight hover:text-accent transition-colors">
          MovieAPI
        </Link>
      </div>
      <div className="flex-1 px-3">
        <DesktopNav />
      </div>
    </aside>
  );
}
