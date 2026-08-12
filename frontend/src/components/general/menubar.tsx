"use client";

import { useAuth } from "@/context/commonContext";
import { useEffect, useState, useTransition } from "react";
import menuData from "@/lib/data/menuOptions.json";
import { MenuOption } from "@/lib/data/interfaces/general";
import { useRouter } from "next/navigation";
import Link from "next/link";
import HamburgerButton from "./hamburgerButton";

export default function Menubar() {
  const { user, logout } = useAuth();
  const [isPending, startTransition] = useTransition();
  const [isMenuOpen, setIsMenuOpen] = useState<boolean>(false);
  const router = useRouter();

  const toggleMenu = () => setIsMenuOpen(!isMenuOpen);
  const closeMenu = () => setIsMenuOpen(false);

  useEffect(() => {
    if (!isMenuOpen) return;

    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === "Escape") closeMenu();
    };

    document.addEventListener("keydown", handleKeyDown);
    return () => document.removeEventListener("keydown", handleKeyDown);
  }, [isMenuOpen]);

  const handleLogout = async () => {
    startTransition(async () => {
      await logout();

      router.push("/");
    });
  };

  const menuItems: MenuOption[] = [
    ...menuData.menu,
    user
      ? { label: "Logout", href: "#", action: handleLogout }
      : { label: "Login", href: "/login" },
  ];

  return (
    <>
      <HamburgerButton isOpen={isMenuOpen} onClick={toggleMenu} />

      <nav className="pb-4">
        {/* Desktop */}
        <div className="hidden md:block bg-white dark:bg-gray-800 rounded-lg shadow">
          <ul className="flex items-center justify-center">
            {menuItems.map((link, idx) => (
              <li
                key={`${idx}-${link.href}`}
                className={`flex-1 ${idx !== 0 ? "border-l border-gray-200 dark:border-gray-700" : ""}`}
              >
                {Boolean(link.action) ? (
                  <button
                    onClick={link.action}
                    disabled={isPending}
                    className="block w-full px-4 py-3 text-center font-medium text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors duration-200 disabled:opacity-50"
                  >
                    {isPending ? "Logging out..." : link.label}
                  </button>
                ) : (
                  <Link
                    href={link.href}
                    className="block w-full px-4 py-3 text-center font-medium text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors duration-200 disabled:opacity-50"
                  >
                    {link.label}
                  </Link>
                )}
              </li>
            ))}
          </ul>
        </div>
        <div className="hidden md:block bg-white dark:bg-gray-800 rounded-lg shadow"></div>

        {/* Mobile backdrop */}
        {isMenuOpen && (
          <div
            className="fixed inset-0 z-40 bg-black/50 md:hidden"
            onClick={closeMenu}
            aria-hidden="true"
          />
        )}

        {/* Mobile */}
        <div
          className={`absolute inset-x-0 top-full z-50 mt-4 md:hidden transition-all duration-300 ease-in-out ${isMenuOpen ? "opacity-100 translate-y-0" : "pointer-events-none -translate-y-2 opacity-0"}`}
        >
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow">
            <ul className="divide-y divide-gray-200 dark:divide-gray-700">
              {menuItems.map((link, idx) => (
                <li key={`${idx}-${link.href}`}>
                  {Boolean(link.action) ? (
                    <button
                      onClick={() => {
                        link.action!();
                        closeMenu();
                      }}
                      disabled={isPending}
                      className="block w-full px-4 py-3 text-center font-medium text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors duration-200 disabled:opacity-50"
                    >
                      {isPending ? "Logging out..." : link.label}
                    </button>
                  ) : (
                    <Link
                      href={link.href}
                      onClick={closeMenu}
                      className="block w-full px-4 py-3 text-center font-medium text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors duration-200 disabled:opacity-50"
                    >
                      {link.label}
                    </Link>
                  )}
                </li>
              ))}
            </ul>
          </div>
        </div>
      </nav>
    </>
  );
}
