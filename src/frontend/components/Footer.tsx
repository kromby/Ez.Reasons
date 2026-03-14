export default function Footer() {
  return (
    <footer className="border-t border-card-border bg-card-bg/50 py-6 text-center">
      <p className="text-sm text-muted">
        Ez.Reasons &copy; {new Date().getFullYear()}
      </p>
    </footer>
  );
}
