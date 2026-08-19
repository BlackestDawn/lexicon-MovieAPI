export default function GenreBadge({ name }: { name: string }) {
  return (
    <div className="p-2 border border-border rounded-md hover:border-primary hover:text-primary transition-colors">
      <span>{name}</span>
    </div>
  );
}
