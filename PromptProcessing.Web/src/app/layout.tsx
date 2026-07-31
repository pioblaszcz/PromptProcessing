import type { Metadata } from "next";
import { JetBrains_Mono } from "next/font/google";
import "./globals.css";

const jetBrainsMono = JetBrains_Mono({
  subsets: ["latin"],
  variable: "--font-jetbrains-mono",
});

export const metadata: Metadata = {
  title: "Prompt Processing",
  description: "Queue prompts and track their processing status.",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html className={`${jetBrainsMono.className} ${jetBrainsMono.variable}`} lang="pl">
      <body>{children}</body>
    </html>
  );
}
