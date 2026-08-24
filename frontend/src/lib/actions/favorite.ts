"use server";

import { revalidatePath } from "next/cache";
import { QueryParams } from "../data/interfaces/general";
import { MovieDto, validateMovieDto } from "../data/models/movieTypes";
import { PaginationMetadata } from "../data/models/paginationTypes";
import { toQueryParams } from "../data/utils/converters";
import { apiDelete, apiGet, apiGetPaginated, apiPost } from "./apiInteract";

type ActionResult = { success: true } | { success: false; error: string };

export async function fetchMyFavorites(
  page?: number,
): Promise<{ movies: MovieDto[]; pagination: PaginationMetadata | null }> {
  const qs = toQueryParams({ page } as QueryParams);
  const { data, pagination } = await apiGetPaginated<MovieDto[]>(`/favorites${qs}`);
  const validated = validateMovieDto(data);
  return { movies: validated as MovieDto[], pagination };
}

// A 404 just means "not favorited" here, not a real error - so unlike the other
// actions in this file, any failure collapses to false rather than surfacing.
export async function isFavorited(movieId: string): Promise<boolean> {
  try {
    await apiGet(`/favorites/${movieId}`);
    return true;
  } catch {
    return false;
  }
}

export async function addFavorite(movieId: string): Promise<ActionResult> {
  try {
    await apiPost(`/favorites/${movieId}`);
    revalidatePath("/user");
    return { success: true };
  } catch (e) {
    console.error("Error adding favorite:", e);
    return {
      success: false,
      error: e instanceof Error ? e.message : "Failed to add favorite",
    };
  }
}

export async function removeFavorite(movieId: string): Promise<ActionResult> {
  try {
    await apiDelete(`/favorites/${movieId}`);
    revalidatePath("/user");
    return { success: true };
  } catch (e) {
    console.error("Error removing favorite:", e);
    return {
      success: false,
      error: e instanceof Error ? e.message : "Failed to remove favorite",
    };
  }
}
