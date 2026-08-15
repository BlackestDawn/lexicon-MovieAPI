export default function GenreBadge({ name }: { name: string }) {
  return (
    <div className="p-2 border border-slate-600 dark:border-slate-400 rounded-md">
      <span>{name}</span>
    </div>
  );
}
