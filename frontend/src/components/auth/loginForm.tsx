"use client";

import Form from "next/form";
import { useAuth } from "@/context/commonContext";
import { useRouter } from "next/navigation";
import { useEffect, useState, useTransition } from "react";
import { LogIn } from "lucide-react";
import { ValidationError } from "@/lib/data/interfaces/errors";
import {
  forgotPasswordRequest,
  resetPasswordRequest,
} from "@/lib/actions/auth";

const fieldInputClass =
  "appearence-once relative block w-full px-3 py-2 border border-border rounded-md bg-surface text-foreground placeholder-muted-foreground focus:outline-none focus:ring-2 focus:ring-primary focus:border-primary";
const fieldLabelClass = "block text-sm font-medium text-muted-foreground mb-1";

type Mode = "login" | "register" | "forgot";
type ForgotStep = "request" | "reset";

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
  const [forgotStep, setForgotStep] = useState<ForgotStep>("request");
  const [resetEmail, setResetEmail] = useState<string>("");

  useEffect(() => {
    if (user && fromPage && redirectTo) router.push(redirectTo);
  }, [user, fromPage, redirectTo, router]);

  const [isPending, startTransition] = useTransition();
  const [error, setError] = useState<string>("");
  const [issues, setIssues] = useState<string[]>([]);
  const [info, setInfo] = useState<string>("");

  const switchMode = (next: Mode) => {
    setMode(next);
    setForgotStep("request");
    setResetEmail("");
    setError("");
    setIssues([]);
    setInfo("");
  };

  const handleSubmit = async (formData: FormData) => {
    setError("");
    setIssues([]);
    setInfo("");

    startTransition(async () => {
      try {
        if (mode === "register") {
          const email = formData.get("email") as string;
          const password = formData.get("password") as string;
          const displayName = (formData.get("displayName") as string) || undefined;
          await register(email, password, displayName);
          if (onClose) onClose();
        } else if (mode === "login") {
          const email = formData.get("email") as string;
          const password = formData.get("password") as string;
          await login(email, password);
          if (onClose) onClose();
        } else if (forgotStep === "request") {
          const email = formData.get("email") as string;
          const result = await forgotPasswordRequest({ email });
          if (!result.success) {
            setError(result.error);
            setIssues(result.issues ?? []);
            return;
          }
          setResetEmail(email);
          setForgotStep("reset");
          setInfo(
            "If an account exists for that email, we've sent password reset instructions.",
          );
        } else {
          const token = formData.get("token") as string;
          const newPassword = formData.get("newPassword") as string;
          const result = await resetPasswordRequest({
            email: resetEmail,
            token,
            newPassword,
          });
          if (!result.success) {
            setError(result.error);
            setIssues(result.issues ?? []);
            return;
          }
          switchMode("login");
          setInfo("Password reset. You can now sign in with your new password.");
        }
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

  const title =
    mode === "login"
      ? "Sign in"
      : mode === "register"
        ? "Create account"
        : forgotStep === "request"
          ? "Reset your password"
          : "Choose a new password";

  const submitLabel = isPending
    ? mode === "login"
      ? "Signing in..."
      : mode === "register"
        ? "Creating account..."
        : forgotStep === "request"
          ? "Sending..."
          : "Resetting..."
    : mode === "login"
      ? "Sign in"
      : mode === "register"
        ? "Create account"
        : forgotStep === "request"
          ? "Send reset instructions"
          : "Reset password";

  return (
    <>
      <h3 id="dialog-title" className="w-full text-center text-2xl mb-4">
        {title}
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

          {mode === "forgot" && forgotStep === "reset" ? (
            <p className="text-sm text-muted-foreground">
              Resetting password for <strong>{resetEmail}</strong>.{" "}
              <button
                type="button"
                onClick={() => switchMode("forgot")}
                className="font-medium text-primary hover:text-primary-hover"
              >
                Use a different email
              </button>
            </p>
          ) : (
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
          )}

          {mode === "forgot" && forgotStep === "reset" && (
            <>
              <div>
                <label htmlFor="token" className={fieldLabelClass}>
                  Reset code
                </label>
                <input
                  id="token"
                  name="token"
                  type="text"
                  required
                  className={fieldInputClass}
                  placeholder="Enter the code from your email"
                  disabled={isPending}
                />
              </div>
              <div>
                <label htmlFor="newPassword" className={fieldLabelClass}>
                  New password
                </label>
                <input
                  id="newPassword"
                  name="newPassword"
                  type="password"
                  required
                  className={fieldInputClass}
                  placeholder="Enter a new password"
                  disabled={isPending}
                />
              </div>
            </>
          )}

          {(mode === "login" || mode === "register") && (
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
          )}

          {info && (
            <div className="rounded-md bg-success/10 border border-success/30 p-4">
              <p className="text-sm text-success">{info}</p>
            </div>
          )}

          {(error || issues.length > 0) && (
            <div className="rounded-md bg-danger/10 border border-danger/30 p-4 space-y-1">
              {error && <p className="text-sm text-danger">{error}</p>}
              {issues.map((issue, i) => (
                <p key={i} className="text-sm text-danger">
                  {issue}
                </p>
              ))}
            </div>
          )}

          <button
            type="submit"
            disabled={isPending}
            className="group relative w-full flex justify-center py-2 px-4 border border-transparent text-sm font-medium rounded-md text-primary-foreground bg-primary hover:bg-primary-hover focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            <span className="absolute left-0 inset-y-0 flex items-center pl-3">
              <LogIn className="h-5 w-5" />
            </span>
            {submitLabel}
          </button>
        </div>
      </Form>

      <div className="mt-4 space-y-1 text-center text-sm text-muted-foreground">
        {mode === "login" && (
          <>
            <p>
              Don&apos;t have an account?{" "}
              <button
                type="button"
                onClick={() => switchMode("register")}
                className="font-medium text-primary hover:text-primary-hover"
              >
                Register
              </button>
            </p>
            <p>
              Forgot your password?{" "}
              <button
                type="button"
                onClick={() => switchMode("forgot")}
                className="font-medium text-primary hover:text-primary-hover"
              >
                Reset it
              </button>
            </p>
          </>
        )}
        {mode === "register" && (
          <p>
            Already have an account?{" "}
            <button
              type="button"
              onClick={() => switchMode("login")}
              className="font-medium text-primary hover:text-primary-hover"
            >
              Sign in
            </button>
          </p>
        )}
        {mode === "forgot" && (
          <p>
            Remembered your password?{" "}
            <button
              type="button"
              onClick={() => switchMode("login")}
              className="font-medium text-primary hover:text-primary-hover"
            >
              Sign in
            </button>
          </p>
        )}
      </div>
    </>
  );
}
