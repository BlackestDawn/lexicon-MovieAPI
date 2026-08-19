"use client";

import menuData from "@/lib/data/consts/menuOptions.json";
import Link from "next/link";
import HamburgerButton from "./hamburgerButton";
import { useDismissableMenu } from "@/hooks/useDismissableMenu";

export default function MobileNav() {
  const { isOpen, toggle, close}= useDismissableMenu();

  return (
    <div className="md:hidden flex items-center gap-3">
      <HamburgerButton isOpen={isOpen} onClick={toggle} />
      <span className="text-lg font-semibold text-foreground">MovieAPI</span>

      {isOpen && (
        <div
          className="fixed inset-0 z-40 bg-black/50"
          onClick={close}
          aria-hidden="true"
        />
      )}

      <div
        className={`absolute inset-x-0 top-full z-50 mt-4 transition-all duration-300 ease-in-out ${isOpen ? "opacity-100 translate-y-0" : "pointer-events-none -translate-y-2 opacity-0"}`}
      >
        <div className="bg-surface border border-border rounded-lg shadow">
          <ul className="divide-y divide-border">
            {menuData.main.map((link) => (
              <li key={link.href}>
                <Link
                  href={link.href}
                  onClick={close}
                  className="block w-full whitespace-nowrap px-4 py-3 text-center font-medium text-foreground hover:bg-background transition-colors duration-200"
                >
                  {link.label}
                </Link>
              </li>
            ))}
          </ul>
        </div>
      </div>
    </div>
  );
}
