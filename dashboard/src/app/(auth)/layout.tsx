import type { Metadata } from "next";

export const metadata: Metadata = {
    title: "Login - ERA Monitor",
    description: "Sign in to ERA Monitor",
};

export default function AuthLayout({
    children,
}: {
    children: React.ReactNode;
}) {
    return children;
}
