import { describe, expect, it, vi } from "vitest";
import { ValidationError } from "../data/interfaces/errors";
import { PersonRole } from "../data/models/personRoleTypes";

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

const { createMovie, updateMovie, removeMovie, fetchMovies, getMovie } =
  await import("./movie");

const movie = {
  id: "9c858901-8a57-4791-81fe-4c455b099bc9",
  createdAt: "2024-01-01T00:00:00.000Z",
  updatedAt: "2024-01-01T00:00:00.000Z",
  title: "Die Hard",
  releaseDate: "1988-07-15T00:00:00.000Z",
  plotSummery: "A cop fights terrorists in a skyscraper.",
  runtimeMinutes: 132,
  averageRating: 8.2,
  genres: [],
};

function formData(fields: Record<string, string | string[]>) {
  const data = new FormData();
  for (const [key, value] of Object.entries(fields)) {
    if (Array.isArray(value)) value.forEach((v) => data.append(key, v));
    else data.set(key, value);
  }
  return data;
}

function validChangeFields() {
  return {
    title: "Die Hard",
    releaseDate: "1988-07-15",
    plotSummery: "A cop fights terrorists in a skyscraper.",
    runtimeMinutes: "132",
    castCrewData: JSON.stringify([
      { personId: "9c858901-8a57-4791-81fe-4c455b099bc8", role: PersonRole.Cast },
    ]),
    genres: ["9c858901-8a57-4791-81fe-4c455b099bc1"],
    synopsis: "Longer synopsis",
    language: "English",
    budget: "28000000",
  };
}

describe("fetchMovies", () => {
  it("returns validated movies and pagination from the API", async () => {
    apiGetPaginated.mockResolvedValue({
      data: [movie],
      pagination: { TotalItemCount: 1, TotalPageCount: 1, PageSize: 10, CurrentPage: 1 },
    });

    const result = await fetchMovies({ search: "hard" });

    expect(apiGetPaginated).toHaveBeenCalledWith("/movies?search=hard");
    expect(result.movies).toHaveLength(1);
    expect(result.pagination?.TotalItemCount).toBe(1);
  });
});

describe("getMovie", () => {
  it("fetches and validates a single movie, requesting persons when asked", async () => {
    const extended = { ...movie, castCrews: [], reviews: [], details: null };
    apiGet.mockResolvedValue(extended);

    const result = await getMovie(movie.id, true);

    expect(apiGet).toHaveBeenCalledWith(`/movies/${movie.id}?includePersons=true`);
    expect(result.title).toBe("Die Hard");
  });
});

describe("createMovie", () => {
  it("posts the parsed form data and reports success", async () => {
    apiPost.mockResolvedValue(movie);

    const result = await createMovie(formData(validChangeFields()));

    expect(apiPost).toHaveBeenCalledWith("/movies", {
      title: "Die Hard",
      releaseDate: "1988-07-15",
      plotSummery: "A cop fights terrorists in a skyscraper.",
      runtimeMinutes: 132,
      castCrews: [{ personId: "9c858901-8a57-4791-81fe-4c455b099bc8", role: PersonRole.Cast }],
      genres: ["9c858901-8a57-4791-81fe-4c455b099bc1"],
      synopsis: "Longer synopsis",
      language: "English",
      budget: 28000000,
    });
    expect(result.success).toBe(true);
  });

  it("fails fast on invalid form data without calling the API", async () => {
    const result = await createMovie(
      formData({ ...validChangeFields(), title: "", genres: [] }),
    );

    expect(apiPost).not.toHaveBeenCalled();
    expect(result.success).toBe(false);
    expect(result.issues).toEqual(
      expect.arrayContaining(["Title is required", "Must have at least 1 genre"]),
    );
  });

  it("surfaces API validation issues", async () => {
    apiPost.mockRejectedValue(
      new ValidationError("Movie already exists", ["Movie already exists"]),
    );

    const result = await createMovie(formData(validChangeFields()));

    expect(result).toEqual({
      success: false,
      error: "Movie already exists",
      issues: ["Movie already exists"],
    });
  });
});

describe("updateMovie", () => {
  it("puts the parsed form data and reports success", async () => {
    apiPut.mockResolvedValue(undefined);

    const result = await updateMovie(movie.id, formData(validChangeFields()));

    expect(apiPut).toHaveBeenCalledWith(
      `/movies/${movie.id}`,
      expect.objectContaining({ title: "Die Hard" }),
    );
    expect(result.success).toBe(true);
  });

  it("reports a generic error for a non-ValidationError failure", async () => {
    apiPut.mockRejectedValue(new Error("network down"));

    const result = await updateMovie(movie.id, formData(validChangeFields()));

    expect(result).toEqual({
      success: false,
      error: "network down",
      issues: null,
    });
  });
});

describe("removeMovie", () => {
  it("deletes the movie by id", async () => {
    apiDelete.mockResolvedValue(undefined);
    await removeMovie(movie.id);
    expect(apiDelete).toHaveBeenCalledWith(`/movies/${movie.id}`);
  });
});
