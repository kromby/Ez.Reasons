"use client";

import { useState } from "react";
import { sendFeedback, type FeedbackType } from "@/lib/api";

interface FeedbackPromptProps {
  letterId: string;
  onComplete: () => void;
}

export default function FeedbackPrompt({
  letterId,
  onComplete,
}: FeedbackPromptProps) {
  const [submitting, setSubmitting] = useState(false);

  const handleFeedback = async (type: FeedbackType) => {
    setSubmitting(true);
    try {
      await sendFeedback(letterId, type);
    } catch {
      // Feedback is non-critical; proceed even on failure
    } finally {
      setSubmitting(false);
      onComplete();
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-foreground/30 backdrop-blur-sm">
      <div className="animate-slide-up mx-4 w-full max-w-sm rounded-xl border border-card-border bg-card-bg p-6 shadow-lg">
        <h3 className="mb-2 text-center text-lg font-semibold text-foreground">
          Hvað finnst þér?
        </h3>
        <p className="mb-5 text-center text-sm text-muted">
          Endurgjöf þín hjálpar okkur að bæta upplifunina.
        </p>
        <div className="flex flex-col gap-3">
          <button
            onClick={() => handleFeedback("like")}
            disabled={submitting}
            className="flex items-center justify-center gap-2 rounded-lg bg-success/10 px-4 py-2.5 text-sm font-medium text-success transition-colors hover:bg-success/20 disabled:opacity-60"
          >
            <span aria-hidden="true">&#128077;</span> Líkar
          </button>
          <button
            onClick={() => handleFeedback("dislike")}
            disabled={submitting}
            className="flex items-center justify-center gap-2 rounded-lg bg-error/10 px-4 py-2.5 text-sm font-medium text-error transition-colors hover:bg-error/20 disabled:opacity-60"
          >
            <span aria-hidden="true">&#128078;</span> Líkar ekki
          </button>
          <button
            onClick={onComplete}
            disabled={submitting}
            className="flex items-center justify-center gap-2 rounded-lg bg-card-border/50 px-4 py-2.5 text-sm font-medium text-muted transition-colors hover:bg-card-border disabled:opacity-60"
          >
            <span aria-hidden="true">&#9193;</span> Sleppa
          </button>
        </div>
      </div>
    </div>
  );
}
