import { genrePillClass } from "@/lib/data/consts/styles";

export default function GenreBadge({ name }: { name: string }) {
  return <span className={genrePillClass}>{name}</span>;
}
