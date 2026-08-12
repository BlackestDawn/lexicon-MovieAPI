"use client";

import Form from "next/form";
import { useAuth } from "@/context/commonContext";
import { useRouter } from "next/navigation";
import { useEffect, useState, useTransition } from "react";
import { LogIn } from "lucide-react";

export default function LoginForm({
  redirectTo = "/",
}: {
  redirectTo?: string;
}) {
  const router = useRouter();
  const { user, login } = useAuth();

  useEffect(() => {
    if (user) router.push(redirectTo);
  }, [user, redirectTo, router]);

  const [isPending, startTransition] = useTransition();
  const [error, setError] = useState<string>("");

  const handleSubmit = async (formData: FormData) => {
    setError("");

    startTransition(async () => {
      const email = formData.get("email") as string;
      const password = formData.get("password") as string;

      try {
        await login(email, password);
      } catch (e) {
        setError(`Login failed: ${e instanceof Error ? e.message : String(e)}`);
      }
    });
  };

  return (
    <Form action={handleSubmit} className="mt-8 space-y-6">
      <div className="rounded-md shadow-sm space-y-4">
        <div>
          <label
            htmlFor="email"
            className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1"
          >
            Email
          </label>
          <input
            id="email"
            name="email"
            type="text"
            required
            className="appearence-once relative block w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-700 text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500"
            placeholder="Enter your email"
            disabled={isPending}
          />
        </div>
        <div>
          <label
            htmlFor="password"
            className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1"
          >
            Password
          </label>
          <input
            id="password"
            name="password"
            type="password"
            required
            className="appearence-once relative block w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-700 text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500"
            placeholder="Enter your password"
            disabled={isPending}
          />
        </div>

        {error && (
          <div className="rounded-md bg-red-50 dark:bg-red-900 p-4">
            <p className="text-sm text-red-800 dark:text-red-200">{error}</p>
          </div>
        )}

        <button
          type="submit"
          disabled={isPending}
          className="group relative w-full flex justify-center py-2 px-4 border border-transparent text-sm font-medium rounded-md text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
        >
          <span className="absolute left-0 inset-y-0 flex items-center pl-3">
            <LogIn className="h-5 w-5" />
          </span>
          {isPending ? "Signing in..." : "Sign in"}
        </button>
      </div>
    </Form>
  );
}
