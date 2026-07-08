import type { Metadata } from "next";
import Link from "next/link";
import { BaseContainer } from "@/components/base/baseContainer";

export const metadata: Metadata = {
  title: "Licensing - MovieAPI",
  description: "Licensing information for MovieAPI",
}

export default function Licensing() {
  return (
    <BaseContainer className="py-12">
      <h1 className="text-4xl font-bold text-black dark:text-zinc-50 mb-8">Licensing</h1>

      <div className="prose prose-gray dark:prose-invert max-w-none space-y-6 text-zinc-700 dark:text-zinc-300">
        <section>
          <h2 className="text-2xl font-semibold text-black dark:text-zinc-50 mt-8 mb-4">
            Creative Commons Attribution 4.0 International License
          </h2>
          <p>
            MovieAPI is licensed under the Creative Commons Attribution 4.0 International License (CC BY 4.0). This means you are free to:
          </p>
          <ul className="list-disc pl-6 mt-4 space-y-2">
            <li>
              <strong>Share:</strong> Copy and redistribute the material in any medium or format
            </li>
            <li>
              <strong>Adapt:</strong> Remix, transform, and build upon the material for any purpose, even commercially
            </li>
          </ul>
        </section>

        <section>
          <h2 className="text-2xl font-semibold text-black dark:text-zinc-50 mt-8 mb-4">
            Terms
          </h2>
          <p>
            Under the following terms:
          </p>
          <ul className="list-disc pl-6 mt-4 space-y-2">
            <li>
              <strong>Attribution:</strong> You must give appropriate credit, provide a link to
              the license, and indicate if changes were made. You may do so in any reasonable
              manner, but not in any way that suggests the licensor endorses you or your use.
            </li>
            <li>
              <strong>No additional restrictions:</strong> You may not apply legal terms or
              technological measures that legally restrict others from doing anything the license
              permits.
            </li>
          </ul>
        </section>

        <section>
          <h2 className="text-2xl font-semibold text-black dark:text-zinc-50 mt-8 mb-4">
            Notices
          </h2>
          <p>
            You do not have to comply with the license for elements of the material in the public
            domain or where your use is permitted by an applicable exception or limitation.
          </p>
          <p className="mt-4">
            No warranties are given. The license may not give you all of the permissions necessary
            for your intended use. For example, other rights such as publicity, privacy, or moral
            rights may limit how you use the material.
          </p>
        </section>

        <section>
          <h2 className="text-2xl font-semibold text-black dark:text-zinc-50 mt-8 mb-4">
            Full License Text
          </h2>
          <p>
            To view the complete license terms, please visit:{' '}
            <Link
              href="https://creativecommons.org/licenses/by/4.0/legalcode"
              target="_blank"
              rel="noopener noreferrer"
              className="text-blue-600 dark:text-blue-400 hover:underline"
            >
              Creative Commons Attribution 4.0 International License
            </Link>
          </p>
        </section>

        <section>
          <h2 className="text-2xl font-semibold text-black dark:text-zinc-50 mt-8 mb-4">
            Third-Party Libraries
          </h2>
          <p>
            This application uses various open-source libraries and frameworks, each with their
            own licenses. These include:
          </p>
          <ul className="list-disc pl-6 mt-4 space-y-2">
            <li>Next.js (MIT License)</li>
            <li>React (MIT License)</li>
            <li>Tailwind CSS (MIT License)</li>
          </ul>
          <p className="mt-4">
            We are grateful to the open-source community for these excellent tools.
          </p>
        </section>

        <section>
          <h2 className="text-2xl font-semibold text-black dark:text-zinc-50 mt-8 mb-4">
            Attribution
          </h2>
          <p>
            If you use or adapt this project, please provide attribution in the following format:
          </p>
          <div className="bg-zinc-100 dark:bg-zinc-800 p-4 rounded-lg mt-4 font-mono text-sm">
            MovieAPI by Alexander Stauch is licensed under CC BY 4.0
          </div>
        </section>
      </div>
    </BaseContainer>
  );
}
