"use server";

import { revalidatePath } from "next/cache";
import { QueryParams } from "../data/interfaces/general";
import { MovieSearchOptions } from "../data/interfaces/movie";
import {
  MovieDto,
  MovieExtendedDto,
  MovieForChangeDto,
  validateMovieDto,
  validateMovieExtendedDto,
  validateMovieForChangeDto,
} from "../data/models/movieTypes";
import { toQueryParams } from "../data/utils/converters";
import { apiDelete, apiGet, apiPost, apiPut } from "./apiInteract";
import { ValidationError } from "../data/interfaces/errors";

export async function fetchMovies(
  options?: MovieSearchOptions,
): Promise<MovieDto[]> {
  const qs = toQueryParams(options as QueryParams);
  const url = `/movies${qs}`;

  const result = await apiGet<MovieDto[]>(url);
  const validated = validateMovieDto(result);

  return validated as MovieDto[];
}

export async function getMovie(
  id: string,
  includePeople?: boolean,
): Promise<MovieExtendedDto> {
  const qs = toQueryParams({ includePeople });
  const url = `/movies/${id}${qs}`;

  const result = await apiGet<MovieExtendedDto>(url);
  const validated = validateMovieExtendedDto(result);

  return validated as MovieExtendedDto;
}

export async function createMovie(formData: FormData) {
  try {
    const data = formToMovieChangeData(formData);

    const result = await apiPost<MovieDto>("/movies", data);
    const validated = validateMovieDto(result)

    revalidatePath("/movies");
    return { success: true, movie: validated };
  } catch (e) {
    console.error("Error updating movie:", e);
    return {
      success: false,
      error: e instanceof Error ? e.message : "Movie update failed",
      issues: e instanceof ValidationError ? e.issues : null,
    };
  }
}

export async function updateMovie(id: string, formData: FormData) {
  try {
    const data = formToMovieChangeData(formData);

    await apiPut<void>(`/movies/${id}`, data);

    revalidatePath(`/movies/${id}`);
    return { success: true, movie: data };
  } catch (e) {
    console.error("Error updating movie:", e);
    return {
      success: false,
      error: e instanceof Error ? e.message : "Movie update failed",
      issues: e instanceof ValidationError ? e.issues : null,
    };
  }
}

export async function removeMovie(id: string) {
  try {
    await apiDelete<void>(`/movies/${id}`);

    revalidatePath("/movies");
    return { success: true };
  } catch (e) {
    console.error("Error deleting movie:", e);
    return {
      success: false,
      error: e instanceof Error ? e.message : "Movie deletion failed",
    };
  }
}

function formToMovieChangeData(data: FormData): MovieForChangeDto {
  const parsed = {
    title: data.get("title"),
    releaseDate: data.get("releaseDate"),
    plotSummery: data.get("plotSummery"),
    runtimeMinutes: Number(data.get("runtimeMinutes")),
    castCrews: JSON.parse(data.get("castCrewData") as string),
    genres: data.getAll("genres"),
    synopsis: data.get("synopsis"),
    language: data.get("language"),
    budget: Number(data.get("budget")),
  };

  return validateMovieForChangeDto(parsed);
}
