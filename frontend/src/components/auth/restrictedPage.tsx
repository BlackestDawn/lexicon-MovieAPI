"use client";

import { useAuth } from "@/context/commonContext";
import { AccessLevel } from "@/lib/data/interfaces/auth";
import { LockKeyhole, ShieldAlert } from "lucide-react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { ReactNode } from "react";

function RestrictedNotice({
  icon,
  title,
  message,
  action,
}: {
  icon: ReactNode;
  title: string;
  message: string;
  action?: ReactNode;
}) {
  return (
    <div className="flex justify-center px-4 py-16">
      <div className="w-full max-w-md rounded-md border border-border bg-surface p-8 text-center shadow-sm">
        <div className="mx-auto mb-4 flex h-12 w-12 items-center justify-center rounded-full bg-danger/10 text-danger">
          {icon}
        </div>
        <h2 className="text-lg font-semibold text-foreground">{title}</h2>
        <p className="mt-2 text-sm text-muted-foreground">{message}</p>
        {action && <div className="mt-6">{action}</div>}
      </div>
    </div>
  );
}

export default function RestrictedPage({
  children,
  accessLevel = "LoggedIn",
}: {
  children: ReactNode;
  accessLevel?: AccessLevel;
}) {
  const { user, hasAccess } = useAuth();
  const pathname = usePathname();

  if (!user) {
    return (
      <RestrictedNotice
        icon={<LockKeyhole className="h-6 w-6" />}
        title="Sign in required"
        message="You must be logged in to access this page."
        action={
          <Link
            href={`/login?redirectTo=${encodeURIComponent(pathname)}`}
            className="inline-flex justify-center rounded-md border border-transparent bg-primary px-4 py-2 text-sm font-medium text-primary-foreground transition-colors hover:bg-primary-hover focus:outline-none focus:ring-2 focus:ring-primary focus:ring-offset-2"
          >
            Sign in
          </Link>
        }
      />
    );
  }

  if (!hasAccess(accessLevel)) {
    return (
      <RestrictedNotice
        icon={<ShieldAlert className="h-6 w-6" />}
        title="Access denied"
        message="You do not have high enough access rights for this page."
        action={
          <Link
            href="/"
            className="inline-flex justify-center rounded-md border border-border bg-surface px-4 py-2 text-sm font-medium text-foreground transition-colors hover:bg-background focus:outline-none focus:ring-2 focus:ring-primary focus:ring-offset-2"
          >
            Go back home
          </Link>
        }
      />
    );
  }

  return <>{children}</>;
}
