"use client";

import Link from "next/link";
import { useTransition } from "react";
import { useAuth } from "@/context/commonContext";
import { useDismissableMenu } from "@/hooks/useDismissableMenu";
import menuData from "@/lib/data/consts/menuOptions.json";
import LoginForm from "../auth/loginForm";
import DialogBase from "./dialogBase";

export function AccountMenu() {
  const { isOpen, toggle, close, menuRef } =
    useDismissableMenu<HTMLDivElement>();
  const { user, logout } = useAuth();
  const [isPending, startTransition] = useTransition();

  const handleLogout = () => {
    startTransition(async () => {
      await logout();
      close();
    });
  };

  if (!user) {
    return (
      <div ref={menuRef}>
        <button onClick={toggle} className="font-medium hover:underline">
          Login
        </button>

        {isOpen && (
          <DialogBase onClose={close}>
            <LoginForm onClose={close} />
          </DialogBase>
        )}
      </div>
    );
  }

  return (
    <div ref={menuRef} className="relative inline-block">
      <button
        onClick={toggle}
        className="inline-flex items-center justify-center p-2 rounded-lg text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700 focus:outline-none focus:ring-2 focus:ring-inset focus:ring-indigo-500 transition-colors duration-200"
      >
        {user.name}
      </button>

      <div
        className={`absolute right-0 origin-top-right z-50 mt-4 transition-all duration-300 ease-in-out ${isOpen ? "opacity-100 translate-y-0" : "pointer-events-none -translate-y-2 opacity-0"}`}
      >
        <div className="bg-white dark:bg-gray-800 rounded-lg shadow">
          <ul className="divide-y divide-gray-200 dark:divide-gray-700">
            {menuData.user.map((link) => (
              <li key={link.href}>
                <Link
                  href={link.href}
                  onClick={close}
                  className="block w-full whitespace-nowrap px-4 py-3 text-center font-medium text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors duration-200"
                >
                  {link.label}
                </Link>
              </li>
            ))}
            <li>
              <button
                onClick={handleLogout}
                disabled={isPending}
                className="block w-full whitespace-nowrap px-4 py-3 text-center font-medium text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors duration-200 disabled:opacity-50"
              >
                {isPending ? "Signing out..." : "Sign out"}
              </button>
            </li>
          </ul>
        </div>
      </div>
    </div>
  );
}
