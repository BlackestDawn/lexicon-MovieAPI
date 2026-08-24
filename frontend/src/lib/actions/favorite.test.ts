import { describe, expect, it, vi } from "vitest";

const { apiGet, apiGetPaginated, apiPost, apiDelete } = vi.hoisted(() => ({
  apiGet: vi.fn(),
  apiGetPaginated: vi.fn(),
  apiPost: vi.fn(),
  apiDelete: vi.fn(),
}));

vi.mock("./apiInteract", () => ({ apiGet, apiGetPaginated, apiPost, apiDelete }));

vi.mock("next/cache", () => ({
  revalidatePath: vi.fn(),
}));

const { fetchMyFavorites, isFavorited, addFavorite, removeFavorite } =
  await import("./favorite");

const movieId = "9c858901-8a57-4791-81fe-4c455b099bc0";

const movie = {
  id: movieId,
  createdAt: "2024-01-01T00:00:00.000Z",
  updatedAt: "2024-01-01T00:00:00.000Z",
  title: "Die Hard",
  releaseDate: "1988-07-15T00:00:00.000Z",
  plotSummery: "A cop fights terrorists in a skyscraper.",
  runtimeMinutes: 132,
  averageRating: 8.2,
  genres: [],
};

describe("fetchMyFavorites", () => {
  it("returns validated favorites and pagination from the API", async () => {
    apiGetPaginated.mockResolvedValue({
      data: [movie],
      pagination: { TotalItemCount: 1, TotalPageCount: 1, PageSize: 10, CurrentPage: 1 },
    });

    const result = await fetchMyFavorites(2);

    expect(apiGetPaginated).toHaveBeenCalledWith("/favorites?page=2");
    expect(result.movies).toHaveLength(1);
    expect(result.pagination?.TotalItemCount).toBe(1);
  });
});

describe("isFavorited", () => {
  it("returns true when the API confirms the favorite exists", async () => {
    apiGet.mockResolvedValue(undefined);

    await expect(isFavorited(movieId)).resolves.toBe(true);
    expect(apiGet).toHaveBeenCalledWith(`/favorites/${movieId}`);
  });

  it("returns false when the API call fails (e.g. a 404)", async () => {
    apiGet.mockRejectedValue(new Error("API Error: 404"));

    await expect(isFavorited(movieId)).resolves.toBe(false);
  });
});

describe("addFavorite", () => {
  it("posts to the favorites endpoint and reports success", async () => {
    apiPost.mockResolvedValue(undefined);

    const result = await addFavorite(movieId);

    expect(apiPost).toHaveBeenCalledWith(`/favorites/${movieId}`);
    expect(result).toEqual({ success: true });
  });

  it("reports a failure when the API call throws", async () => {
    apiPost.mockRejectedValue(new Error("Movie not found"));

    const result = await addFavorite(movieId);

    expect(result).toEqual({ success: false, error: "Movie not found" });
  });
});

describe("removeFavorite", () => {
  it("deletes the favorite and reports success", async () => {
    apiDelete.mockResolvedValue(undefined);

    const result = await removeFavorite(movieId);

    expect(apiDelete).toHaveBeenCalledWith(`/favorites/${movieId}`);
    expect(result).toEqual({ success: true });
  });
});
