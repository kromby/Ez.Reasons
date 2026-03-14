"use client";

import { useState } from "react";
import SubmitForm from "@/components/SubmitForm";

export default function SubmitPage() {
  const [submitted, setSubmitted] = useState(false);

  if (submitted) {
    return (
      <div className="mx-auto max-w-2xl px-4 py-10 sm:px-6 sm:py-16">
        <div className="animate-fade-in rounded-xl border border-card-border bg-card-bg p-8 text-center">
          <div className="mb-4 text-4xl" aria-hidden="true">
            &#9993;
          </div>
          <h1 className="mb-2 text-2xl font-bold text-foreground">
            Takk fyrir!
          </h1>
          <p className="text-muted">
            Bréfið þitt verður skoðað fljótlega.
          </p>
          <button
            onClick={() => setSubmitted(false)}
            className="mt-6 rounded-lg bg-primary px-6 py-2.5 text-sm font-semibold text-white transition-colors hover:bg-primary-hover"
          >
            Senda annað bréf
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-2xl px-4 py-10 sm:px-6 sm:py-16">
      <h1 className="mb-2 text-3xl font-bold tracking-tight text-foreground sm:text-4xl">
        Senda bréf
      </h1>
      <p className="mb-8 text-muted">
        Skrifaðu nafnlaust uppörvandi bréf. Það verður yfirfarið áður en það
        birtist.
      </p>

      <div className="rounded-xl border border-card-border bg-card-bg p-6 sm:p-8">
        <SubmitForm onSuccess={() => setSubmitted(true)} />
      </div>
    </div>
  );
}
