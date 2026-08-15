import { QueryParams } from "../interfaces/general";

export function toQueryParams(params?: QueryParams): string {
  if (params === undefined) return "";
  const query = new URLSearchParams();

  const append = (key: string, value: unknown) => {
    if (value === undefined || value === null) return;

    if (Array.isArray(value)) {
      value.forEach((v) => append(key, v));
    } else if (value instanceof Date) {
      query.append(key, value.toISOString());
    } else if (typeof value === "object") {
      query.append(key, JSON.stringify(value));
    } else {
      query.append(key, String(value));
    }
  };

  for (const [key, value] of Object.entries(params)) append(key, value);
  if (query.size === 0) return "";

  return "?" + query.toString();
}

export function minsToDisplayRuntime(mins: number): string {
  return `${Math.floor(mins / 60)}h ${mins % 60}m`;
}
