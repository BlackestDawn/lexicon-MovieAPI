import Link from "next/link";
import menuData from "@/lib/data/consts/menuOptions.json";

export function DesktopNav() {
  return (
    <nav>
      <ul className="space-y-1">
        {menuData.main.map((link) => (
          <li key={link.href}>
            <Link
              href={link.href}
              className="block whitespace-nowrap rounded-md px-3 py-2 font-medium text-sidebar-foreground/80 hover:bg-sidebar-hover hover:text-sidebar-foreground transition-colors duration-200"
            >
              {link.label}
            </Link>
          </li>
        ))}
      </ul>
    </nav>
  );
}
