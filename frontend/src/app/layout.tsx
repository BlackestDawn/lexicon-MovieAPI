import type { Metadata } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import "./globals.css";
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
      <body className="min-h-full flex flex-col">
        <CommonContext initialUser={user}>
          <header className="bg-gray-700">
            <SiteHeader />
          </header>
          <main>{children}</main>
          <footer className="bg-gray-700">
            <SiteFooter />
          </footer>
        </CommonContext>
      </body>
    </html>
  );
}
