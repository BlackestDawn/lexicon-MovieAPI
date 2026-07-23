import type { Metadata } from "next";
import Link from "next/link";
import { BaseContainer } from "@/components/base/baseContainer";

export const metadata: Metadata = {
  title: "Cookie Policy - MovieAPI",
  description: "Cookie Policy for the MovieAPI frontend application",
}

export default function CookiePolicy() {
  return (
    <BaseContainer className="py-12">
      <h1 className="text-4xl font-bold text-black dark:text-zinc-50 mb-8">
        Cookie Policy
      </h1>

      <div className="prose prose-gray dark:prose-invert max-w-none space-y-6 text-zinc-700 dark:text-zinc-300">
        <p className="text-lg">
          Last updated: 8 July 2026
        </p>

        <section>
          <h2 className="text-2xl font-semibold text-black dark:text-zinc-50 mt-8 mb-4">
            What Are Cookies
          </h2>
          <p>
            Cookies are small text files that are stored on your device when you visit our website.
            They help us provide you with a better experience by remembering your preferences and
            understanding how you use our application.
          </p>
        </section>

        <section>
          <h2 className="text-2xl font-semibold text-black dark:text-zinc-50 mt-8 mb-4">
            How We Use Cookies
          </h2>
          <p>
            MovieAPI uses cookies for the following purposes:
          </p>
          <ul className="list-disc pl-6 mt-4 space-y-2">
            <li>
              <strong>Authentication:</strong> To keep you logged in and maintain your session securely.
            </li>
            <li>
              <strong>Security:</strong> To protect against unauthorized access and ensure secure communication.
            </li>
            <li>
              <strong>Performance:</strong> To understand how you interact with our application and improve functionality.
            </li>
          </ul>
        </section>

        <section>
          <h2 className="text-2xl font-semibold text-black dark:text-zinc-50 mt-8 mb-4">
            Types of Cookies We Use
          </h2>

          <h3 className="text-xl font-semibold text-black dark:text-zinc-50 mt-6 mb-3">
            Essential Cookies
          </h3>
          <p>
            These cookies are necessary for the application to function properly. They enable core
            functionality such as security, authentication, and session management. The application
            cannot function properly without these cookies.
          </p>
        </section>

        <section>
          <h2 className="text-2xl font-semibold text-black dark:text-zinc-50 mt-8 mb-4">
            Managing Cookies
          </h2>
          <p>
            You can control and manage cookies in your browser settings. Please note that removing
            or blocking cookies may impact your user experience and some features of the application
            may not function properly.
          </p>
          <p className="mt-4">
            Most browsers allow you to:
          </p>
          <ul className="list-disc pl-6 mt-4 space-y-2">
            <li>View what cookies are stored and delete them individually</li>
            <li>Block third-party cookies</li>
            <li>Block cookies from specific sites</li>
            <li>Block all cookies from being set</li>
            <li>Delete all cookies when you close your browser</li>
          </ul>
        </section>

        <section>
          <h2 className="text-2xl font-semibold text-black dark:text-zinc-50 mt-8 mb-4">
            Changes to This Policy
          </h2>
          <p>
            We may update this Cookie Policy from time to time to reflect changes in our practices
            or for other operational, legal, or regulatory reasons. Please review this page
            periodically for any updates.
          </p>
        </section>

        <section>
          <h2 className="text-2xl font-semibold text-black dark:text-zinc-50 mt-8 mb-4">
            Contact Us
          </h2>
          <p>
            If you have any questions about this Cookie Policy, please open an issue on the{" "}
            <Link
              href="https://github.com/BlackestDawn/lexicon-MovieAPI"
              target="_blank"
              rel="noopener noreferrer"
              className="text-blue-600 dark:text-blue-400 hover:underline"
            >
              project repository
            </Link>
            .
          </p>
        </section>
      </div>
    </BaseContainer>
  );
}