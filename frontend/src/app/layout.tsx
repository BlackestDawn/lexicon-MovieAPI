import type { Metadata } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import "./globals.css";
import { Sidebar } from "@/components/general/sidebar";
import { SiteHeader } from "@/components/general/siteHeader";
import { SiteFooter } from "@/components/general/siteFooter";
import CommonContext from "@/context/commonContext";
import { fetchCurrentUser } from "@/lib/actions/auth";

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
              <SiteHeader />
            </header>

            <main className="md:h-full">{children}</main>

            <footer className="bg-gray-200 dark:bg-gray-700">
              <SiteFooter />
            </footer>
          </div>
        </CommonContext>
      </body>
    </html>
  );
}
