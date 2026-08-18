"use client";

import Form from "next/form";
import { useAuth } from "@/context/commonContext";
import { useRouter } from "next/navigation";
import { useEffect, useState, useTransition } from "react";
import { LogIn } from "lucide-react";
import { ValidationError } from "@/lib/data/interfaces/errors";

const fieldInputClass =
  "appearence-once relative block w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-700 text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500";
const fieldLabelClass =
  "block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1";

type Mode = "login" | "register";

export default function LoginForm({
  redirectTo = "/",
  fromPage = false,
  onClose,
}: {
  redirectTo?: string;
  fromPage?: boolean;
  onClose?: () => void;
}) {
  const router = useRouter();
  const { user, login, register } = useAuth();
  const [mode, setMode] = useState<Mode>("login");

  useEffect(() => {
    if (user && fromPage && redirectTo) router.push(redirectTo);
  }, [user, fromPage, redirectTo, router]);

  const [isPending, startTransition] = useTransition();
  const [error, setError] = useState<string>("");
  const [issues, setIssues] = useState<string[]>([]);

  const toggleMode = () => {
    setMode((m) => (m === "login" ? "register" : "login"));
    setError("");
    setIssues([]);
  };

  const handleSubmit = async (formData: FormData) => {
    setError("");
    setIssues([]);

    startTransition(async () => {
      const email = formData.get("email") as string;
      const password = formData.get("password") as string;

      try {
        if (mode === "register") {
          const displayName = (formData.get("displayName") as string) || undefined;
          await register(email, password, displayName);
        } else {
          await login(email, password);
        }
        if (onClose) onClose();
      } catch (e) {
        if (e instanceof ValidationError) {
          setError(e.message);
          setIssues(e.issues);
        } else {
          const action = mode === "register" ? "Registration" : "Login";
          setError(`${action} failed: ${e instanceof Error ? e.message : String(e)}`);
        }
      }
    });
  };

  return (
    <>
      <h3 id="dialog-title" className="w-full text-center text-2xl mb-4">
        {mode === "login" ? "Sign in" : "Create account"}
      </h3>

      <Form action={handleSubmit} className="space-y-6">
        <div className="rounded-md shadow-sm space-y-4">
          {mode === "register" && (
            <div>
              <label htmlFor="displayName" className={fieldLabelClass}>
                Display name
              </label>
              <input
                id="displayName"
                name="displayName"
                type="text"
                className={fieldInputClass}
                placeholder="Enter a display name (optional)"
                disabled={isPending}
              />
            </div>
          )}
          <div>
            <label htmlFor="email" className={fieldLabelClass}>
              Email
            </label>
            <input
              id="email"
              name="email"
              type="text"
              required
              className={fieldInputClass}
              placeholder="Enter your email"
              disabled={isPending}
            />
          </div>
          <div>
            <label htmlFor="password" className={fieldLabelClass}>
              Password
            </label>
            <input
              id="password"
              name="password"
              type="password"
              required
              className={fieldInputClass}
              placeholder="Enter your password"
              disabled={isPending}
            />
          </div>

          {(error || issues.length > 0) && (
            <div className="rounded-md bg-red-50 dark:bg-red-900 p-4 space-y-1">
              {error && (
                <p className="text-sm text-red-800 dark:text-red-200">{error}</p>
              )}
              {issues.map((issue, i) => (
                <p key={i} className="text-sm text-red-800 dark:text-red-200">
                  {issue}
                </p>
              ))}
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
            {isPending
              ? mode === "login"
                ? "Signing in..."
                : "Creating account..."
              : mode === "login"
                ? "Sign in"
                : "Create account"}
          </button>
        </div>
      </Form>

      <p className="mt-4 text-center text-sm text-gray-600 dark:text-gray-400">
        {mode === "login" ? "Don't have an account?" : "Already have an account?"}{" "}
        <button
          type="button"
          onClick={toggleMode}
          className="font-medium text-indigo-600 hover:text-indigo-500 dark:text-indigo-400"
        >
          {mode === "login" ? "Register" : "Sign in"}
        </button>
      </p>
    </>
  );
}
