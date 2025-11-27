import type { Metadata } from "next";
import { Inter } from "next/font/google";
import "./globals.css";
import { Providers } from "./providers";

const inter = Inter({
  subsets: ["latin"],
  variable: "--font-inter",
});

export const metadata: Metadata = {
  title: "ERA Monitor",
  description: "Enterprise Resource Availability Monitoring",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en" className="dark">
      <head>
        <link
          href="https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined:wght@100..700"
          rel="stylesheet"
        />
      </head>
      <body
        className={`${inter.variable} antialiased bg-[#111c21] font-sans`}
      >
        <Providers>
          {children}
        </Providers>
      </body>
    </html>
  );
}
