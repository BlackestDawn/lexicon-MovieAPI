import z from "zod";

export const reviewDtoSchema = z.object({
  id: z.guid(),
  createdAt: z.coerce.date(),
  updatedAt: z.coerce.date(),
  authorName: z.string(),
  body: z.string(),
  score: z.number(),
});

export type ReviewDto = z.infer<typeof reviewDtoSchema>;

export function validateReviewDto(
  item: unknown | unknown[],
): ReviewDto | ReviewDto[] {
  if (Array.isArray(item)) {
    const result = reviewDtoSchema.array().safeParse(item);
    if (!result.success) {
      console.error("Invalid ReviewDto:", result.error);
      throw new Error("invalid ReviewDto item");
    }
    return result.data;
  }

  const result = reviewDtoSchema.safeParse(item);
  if (!result.success) {
    console.error("Invalid ReviewDto:", result.error);
    throw new Error("invalid ReviewDto item");
  }
  return result.data;
}

// Mirrors ReviewChangeValidator: score must be in (0, 10].
export const reviewForChangeSchema = z.object({
  authorName: z.string().min(1, "Author name is required"),
  body: z.string().min(1, "Body is required"),
  score: z.number().int().gt(0).lte(10),
});

export type ReviewForChange = z.infer<typeof reviewForChangeSchema>;
