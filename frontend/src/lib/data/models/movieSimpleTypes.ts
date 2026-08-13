import z from "zod";

export const movieSimpleDtoSchema = z.object({
  id: z.guid(),
  createdAt: z.coerce.date(),
  updatedAt: z.coerce.date(),
  title: z.string(),
  releaseDate: z.coerce.date(),
  runtimeMinutes: z.number(),
  averageRating: z.number(),
});

export type MovieSimpleDto = z.infer<typeof movieSimpleDtoSchema>;

export function validateMovieSimpleDto(
  item: unknown | unknown[],
): MovieSimpleDto | MovieSimpleDto[] {
  if (Array.isArray(item)) {
    const result = movieSimpleDtoSchema.array().safeParse(item);
    if (!result.success) {
      console.error("Invalid MovieSimpleDto:", result.error);
      throw new Error("invalid MovieSimpleDto item");
    }
    return result.data;
  }

  const result = movieSimpleDtoSchema.safeParse(item);
  if (!result.success) {
    console.error("Invalid MovieSimpleDto:", result.error);
    throw new Error("invalid MovieSimpleDto item");
  }
  return result.data;
}
