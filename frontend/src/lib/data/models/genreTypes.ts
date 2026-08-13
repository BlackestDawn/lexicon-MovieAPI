import z from "zod";
import { movieSimpleDtoSchema } from "./movieSimpleTypes";

export const genreDtoSchema = z.object({
  id: z.guid(),
  name: z.string(),
  slug: z.string(),
});

export type GenreDto = z.infer<typeof genreDtoSchema>;

export function validateGenreDto(
  item: unknown | unknown[],
): GenreDto | GenreDto[] {
  if (Array.isArray(item)) {
    const result = genreDtoSchema.array().safeParse(item);
    if (!result.success) {
      console.error("Invalid GenreDto:", result.error);
      throw new Error("invalid GenreDto item");
    }
    return result.data;
  }

  const result = genreDtoSchema.safeParse(item);
  if (!result.success) {
    console.error("Invalid GenreDto:", result.error);
    throw new Error("invalid GenreDto item");
  }
  return result.data;
}

export const genreExtendedDtoSchema = z.object({
  id: z.guid(),
  name: z.string(),
  slug: z.string(),
  movies: z.array(movieSimpleDtoSchema),
});

export type GenreExtendedDto = z.infer<typeof genreExtendedDtoSchema>;

export function validateGenreExtendedDto(item: unknown): GenreExtendedDto {
  const result = genreExtendedDtoSchema.safeParse(item);
  if (!result.success) {
    console.error("Invalid GenreExtendedDto:", result.error);
    throw new Error("invalid GenreExtendedDto item");
  }
  return result.data;
}

// Mirrors GenreChangeValidator: both fields required.
export const genreForChangeSchema = z.object({
  name: z.string().min(1, "Name is required"),
  slug: z.string().min(1, "Slug is required"),
});

export type GenreForChange = z.infer<typeof genreForChangeSchema>;
