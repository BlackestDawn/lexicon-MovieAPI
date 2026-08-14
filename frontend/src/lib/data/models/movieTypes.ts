import z from "zod";
import { castCrewDtoSchema, castCrewForChangeSchema } from "./castCrewTypes";
import { genreDtoSchema } from "./genreTypes";
import { reviewDtoSchema } from "./reviewTypes";
import { ValidationError } from "../interfaces/errors";

const movieDtoSchema = z.object({
  id: z.guid(),
  createdAt: z.coerce.date(),
  updatedAt: z.coerce.date(),
  title: z.string(),
  releaseDate: z.coerce.date(),
  plotSummery: z.string(),
  runtimeMinutes: z.number(),
  averageRating: z.number(),
  genres: z.array(genreDtoSchema),
});

export type MovieDto = z.infer<typeof movieDtoSchema>;

export function validateMovieDto(
  item: unknown | unknown[],
): MovieDto | MovieDto[] {
  if (Array.isArray(item)) {
    const result = movieDtoSchema.array().safeParse(item);
    if (!result.success) {
      console.error("Invalid MovieDto:", result.error);
      throw new Error("invalid MovieDto item");
    }
    return result.data;
  }

  const result = movieDtoSchema.safeParse(item);
  if (!result.success) {
    console.error("Invalid MovieDto:", result.error);
    throw new Error("invalid MovieDto item");
  }
  return result.data;
}

const movieDetailDtoSchema = z.object({
  id: z.guid(),
  synopsis: z.string(),
  language: z.string(),
  budget: z.number(),
});

export type MovieDetailDto = z.infer<typeof movieDetailDtoSchema>;

export function validateMovieDetailDto(item: unknown): MovieDetailDto {
  const result = movieDetailDtoSchema.safeParse(item);
  if (!result.success) {
    console.error("Invalid MovieDetailDto:", result.error);
    throw new Error("invalid MovieDetailDto item");
  }
  return result.data;
}

const movieExtendedDtoSchema = z.object({
  id: z.guid(),
  createdAt: z.coerce.date(),
  updatedAt: z.coerce.date(),
  title: z.string(),
  releaseDate: z.coerce.date(),
  plotSummery: z.string(),
  runtimeMinutes: z.number(),
  castCrews: z.array(castCrewDtoSchema).nullable().optional(),
  genres: z.array(genreDtoSchema),
  reviews: z.array(reviewDtoSchema),
  details: movieDetailDtoSchema.nullable().optional(),
  averageRating: z.number(),
});

export type MovieExtendedDto = z.infer<typeof movieExtendedDtoSchema>;

export function validateMovieExtendedDto(item: unknown): MovieExtendedDto {
  const result = movieExtendedDtoSchema.safeParse(item);
  if (!result.success) {
    console.error("Invalid MovieExtendedDto:", result.error);
    throw new Error("invalid MovieExtendedDto item");
  }
  return result.data;
}

// Mirrors MovieChangeValidator: release date window, positive runtime/budget,
// and at least one genre and one cast/crew member.
export const movieForChangeDtoSchema = z.object({
  title: z.string().min(1, "Title is required"),
  releaseDate: z.coerce
    .date()
    .min(new Date(1850, 0, 1))
    .max(new Date(new Date().getFullYear() + 10, 11, 31)),
  plotSummery: z.string().min(1, "Plot summary is required"),
  runtimeMinutes: z.number().int().gt(0, "Runtime must be positive"),
  castCrews: z
    .array(castCrewForChangeSchema)
    .min(1, "Must have at least 1 person for cast or crew"),
  genres: z.array(z.guid()).min(1, "Must have at least 1 genre"),
  synopsis: z.string().min(1, "Synopsis is required"),
  language: z.string().min(1, "Language is required"),
  budget: z.number().int().gt(0, "Budget must be positive"),
});

export type MovieForChangeDto = z.infer<typeof movieForChangeDtoSchema>;

export function validateMovieForChangeDto(item: unknown): MovieForChangeDto {
  const result = movieForChangeDtoSchema.safeParse(item);
  if (!result.success) {
    console.error("Invalid MovieForChangeDto:", result.error);
    throw new ValidationError("invalid MovieForChangeDto item", result.error.issues.map(e => e.message));
  }
  return result.data;
}
