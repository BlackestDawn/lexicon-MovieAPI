"use client";

import { changePasswordRequest } from "@/lib/actions/auth";
import { inputClass, labelClass, panelClass } from "@/lib/data/consts/styles";
import Form from "next/form";
import { useRef, useState, useTransition } from "react";

export default function ChangePasswordForm() {
  const formRef = useRef<HTMLFormElement>(null);
  const [isPending, startTransition] = useTransition();
  const [error, setError] = useState<string>("");
  const [issues, setIssues] = useState<string[]>([]);
  const [success, setSuccess] = useState<boolean>(false);

  const handleSubmit = async (formData: FormData) => {
    setError("");
    setIssues([]);
    setSuccess(false);

    const newPassword = formData.get("newPassword") as string;
    const confirmPassword = formData.get("confirmPassword") as string;
    if (newPassword !== confirmPassword) {
      setIssues(["New password and confirmation do not match"]);
      return;
    }

    startTransition(async () => {
      const result = await changePasswordRequest(formData);
      if (!result.success) {
        setError(result.error);
        setIssues(result.issues ?? []);
        return;
      }
      formRef.current?.reset();
      setSuccess(true);
    });
  };

  return (
    <div className={`${panelClass} max-w-md space-y-4`}>
      <h3 className="text-lg font-semibold">Change password</h3>

      <Form ref={formRef} action={handleSubmit} className="space-y-4">
        <div>
          <label htmlFor="currentPassword" className={labelClass}>
            Current password
          </label>
          <input
            id="currentPassword"
            name="currentPassword"
            type="password"
            required
            className={inputClass}
            placeholder="Enter your current password"
            disabled={isPending}
          />
        </div>

        <div>
          <label htmlFor="newPassword" className={labelClass}>
            New password
          </label>
          <input
            id="newPassword"
            name="newPassword"
            type="password"
            required
            className={inputClass}
            placeholder="Enter a new password"
            disabled={isPending}
          />
        </div>

        <div>
          <label htmlFor="confirmPassword" className={labelClass}>
            Confirm new password
          </label>
          <input
            id="confirmPassword"
            name="confirmPassword"
            type="password"
            required
            className={inputClass}
            placeholder="Re-enter your new password"
            disabled={isPending}
          />
        </div>

        {success && (
          <div className="rounded-md bg-success/10 border border-success/30 p-4">
            <p className="text-sm text-success">Your password has been changed.</p>
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

        <div className="flex justify-end">
          <button
            type="submit"
            disabled={isPending}
            className="px-4 py-2 bg-primary text-primary-foreground rounded-md hover:bg-primary-hover transition-colors disabled:opacity-50"
          >
            {isPending ? "Saving..." : "Change password"}
          </button>
        </div>
      </Form>
    </div>
  );
}
