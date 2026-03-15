// API service layer for Ez.Reasons
// Framework-agnostic: auth tokens are passed as parameters, not imported from hooks.

// ---------------------------------------------------------------------------
// Types
// ---------------------------------------------------------------------------

export interface Letter {
  id: string;
  title: string;
  body: string;
  submittedAt: string;
}

export interface PendingLetter {
  id: string;
  title: string;
  body: string;
  email: string;
  submittedAt: string;
}

export interface SubmitLetterRequest {
  title: string;
  body: string;
  email?: string;
}

export interface LoginResponse {
  token: string;
}

export type FeedbackType = "like" | "dislike";

export type ModerationAction = "approve" | "reject";

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

class ApiError extends Error {
  constructor(
    message: string,
    public status: number,
  ) {
    super(message);
    this.name = "ApiError";
  }
}

export { ApiError };

async function parseErrorMessage(res: Response, fallback: string): Promise<string> {
  try {
    const data = await res.json();
    return data?.error || fallback;
  } catch {
    return fallback;
  }
}

// ---------------------------------------------------------------------------
// Public API – Letters
// ---------------------------------------------------------------------------

/**
 * Fetch the next unseen letter.
 * Returns `null` when there are no more letters (404).
 */
export async function fetchNextLetter(seenIds: string[]): Promise<Letter | null> {
  const res = await fetch("/api/letters/next", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ seenIds }),
  });

  if (res.status === 404) {
    return null;
  }

  if (!res.ok) {
    const message = await parseErrorMessage(res, "Failed to load letter");
    throw new ApiError(message, res.status);
  }

  return res.json() as Promise<Letter>;
}

/**
 * Submit a new letter for moderation.
 */
export async function submitLetter(request: SubmitLetterRequest): Promise<void> {
  const res = await fetch("/api/letters", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(request),
  });

  if (!res.ok) {
    const message = await parseErrorMessage(res, "Villa kom upp við að senda bréf.");
    throw new ApiError(message, res.status);
  }
}

/**
 * Send feedback (like / dislike) for a letter.
 * Feedback is best-effort; callers may choose to ignore errors.
 */
export async function sendFeedback(letterId: string, type: FeedbackType): Promise<void> {
  const res = await fetch(`/api/letters/${letterId}/feedback`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ type }),
  });

  if (!res.ok) {
    const message = await parseErrorMessage(res, "Failed to send feedback");
    throw new ApiError(message, res.status);
  }
}

// ---------------------------------------------------------------------------
// Public API – Auth
// ---------------------------------------------------------------------------

/**
 * Authenticate a moderator and return a JWT token.
 */
export async function login(username: string, password: string): Promise<LoginResponse> {
  const res = await fetch("/api/auth/login", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ username, password }),
  });

  if (!res.ok) {
    const message = await parseErrorMessage(res, "Rangt notendanafn eða lykilorð.");
    throw new ApiError(message, res.status);
  }

  return res.json() as Promise<LoginResponse>;
}

// ---------------------------------------------------------------------------
// Public API – Moderation (requires auth token)
// ---------------------------------------------------------------------------

/**
 * Fetch all letters pending moderation.
 */
export async function fetchPendingLetters(token: string): Promise<PendingLetter[]> {
  const res = await fetch("/api/moderation/pending", {
    headers: { "X-Auth-Token": token },
  });

  if (!res.ok) {
    const message = await parseErrorMessage(res, "Villa við að sækja bréf.");
    throw new ApiError(message, res.status);
  }

  return res.json() as Promise<PendingLetter[]>;
}

/**
 * Approve or reject a pending letter.
 */
export async function moderateLetter(
  id: string,
  action: ModerationAction,
  token: string,
): Promise<void> {
  const res = await fetch(`/api/moderation/${id}/${action}`, {
    method: "POST",
    headers: { "X-Auth-Token": token },
  });

  if (!res.ok) {
    const message = await parseErrorMessage(res, "Villa við aðgerð.");
    throw new ApiError(message, res.status);
  }
}
