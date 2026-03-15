"use client";

import { useState, useEffect, useCallback } from "react";
import LetterCard from "@/components/LetterCard";
import FeedbackPrompt from "@/components/FeedbackPrompt";
import { fetchNextLetter, type Letter } from "@/lib/api";

const SEEN_KEY = "ez-reasons-seen";

function getSeenIds(): string[] {
  try {
    const stored = localStorage.getItem(SEEN_KEY);
    return stored ? JSON.parse(stored) : [];
  } catch {
    return [];
  }
}

function addSeenId(id: string) {
  const seen = getSeenIds();
  if (!seen.includes(id)) {
    seen.push(id);
    localStorage.setItem(SEEN_KEY, JSON.stringify(seen));
  }
}

export default function HomePage() {
  const [letter, setLetter] = useState<Letter | null>(null);
  const [loading, setLoading] = useState(true);
  const [empty, setEmpty] = useState(false);
  const [showFeedback, setShowFeedback] = useState(false);

  const loadLetter = useCallback(async () => {
    setLoading(true);
    setEmpty(false);
    try {
      const seenIds = getSeenIds();
      const data = await fetchNextLetter(seenIds);

      if (!data) {
        setLetter(null);
        setEmpty(true);
        return;
      }

      setLetter(data);
      addSeenId(data.id);
    } catch {
      setLetter(null);
      setEmpty(true);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadLetter();
  }, [loadLetter]);

  const handleNextClick = () => {
    if (letter) {
      setShowFeedback(true);
    } else {
      loadLetter();
    }
  };

  const handleFeedbackComplete = () => {
    setShowFeedback(false);
    loadLetter();
  };

  return (
    <div className="mx-auto max-w-2xl px-4 py-10 sm:px-6 sm:py-16">
      <div className="mb-8 text-center">
        <h1 className="mb-2 text-3xl font-bold tracking-tight text-foreground sm:text-4xl">
          Uppörvandi bréf
        </h1>
        <p className="text-muted">
          Nafnlaus bréf full af jákvæðni og uppörvun.
        </p>
      </div>

      {loading && (
        <div className="flex justify-center py-16">
          <div className="h-8 w-8 animate-spin rounded-full border-2 border-primary border-t-transparent" />
        </div>
      )}

      {!loading && empty && (
        <div className="animate-fade-in rounded-xl border border-card-border bg-card-bg p-8 text-center">
          <p className="text-lg text-muted">
            Engin bréf eru tiltæk eins og er.
          </p>
          <p className="mt-2 text-sm text-muted">
            Komdu aftur síðar eða{" "}
            <a href="/submit" className="text-primary hover:underline">
              sendu bréf
            </a>{" "}
            sjálf/ur.
          </p>
        </div>
      )}

      {!loading && letter && (
        <div className="animate-slide-up">
          <LetterCard
            title={letter.title}
            body={letter.body}
            submittedAt={letter.submittedAt}
          />
        </div>
      )}

      {!loading && (
        <div className="mt-8 flex justify-center">
          <button
            onClick={handleNextClick}
            className="rounded-lg bg-primary px-8 py-3 text-sm font-semibold text-white transition-colors hover:bg-primary-hover"
          >
            Næsta bréf
          </button>
        </div>
      )}

      {showFeedback && letter && (
        <FeedbackPrompt
          letterId={letter.id}
          onComplete={handleFeedbackComplete}
        />
      )}
    </div>
  );
}
