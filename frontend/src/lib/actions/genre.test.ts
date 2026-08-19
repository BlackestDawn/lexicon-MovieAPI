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

const { createGenre, updateGenre, removeGenre, fetchGenres, getGenre } =
  await import("./genre");

const genre = {
  id: "9c858901-8a57-4791-81fe-4c455b099bc9",
  name: "Action",
  slug: "action",
};

function formData(fields: Record<string, string>) {
  const data = new FormData();
  for (const [key, value] of Object.entries(fields)) data.set(key, value);
  return data;
}

describe("fetchGenres", () => {
  it("returns validated genres from the API", async () => {
    apiGet.mockResolvedValue([genre]);
    await expect(fetchGenres()).resolves.toEqual([genre]);
    expect(apiGet).toHaveBeenCalledWith("/genres");
  });
});

describe("getGenre", () => {
  it("fetches and validates a single genre with its pagination", async () => {
    const extended = { ...genre, movies: [] };
    apiGetPaginated.mockResolvedValue({
      data: extended,
      pagination: { TotalItemCount: 0, TotalPageCount: 1, PageSize: 10, CurrentPage: 1 },
    });

    const result = await getGenre(genre.id, { page: 2 });

    expect(apiGetPaginated).toHaveBeenCalledWith(`/genres/${genre.id}?page=2`);
    expect(result.genre).toEqual(extended);
    expect(result.pagination?.CurrentPage).toBe(1);
  });
});

describe("createGenre", () => {
  it("posts the form data and reports success", async () => {
    apiPost.mockResolvedValue(genre);

    const result = await createGenre(formData({ name: "Action", slug: "action" }));

    expect(apiPost).toHaveBeenCalledWith("/genres", {
      name: "Action",
      slug: "action",
    });
    expect(result).toEqual({ success: true, genre });
  });

  it("fails fast on invalid form data without calling the API", async () => {
    const result = await createGenre(formData({ name: "", slug: "" }));

    expect(apiPost).not.toHaveBeenCalled();
    expect(result.success).toBe(false);
    expect(result.issues).toEqual(["Name is required", "Slug is required"]);
  });

  it("surfaces API validation issues", async () => {
    apiPost.mockRejectedValue(new ValidationError("Slug already in use", ["Slug already in use"]));

    const result = await createGenre(formData({ name: "Action", slug: "action" }));

    expect(result).toEqual({
      success: false,
      error: "Slug already in use",
      issues: ["Slug already in use"],
    });
  });
});

describe("updateGenre", () => {
  it("puts the form data and reports success", async () => {
    apiPut.mockResolvedValue(undefined);

    const result = await updateGenre(genre.id, formData({ name: "Action", slug: "action" }));

    expect(apiPut).toHaveBeenCalledWith(`/genres/${genre.id}`, {
      name: "Action",
      slug: "action",
    });
    expect(result).toEqual({
      success: true,
      genre: { name: "Action", slug: "action" },
    });
  });

  it("reports a generic error for a non-ValidationError failure", async () => {
    apiPut.mockRejectedValue(new Error("network down"));

    const result = await updateGenre(genre.id, formData({ name: "Action", slug: "action" }));

    expect(result).toEqual({
      success: false,
      error: "network down",
      issues: null,
    });
  });
});

describe("removeGenre", () => {
  it("deletes the genre by id", async () => {
    apiDelete.mockResolvedValue(undefined);
    await removeGenre(genre.id);
    expect(apiDelete).toHaveBeenCalledWith(`/genres/${genre.id}`);
  });
});
