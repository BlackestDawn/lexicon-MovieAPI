import z from "zod";

// The backend writes this via a raw JsonSerializer.Serialize call (not the
// MVC pipeline's camelCase settings), so the X-Pagination header keeps the
// PascalCase property names of MovieAPI.Infrastructure.Models.PaginationMetadata.
const paginationMetadataSchema = z.object({
  TotalItemCount: z.number(),
  TotalPageCount: z.number(),
  PageSize: z.number(),
  CurrentPage: z.number(),
});

export type PaginationMetadata = z.infer<typeof paginationMetadataSchema>;

export function parsePaginationHeader(
  header: string | null | undefined,
): PaginationMetadata | null {
  if (!header) return null;

  const result = paginationMetadataSchema.safeParse(JSON.parse(header));
  if (!result.success) {
    console.error("Invalid X-Pagination header:", result.error);
    throw new Error("invalid X-Pagination header");
  }
  return result.data;
}
