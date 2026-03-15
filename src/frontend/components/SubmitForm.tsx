"use client";

import { useState, type FormEvent } from "react";
import { submitLetter } from "@/lib/api";

interface SubmitFormProps {
  onSuccess: () => void;
}

export default function SubmitForm({ onSuccess }: SubmitFormProps) {
  const [title, setTitle] = useState("");
  const [body, setBody] = useState("");
  const [email, setEmail] = useState("");
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [loading, setLoading] = useState(false);
  const [apiError, setApiError] = useState("");

  const validate = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!title.trim()) {
      newErrors.title = "Titill er nauðsynlegur.";
    } else if (title.length > 200) {
      newErrors.title = "Titill má ekki vera lengri en 200 stafir.";
    }

    if (!body.trim()) {
      newErrors.body = "Bréf er nauðsynlegt.";
    } else if (body.length > 5000) {
      newErrors.body = "Bréf má ekki vera lengra en 5000 stafir.";
    }

    if (email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
      newErrors.email = "Netfang er ekki á réttu sniði.";
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setApiError("");

    if (!validate()) return;

    setLoading(true);
    try {
      await submitLetter({
        title: title.trim(),
        body: body.trim(),
        email: email.trim() || undefined,
      });

      onSuccess();
    } catch (err) {
      setApiError(
        err instanceof Error ? err.message : "Villa kom upp við að senda bréf."
      );
    } finally {
      setLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-5" noValidate>
      <div>
        <label
          htmlFor="title"
          className="mb-1 block text-sm font-medium text-foreground"
        >
          Titill <span className="text-error">*</span>
        </label>
        <input
          id="title"
          type="text"
          maxLength={200}
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          className="w-full rounded-lg border border-card-border bg-card-bg px-4 py-2.5 text-foreground placeholder-muted outline-none transition-colors focus:border-primary focus:ring-1 focus:ring-primary"
          placeholder="Gefðu bréfinu titil..."
        />
        <div className="mt-1 flex justify-between">
          {errors.title && (
            <p className="text-xs text-error">{errors.title}</p>
          )}
          <p className="ml-auto text-xs text-muted">{title.length}/200</p>
        </div>
      </div>

      <div>
        <label
          htmlFor="body"
          className="mb-1 block text-sm font-medium text-foreground"
        >
          Bréf <span className="text-error">*</span>
        </label>
        <textarea
          id="body"
          maxLength={5000}
          rows={8}
          value={body}
          onChange={(e) => setBody(e.target.value)}
          className="w-full rounded-lg border border-card-border bg-card-bg px-4 py-2.5 text-foreground placeholder-muted outline-none transition-colors focus:border-primary focus:ring-1 focus:ring-primary"
          placeholder="Skrifaðu uppörvandi bréf..."
        />
        <div className="mt-1 flex justify-between">
          {errors.body && <p className="text-xs text-error">{errors.body}</p>}
          <p className="ml-auto text-xs text-muted">{body.length}/5000</p>
        </div>
      </div>

      <div>
        <label
          htmlFor="email"
          className="mb-1 block text-sm font-medium text-foreground"
        >
          Netfang{" "}
          <span className="text-xs font-normal text-muted">(valfrjálst)</span>
        </label>
        <input
          id="email"
          type="email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          className="w-full rounded-lg border border-card-border bg-card-bg px-4 py-2.5 text-foreground placeholder-muted outline-none transition-colors focus:border-primary focus:ring-1 focus:ring-primary"
          placeholder="netfang@daemi.is"
        />
        {errors.email && (
          <p className="mt-1 text-xs text-error">{errors.email}</p>
        )}
      </div>

      {apiError && (
        <div className="rounded-lg bg-error/10 px-4 py-3 text-sm text-error">
          {apiError}
        </div>
      )}

      <button
        type="submit"
        disabled={loading}
        className="w-full rounded-lg bg-primary px-6 py-3 text-sm font-semibold text-white transition-colors hover:bg-primary-hover disabled:cursor-not-allowed disabled:opacity-60"
      >
        {loading ? "Sendir..." : "Senda bréf"}
      </button>
    </form>
  );
}
