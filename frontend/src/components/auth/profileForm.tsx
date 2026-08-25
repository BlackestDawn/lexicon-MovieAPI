"use client";

import { useAuth } from "@/context/commonContext";
import { ValidationError } from "@/lib/data/interfaces/errors";
import { inputClass, labelClass, panelClass } from "@/lib/data/consts/styles";
import Form from "next/form";
import { useState, useTransition } from "react";

export default function ProfileForm() {
  const { user, updateProfile } = useAuth();
  const [isPending, startTransition] = useTransition();
  const [error, setError] = useState<string>("");
  const [issues, setIssues] = useState<string[]>([]);
  const [success, setSuccess] = useState<boolean>(false);

  if (!user) return null;

  const handleSubmit = async (formData: FormData) => {
    setError("");
    setIssues([]);
    setSuccess(false);

    startTransition(async () => {
      try {
        const email = formData.get("email") as string;
        const displayName = (formData.get("displayName") as string) || undefined;
        await updateProfile(email, displayName);
        setSuccess(true);
      } catch (e) {
        if (e instanceof ValidationError) {
          setError(e.message);
          setIssues(e.issues);
        } else {
          setError(`Profile update failed: ${e instanceof Error ? e.message : String(e)}`);
        }
      }
    });
  };

  return (
    <div className={`${panelClass} max-w-md space-y-4`}>
      <h3 className="text-lg font-semibold">Profile</h3>

      <Form action={handleSubmit} className="space-y-4">
        <div>
          <label htmlFor="email" className={labelClass}>
            Email
          </label>
          <input
            id="email"
            name="email"
            type="text"
            required
            defaultValue={user.email}
            className={inputClass}
            placeholder="Enter your email"
            disabled={isPending}
          />
        </div>

        <div>
          <label htmlFor="displayName" className={labelClass}>
            Display name
          </label>
          <input
            id="displayName"
            name="displayName"
            type="text"
            defaultValue={user.name}
            className={inputClass}
            placeholder="Enter a display name"
            disabled={isPending}
          />
        </div>

        {success && (
          <div className="rounded-md bg-success/10 border border-success/30 p-4">
            <p className="text-sm text-success">Your profile has been updated.</p>
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
            {isPending ? "Saving..." : "Save changes"}
          </button>
        </div>
      </Form>
    </div>
  );
}
