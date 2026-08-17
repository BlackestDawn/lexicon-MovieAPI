"use server";

import { revalidatePath } from "next/cache";
import { QueryParams } from "../data/interfaces/general";
import { ReviewSearchOptions } from "../data/interfaces/review";
import {
  ReviewDto,
  ReviewForChangeDto,
  validateReviewDto,
  validateReviewForChangeDto,
} from "../data/models/reviewTypes";
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

export async function fetchReviews(
  movieId: string,
  options?: ReviewSearchOptions,
): Promise<{ reviews: ReviewDto[]; pagination: PaginationMetadata | null }> {
  const qs = toQueryParams(options as QueryParams);
  const { data, pagination } = await apiGetPaginated(
    `/movies/${movieId}/reviews${qs}`,
  );
  const validated = validateReviewDto(data) as ReviewDto[];
  return { reviews: validated, pagination };
}

export async function getReview(
  movieId: string,
  id: string,
): Promise<ReviewDto> {
  const result = await apiGet(`/movies/${movieId}/reviews/${id}`);
  return validateReviewDto(result) as ReviewDto;
}

export async function createReview(movieId: string, formData: FormData) {
  try {
    const data = formToReviewChangeData(formData);

    const result = await apiPost(`/movies/${movieId}/reviews`, data);
    const validated = validateReviewDto(result);
    revalidatePath(`/movies/${movieId}`);
    return { success: true, review: validated };
  } catch (e) {
    console.error("Error creating review:", e);
    return {
      success: false,
      error: e instanceof Error ? e.message : "Review creation failed",
      issues: e instanceof ValidationError ? e.issues : null,
    };
  }
}

export async function updateReview(
  movieId: string,
  id: string,
  formData: FormData,
) {
  try {
    const data = formToReviewChangeData(formData);

    await apiPut(`/movies/${movieId}/reviews/${id}`, data);
    revalidatePath(`/movies/${movieId}`);
    revalidatePath(`/movies/${movieId}/reviews/${id}`);
    return { success: true, review: data };
  } catch (e) {
    console.error("Error updating review:", e);
    return {
      success: false,
      error: e instanceof Error ? e.message : "Review update failed",
      issues: e instanceof ValidationError ? e.issues : null,
    };
  }
}

export async function removeReview(movieId: string, id: string) {
  await apiDelete<void>(`/movies/${movieId}/reviews/${id}`);
  revalidatePath(`/movies/${movieId}`);
}

function formToReviewChangeData(data: FormData) {
  const parsed: ReviewForChangeDto = {
    authorName: data.get("authorName") as string,
    body: data.get("body") as string,
    score: Number(data.get("score")),
  };

  return validateReviewForChangeDto(parsed);
}
