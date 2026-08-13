export function toQueryParams(
  params: Record<string, string | number | boolean | undefined | null>,
): string {
  if (params === undefined) return "";

  const query = new URLSearchParams();

  for (const [key, value] of Object.entries(params))
    if (value !== undefined && value !== null) query.append(key, String(value));

  return query.toString();
}
