import { describe, expect, it } from "vitest";
import {
  validateGenreDto,
  validateGenreExtendedDto,
  validateGenreForChangeDto,
} from "./genreTypes";
import { ValidationError } from "../interfaces/errors";

const validGenre = {
  id: "9c858901-8a57-4791-81fe-4c455b099bc9",
  name: "Action",
  slug: "action",
};

const validMovie = {
  id: "9c858901-8a57-4791-81fe-4c455b099bc8",
  createdAt: "2024-01-01T00:00:00.000Z",
  updatedAt: "2024-01-01T00:00:00.000Z",
  title: "Die Hard",
  releaseDate: "1988-07-15T00:00:00.000Z",
  runtimeMinutes: 132,
  averageRating: 8.2,
};

describe("validateGenreDto", () => {
  it("accepts a single valid genre", () => {
    expect(validateGenreDto(validGenre)).toEqual(validGenre);
  });

  it("accepts an array of valid genres", () => {
    expect(validateGenreDto([validGenre])).toEqual([validGenre]);
  });

  it("throws on an invalid single item", () => {
    expect(() => validateGenreDto({ id: "not-a-guid" })).toThrow(
      "invalid GenreDto item",
    );
  });

  it("throws when any item in an array is invalid", () => {
    expect(() => validateGenreDto([validGenre, { name: "bad" }])).toThrow(
      "invalid GenreDto item",
    );
  });
});

describe("validateGenreExtendedDto", () => {
  it("accepts a genre with movies", () => {
    const item = { ...validGenre, movies: [validMovie] };
    const result = validateGenreExtendedDto(item);
    expect(result.movies).toHaveLength(1);
  });

  it("throws when movies is missing", () => {
    expect(() => validateGenreExtendedDto(validGenre)).toThrow(
      "invalid GenreExtendedDto item",
    );
  });
});

describe("validateGenreForChangeDto", () => {
  it("accepts a valid name and slug", () => {
    const data = { name: "Action", slug: "action" };
    expect(validateGenreForChangeDto(data)).toEqual(data);
  });

  it("throws a ValidationError with per-field issues when fields are empty", () => {
    try {
      validateGenreForChangeDto({ name: "", slug: "" });
      expect.unreachable("expected validateGenreForChangeDto to throw");
    } catch (e) {
      expect(e).toBeInstanceOf(ValidationError);
      const issues = (e as ValidationError).issues;
      expect(issues).toContain("Name is required");
      expect(issues).toContain("Slug is required");
    }
  });
});
