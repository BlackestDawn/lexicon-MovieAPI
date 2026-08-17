import z from "zod";
import { movieSimpleDtoSchema } from "./movieSimpleTypes";
import { ValidationError } from "../interfaces/errors";

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
export const genreForChangeDtoSchema = z.object({
  name: z.string().min(1, "Name is required"),
  slug: z.string().min(1, "Slug is required"),
});

export type GenreForChangeDto = z.infer<typeof genreForChangeDtoSchema>;

export function validateGenreForChangeDto(item: unknown) {
  const result = genreForChangeDtoSchema.safeParse(item);
  if (!result.success) {
    console.error("Invalid GenreForChangeDto:", result.error);
    throw new ValidationError(
      "invalid GenreForChangeDto item",
      result.error.issues.map((e) => e.message),
    );
  }
  return result.data;
}
