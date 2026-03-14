interface LetterCardProps {
  title: string;
  body: string;
  submittedAt: string;
}

export default function LetterCard({
  title,
  body,
  submittedAt,
}: LetterCardProps) {
  const formatDate = (dateStr: string) => {
    try {
      const date = new Date(dateStr);
      return date.toLocaleDateString("is-IS", {
        year: "numeric",
        month: "long",
        day: "numeric",
      });
    } catch {
      return dateStr;
    }
  };

  return (
    <div className="animate-fade-in rounded-xl border border-card-border bg-card-bg p-6 shadow-sm sm:p-8">
      <div className="mb-1 flex items-center gap-2">
        <span className="text-accent" aria-hidden="true">
          &#9830;
        </span>
        <h2 className="text-xl font-semibold text-foreground sm:text-2xl">
          {title}
        </h2>
      </div>
      <p className="mb-6 text-xs text-muted">{formatDate(submittedAt)}</p>
      <div className="whitespace-pre-wrap leading-relaxed text-foreground/85">
        {body}
      </div>
    </div>
  );
}
