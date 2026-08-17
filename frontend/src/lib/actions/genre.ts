"use server";

import { revalidatePath } from "next/cache";
import { QueryParams } from "../data/interfaces/general";
import { GenreSearchOptions } from "../data/interfaces/genre";
import {
  GenreDto,
  GenreExtendedDto,
  GenreForChangeDto,
  validateGenreDto,
  validateGenreExtendedDto,
  validateGenreForChangeDto,
} from "../data/models/genreTypes";
import { PaginationMetadata } from "../data/models/paginationTypes";
import { toQueryParams } from "../data/utils/converters";
import {
  apiDelete,
  apiGet,
  apiGetPaginated,
  apiPost,
  apiPut,
} from "./apiInteract";
import { ValidationError } from "../data/interfaces/errors";

export async function fetchGenres(): Promise<GenreDto[]> {
  const result = await apiGet<GenreDto>("/genres");
  return validateGenreDto(result) as GenreDto[];
}

export async function getGenre(
  id: string,
  options?: GenreSearchOptions,
): Promise<{ genre: GenreExtendedDto; pagination: PaginationMetadata | null }> {
  const qs = toQueryParams(options as QueryParams);

  const { data, pagination } = await apiGetPaginated(`/genres/${id}${qs}`);
  const validated = validateGenreExtendedDto(data);

  return { genre: validated as GenreExtendedDto, pagination };
}

export async function createGenre(formData: FormData) {
  try {
    const data = formToGenreChangeData(formData);

    const result = await apiPost("/genres", data);
    const validated = validateGenreDto(result);
    revalidatePath("/genres");
    return { success: true, genre: validated };
  } catch (e) {
    console.error("Error creating genre:", e);
    return {
      success: false,
      error: e instanceof Error ? e.message : "Genre creation failed",
      issues: e instanceof ValidationError ? e.issues : null,
    };
  }
}

export async function updateGenre(id: string, formData: FormData) {
  try {
    const data = formToGenreChangeData(formData);

    await apiPut(`/genres/${id}`, data);
    revalidatePath(`/genres/${id}`);
    return { success: true, genre: data };
  } catch (e) {
    console.error("Error updating genre:", e);
    return {
      success: false,
      error: e instanceof Error ? e.message : "Genre update failed",
      issues: e instanceof ValidationError ? e.issues : null,
    };
  }
}

export async function removeGenre(id: string) {
  await apiDelete<void>(`/genres/${id}`);
  revalidatePath("/genres");
}

function formToGenreChangeData(data: FormData) {
  const parsed: GenreForChangeDto = {
    name: data.get("name") as string,
    slug: data.get("slug") as string,
  };

  return validateGenreForChangeDto(parsed);
}
