import type { Metadata } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import "./globals.css";
import { Sidebar } from "@/components/general/sidebar";
import { SiteHeader } from "@/components/general/siteHeader";
import { SiteFooter } from "@/components/general/siteFooter";
import CommonContext from "@/context/commonContext";
import { fetchCurrentUser } from "@/lib/actions/auth";
import { BaseContainer } from "@/components/general/baseContainer";

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: "MovieAPI",
  description: "Frontend for the MovieAPI movie catalog service.",
};

export default async function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  const user = await fetchCurrentUser();

  return (
    <html
      lang="en"
      className={`${geistSans.variable} ${geistMono.variable} h-full antialiased`}
    >
      <body className="min-h-full flex flex-col md:flex-row">
        <CommonContext initialUser={user}>
          <Sidebar />

          <div className="flex flex-1 flex-col min-w-0">
            <header className="bg-gray-200 dark:bg-gray-700 text-white">
              <BaseContainer className="py-4">
                <SiteHeader />
              </BaseContainer>
            </header>

            <main className="md:h-full">
              <BaseContainer>{children}</BaseContainer>
            </main>

            <footer className="bg-gray-200 dark:bg-gray-700">
              <BaseContainer className="py-8 text-center text-sm text-zinc-600 dark:text-zinc-400">
                <SiteFooter />
              </BaseContainer>
            </footer>
          </div>
        </CommonContext>
      </body>
    </html>
  );
}
