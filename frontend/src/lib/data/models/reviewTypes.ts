import z from "zod";
import { ValidationError } from "../interfaces/errors";

export const reviewDtoSchema = z.object({
  id: z.guid(),
  createdAt: z.coerce.date(),
  updatedAt: z.coerce.date(),
  authorName: z.string(),
  body: z.string(),
  score: z.number(),
  userId: z.guid().nullable().optional(),
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

// Mirrors ReviewChangeValidator: score must be in (0, 10]. authorName isn't part of
// this - it's derived server-side from the poster's account DisplayName.
export const reviewForChangeDtoSchema = z.object({
  body: z.string().min(1, "Body is required"),
  score: z.number().int().gt(0).lte(10),
});

export type ReviewForChangeDto = z.infer<typeof reviewForChangeDtoSchema>;

export function validateReviewForChangeDto(item: unknown) {
  const result = reviewForChangeDtoSchema.safeParse(item);
  if (!result.success) {
    console.error("Invalid ReviewForChangeDto:", result.error);
    throw new ValidationError(
      "invalid ReviewForChangeDto item:",
      result.error.issues.map((e) => e.message),
    );
  }
  return result.data;
}
