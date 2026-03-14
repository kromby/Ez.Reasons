# Ez.Reasons — Product Requirements Document

## Summary

Ez.Reasons is an Icelandic-language website that displays random anonymous letters of encouragement. Visitors read one letter at a time and can request another. Anyone can submit a letter. Moderators log in to approve or reject submitted letters before they become visible.

---

## Problem

There is no Icelandic-language platform for sharing anonymous letters of encouragement. Existing English-language services (e.g. reasonstostay.co.uk) do not serve Icelandic speakers. A simple, moderated letter platform fills this gap.

---

## Goals

- Display one approved letter per page load, with the ability to request another
- Rank letters by quality score (likes minus dislikes) so better-received letters appear more often
- Avoid showing a visitor a letter they have already seen (tracked per-browser via localStorage)
- Offer optional feedback (like or dislike) when requesting the next letter
- Log how often each letter has been displayed (view counter on the letter)
- Allow anyone to submit a letter (title, body, optional email) without creating an account
- Provide a moderation workflow where logged-in moderators approve or reject submissions
- Only approved letters are visible to the public
- All user-facing text is in Icelandic; all URL paths are in English

---

## Non-goals

- User accounts or authentication for letter submitters
- Email notifications to submitters when their letter is approved/rejected
- Full-text search across letters
- Multi-language support or language switching
- Email subscriptions or newsletters
- Embeddable widgets
- Moderator account management via UI (accounts are created manually)
- Rich text or markdown in letters
- Rate limiting on submissions (can be added later)

---

## Requirements

### Letters

- A letter has a title (max 200 characters), body (max 5000 characters), and an optional submitter email (never displayed publicly).
- Letters have three statuses: pending, approved, rejected.
- New submissions start as pending.
- Only approved letters are visible to visitors.
- Each letter tracks: view count, like count, dislike count.

### Scoring and Selection

- Quality score = likes minus dislikes. Letters with higher scores are more likely to be shown.
- The visitor's browser stores IDs of previously seen letters in localStorage.
- When requesting a letter, the API receives the list of seen IDs and excludes them from candidates.
- If all approved letters have been seen, the seen list is ignored and any approved letter may be shown.

### Feedback

- When a visitor clicks "Naesta bref", they are shown an optional feedback prompt: like, dislike, or skip.
- Feedback is optional. The visitor can skip directly to the next letter.
- Each like increments the letter's like count. Each dislike increments the dislike count.
- Each display of a letter increments its view count.

### Submission

- Anyone can submit a letter without an account.
- The submission form collects: title, body, and email.
- Title and body are required. Email is optional but must be a valid format if provided.
- After submission, the visitor sees a "thank you" message.

### Moderation

- Moderators log in with a username and password.
- The moderation dashboard lists all pending letters, showing title, body, email, and submission date.
- For each pending letter, a moderator can approve or reject it.
- Approved letters become visible on the home page. Rejected letters are hidden permanently.

### Reading

- The home page displays one approved letter (title and body).
- A button labeled "Naesta bref" loads another letter.
- Before loading the next letter, an optional feedback prompt appears (like, dislike, or skip).
- If no approved letters exist, an empty state message is shown instead.

### Pages

| URL | Page | Description |
|---|---|---|
| `/` | Home | Approved letter with feedback and "next letter" button |
| `/about` | About | Static Icelandic text about the project |
| `/submit` | Submit | Letter submission form |
| `/login` | Login | Moderator login form |
| `/dashboard` | Dashboard | Pending letter list with approve/reject actions |

---

## User Flows

### Read a letter
1. Visitor opens the home page
2. One approved letter is displayed (selected by score, excluding previously seen)
3. Visitor clicks "Naesta bref"
4. An optional feedback prompt appears: like, dislike, or skip
5. A different letter replaces the current one

### Submit a letter
1. Visitor navigates to `/submit`
2. Fills in title, body, and optionally email
3. Submits the form
4. Sees a thank you message

### Moderate letters
1. Moderator navigates to `/login`
2. Enters username and password
3. Redirected to `/dashboard`
4. Sees a list of pending letters with submitter email
5. Clicks "Samthykkja" (Approve) or "Hafna" (Reject) on a letter
6. The letter is removed from the pending list

---

## Edge Cases

- **No approved letters exist**: Home page displays an empty state message. The "next letter" button is hidden.
- **Only one approved letter exists**: The "next letter" button loads the same letter. Acceptable for v1.
- **All letters have been seen**: The seen list is cleared and letters may repeat.
- **Two moderators act on the same letter simultaneously**: The second action fails. The dashboard shows an error and refreshes the list.
- **Letter body at max length**: 5000 characters must display without truncation.
- **Session expires during moderation**: The moderator is redirected to the login page.
- **Invalid email on submission**: The form prevents submission. The API returns a validation error.
- **localStorage unavailable or cleared**: Seen tracking resets. The visitor may see repeated letters. This is acceptable.

---

## Success Metrics

- **Availability**: Site loads and displays a letter within 2 seconds on a standard connection
- **Letter throughput**: At least 100 letters stored and served without degradation
- **Moderation latency**: Approve/reject action completes within 1 second
- **Submission success rate**: >99% of valid form submissions result in a stored pending letter

---

## Dependencies and Blockers

- **Azure subscription**: Required for hosting and data storage
- **First moderator account**: Must be created before moderation can begin
