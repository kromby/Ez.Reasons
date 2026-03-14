"use client";

import { useRouter } from "next/navigation";
import { useAuth } from "@/components/AuthProvider";
import LoginForm from "@/components/LoginForm";
import { useEffect } from "react";

export default function LoginPage() {
  const router = useRouter();
  const { isAuthenticated } = useAuth();

  useEffect(() => {
    if (isAuthenticated) {
      router.push("/dashboard");
    }
  }, [isAuthenticated, router]);

  if (isAuthenticated) {
    return null;
  }

  return (
    <div className="mx-auto max-w-sm px-4 py-10 sm:px-6 sm:py-16">
      <h1 className="mb-2 text-center text-3xl font-bold tracking-tight text-foreground">
        Innskráning
      </h1>
      <p className="mb-8 text-center text-muted">
        Skráðu þig inn til að fá aðgang að stjórnborðinu.
      </p>

      <div className="rounded-xl border border-card-border bg-card-bg p-6 sm:p-8">
        <LoginForm onSuccess={() => router.push("/dashboard")} />
      </div>
    </div>
  );
}
