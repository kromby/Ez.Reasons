"use client";

import { useState, useEffect, useCallback } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/components/AuthProvider";

interface PendingLetter {
  id: string;
  title: string;
  body: string;
  email: string;
  submittedAt: string;
}

export default function DashboardPage() {
  const router = useRouter();
  const { token, isAuthenticated, logout } = useAuth();
  const [letters, setLetters] = useState<PendingLetter[]>([]);
  const [loading, setLoading] = useState(true);
  const [actionLoading, setActionLoading] = useState<string | null>(null);
  const [error, setError] = useState("");

  const handleUnauthorized = useCallback(() => {
    logout();
    router.push("/login");
  }, [logout, router]);

  const fetchPending = useCallback(async () => {
    setLoading(true);
    setError("");
    try {
      const res = await fetch("/api/moderation/pending", {
        headers: { Authorization: `Bearer ${token}` },
      });

      if (res.status === 401) {
        handleUnauthorized();
        return;
      }

      if (!res.ok) {
        throw new Error("Villa við að sækja bréf.");
      }

      const data: PendingLetter[] = await res.json();
      setLetters(data);
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Villa við að sækja bréf."
      );
    } finally {
      setLoading(false);
    }
  }, [token, handleUnauthorized]);

  useEffect(() => {
    if (!isAuthenticated) {
      router.push("/login");
      return;
    }
    fetchPending();
  }, [isAuthenticated, router, fetchPending]);

  const handleAction = async (id: string, action: "approve" | "reject") => {
    setActionLoading(id);
    try {
      const res = await fetch(`/api/moderation/${id}/${action}`, {
        method: "POST",
        headers: { Authorization: `Bearer ${token}` },
      });

      if (res.status === 401) {
        handleUnauthorized();
        return;
      }

      if (!res.ok) {
        throw new Error("Villa við aðgerð.");
      }

      setLetters((prev) => prev.filter((l) => l.id !== id));
    } catch (err) {
      setError(err instanceof Error ? err.message : "Villa við aðgerð.");
    } finally {
      setActionLoading(null);
    }
  };

  const formatDate = (dateStr: string) => {
    try {
      const date = new Date(dateStr);
      return date.toLocaleDateString("is-IS", {
        year: "numeric",
        month: "short",
        day: "numeric",
        hour: "2-digit",
        minute: "2-digit",
      });
    } catch {
      return dateStr;
    }
  };

  if (!isAuthenticated) {
    return null;
  }

  return (
    <div className="mx-auto max-w-4xl px-4 py-10 sm:px-6 sm:py-16">
      <div className="mb-8 flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-foreground sm:text-4xl">
            Stjórnborð
          </h1>
          <p className="mt-1 text-muted">
            Yfirferð á innsendum bréfum.
          </p>
        </div>
        <button
          onClick={fetchPending}
          disabled={loading}
          className="rounded-lg border border-card-border px-4 py-2 text-sm font-medium text-muted transition-colors hover:border-primary hover:text-primary disabled:opacity-60"
        >
          Endurhlaða
        </button>
      </div>

      {error && (
        <div className="mb-6 rounded-lg bg-error/10 px-4 py-3 text-sm text-error">
          {error}
        </div>
      )}

      {loading && (
        <div className="flex justify-center py-16">
          <div className="h-8 w-8 animate-spin rounded-full border-2 border-primary border-t-transparent" />
        </div>
      )}

      {!loading && letters.length === 0 && (
        <div className="rounded-xl border border-card-border bg-card-bg p-8 text-center">
          <p className="text-lg text-muted">Engin bréf bíða yfirferðar.</p>
        </div>
      )}

      {!loading && letters.length > 0 && (
        <div className="space-y-4">
          {letters.map((letter) => (
            <div
              key={letter.id}
              className="animate-fade-in rounded-xl border border-card-border bg-card-bg p-5 sm:p-6"
            >
              <div className="mb-1 flex flex-wrap items-center justify-between gap-2">
                <h2 className="text-lg font-semibold text-foreground">
                  {letter.title}
                </h2>
                <span className="text-xs text-muted">
                  {formatDate(letter.submittedAt)}
                </span>
              </div>

              {letter.email && (
                <p className="mb-3 text-xs text-muted">
                  Netfang: {letter.email}
                </p>
              )}

              <p className="mb-4 whitespace-pre-wrap text-sm leading-relaxed text-foreground/80">
                {letter.body}
              </p>

              <div className="flex gap-3">
                <button
                  onClick={() => handleAction(letter.id, "approve")}
                  disabled={actionLoading === letter.id}
                  className="rounded-lg bg-success/10 px-5 py-2 text-sm font-medium text-success transition-colors hover:bg-success/20 disabled:opacity-60"
                >
                  {actionLoading === letter.id
                    ? "Hleður..."
                    : "Samþykkja"}
                </button>
                <button
                  onClick={() => handleAction(letter.id, "reject")}
                  disabled={actionLoading === letter.id}
                  className="rounded-lg bg-error/10 px-5 py-2 text-sm font-medium text-error transition-colors hover:bg-error/20 disabled:opacity-60"
                >
                  {actionLoading === letter.id ? "Hleður..." : "Hafna"}
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
