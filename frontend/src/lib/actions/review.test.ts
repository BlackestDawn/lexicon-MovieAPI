import { describe, expect, it, vi } from "vitest";
import { ValidationError } from "../data/interfaces/errors";

const { apiGet, apiGetPaginated, apiPost, apiPut, apiDelete } = vi.hoisted(
  () => ({
    apiGet: vi.fn(),
    apiGetPaginated: vi.fn(),
    apiPost: vi.fn(),
    apiPut: vi.fn(),
    apiDelete: vi.fn(),
  }),
);

vi.mock("./apiInteract", () => ({
  apiGet,
  apiGetPaginated,
  apiPost,
  apiPut,
  apiDelete,
}));

vi.mock("next/cache", () => ({
  revalidatePath: vi.fn(),
}));

const { createReview, updateReview, removeReview, fetchReviews, getReview } =
  await import("./review");

const movieId = "9c858901-8a57-4791-81fe-4c455b099bc0";

const review = {
  id: "9c858901-8a57-4791-81fe-4c455b099bc9",
  createdAt: "2024-01-01T00:00:00.000Z",
  updatedAt: "2024-01-01T00:00:00.000Z",
  authorName: "Alice",
  body: "Great movie",
  score: 9,
};

function formData(fields: Record<string, string>) {
  const data = new FormData();
  for (const [key, value] of Object.entries(fields)) data.set(key, value);
  return data;
}

describe("fetchReviews", () => {
  it("returns validated reviews and pagination from the API", async () => {
    apiGetPaginated.mockResolvedValue({
      data: [review],
      pagination: { TotalItemCount: 1, TotalPageCount: 1, PageSize: 10, CurrentPage: 1 },
    });

    const result = await fetchReviews(movieId, { minScore: 5 });

    expect(apiGetPaginated).toHaveBeenCalledWith(
      `/movies/${movieId}/reviews?minScore=5`,
    );
    expect(result.reviews).toHaveLength(1);
    expect(result.pagination?.TotalItemCount).toBe(1);
  });
});

describe("getReview", () => {
  it("fetches and validates a single review", async () => {
    apiGet.mockResolvedValue(review);

    const result = await getReview(movieId, review.id);

    expect(apiGet).toHaveBeenCalledWith(`/movies/${movieId}/reviews/${review.id}`);
    expect(result).toMatchObject({ authorName: "Alice", score: 9 });
  });
});

describe("createReview", () => {
  it("posts the parsed form data and reports success", async () => {
    apiPost.mockResolvedValue(review);

    const result = await createReview(
      movieId,
      formData({ body: "Great movie", score: "9" }),
    );

    expect(apiPost).toHaveBeenCalledWith(`/movies/${movieId}/reviews`, {
      body: "Great movie",
      score: 9,
    });
    expect(result.success).toBe(true);
  });

  it("fails fast on invalid form data without calling the API", async () => {
    const result = await createReview(
      movieId,
      formData({ body: "", score: "9" }),
    );

    expect(apiPost).not.toHaveBeenCalled();
    expect(result.success).toBe(false);
    expect(result.issues).toEqual(["Body is required"]);
  });

  it("fails fast when the score is out of range", async () => {
    const result = await createReview(
      movieId,
      formData({ body: "Great movie", score: "11" }),
    );

    expect(apiPost).not.toHaveBeenCalled();
    expect(result.success).toBe(false);
  });

  it("surfaces API validation issues", async () => {
    apiPost.mockRejectedValue(
      new ValidationError("Duplicate review", ["Duplicate review"]),
    );

    const result = await createReview(
      movieId,
      formData({ body: "Great movie", score: "9" }),
    );

    expect(result).toEqual({
      success: false,
      error: "Duplicate review",
      issues: ["Duplicate review"],
    });
  });
});

describe("updateReview", () => {
  it("puts the parsed form data and reports success", async () => {
    apiPut.mockResolvedValue(undefined);

    const result = await updateReview(
      movieId,
      review.id,
      formData({ body: "Updated", score: "7" }),
    );

    expect(apiPut).toHaveBeenCalledWith(
      `/movies/${movieId}/reviews/${review.id}`,
      { body: "Updated", score: 7 },
    );
    expect(result).toEqual({
      success: true,
      review: { body: "Updated", score: 7 },
    });
  });

  it("reports a generic error for a non-ValidationError failure", async () => {
    apiPut.mockRejectedValue(new Error("network down"));

    const result = await updateReview(
      movieId,
      review.id,
      formData({ body: "Updated", score: "7" }),
    );

    expect(result).toEqual({
      success: false,
      error: "network down",
      issues: null,
    });
  });
});

describe("removeReview", () => {
  it("deletes the review by movie and review id", async () => {
    apiDelete.mockResolvedValue(undefined);
    await removeReview(movieId, review.id);
    expect(apiDelete).toHaveBeenCalledWith(
      `/movies/${movieId}/reviews/${review.id}`,
    );
  });
});
