import { describe, expect, it } from "vitest";
import {
  validateMovieDto,
  validateMovieExtendedDto,
  validateMovieForChangeDto,
} from "./movieTypes";
import { PersonRole } from "./personRoleTypes";
import { ValidationError } from "../interfaces/errors";

const validGenre = {
  id: "9c858901-8a57-4791-81fe-4c455b099bc1",
  name: "Action",
  slug: "action",
};

const validMovie = {
  id: "9c858901-8a57-4791-81fe-4c455b099bc9",
  createdAt: "2024-01-01T00:00:00.000Z",
  updatedAt: "2024-01-01T00:00:00.000Z",
  title: "Die Hard",
  releaseDate: "1988-07-15T00:00:00.000Z",
  plotSummery: "A cop fights terrorists in a skyscraper.",
  runtimeMinutes: 132,
  averageRating: 8.2,
  genres: [validGenre],
};

describe("validateMovieDto", () => {
  it("accepts a single valid movie", () => {
    const result = validateMovieDto(validMovie);
    expect(result).toMatchObject({ title: "Die Hard" });
  });

  it("accepts an array of valid movies", () => {
    const result = validateMovieDto([validMovie]) as unknown[];
    expect(result).toHaveLength(1);
  });

  it("throws on an invalid item", () => {
    expect(() => validateMovieDto({ id: "not-a-guid" })).toThrow(
      "invalid MovieDto item",
    );
  });
});

describe("validateMovieExtendedDto", () => {
  const validExtended = {
    ...validMovie,
    castCrews: [
      {
        personId: "9c858901-8a57-4791-81fe-4c455b099bc8",
        givenName: "Bruce",
        lastName: "Willis",
        role: PersonRole.Cast,
      },
    ],
    reviews: [
      {
        id: "9c858901-8a57-4791-81fe-4c455b099bc7",
        createdAt: "2024-01-01T00:00:00.000Z",
        updatedAt: "2024-01-01T00:00:00.000Z",
        authorName: "Alice",
        body: "Great movie",
        score: 9,
      },
    ],
    details: { id: validMovie.id, synopsis: "Longer synopsis", language: "English", budget: 28000000 },
  };

  it("accepts a movie with cast, reviews and details", () => {
    const result = validateMovieExtendedDto(validExtended);
    expect(result.castCrews).toHaveLength(1);
    expect(result.reviews).toHaveLength(1);
  });

  it("accepts a movie without cast or details (nullable/optional)", () => {
    const { castCrews, details, ...rest } = validExtended;
    void castCrews;
    void details;
    const result = validateMovieExtendedDto(rest);
    expect(result.castCrews).toBeUndefined();
    expect(result.details).toBeUndefined();
  });

  it("throws when reviews is missing", () => {
    const { reviews, ...rest } = validExtended;
    void reviews;
    expect(() => validateMovieExtendedDto(rest)).toThrow(
      "invalid MovieExtendedDto item",
    );
  });
});

describe("validateMovieForChangeDto", () => {
  const validChange = {
    title: "Die Hard",
    releaseDate: "1988-07-15",
    plotSummery: "A cop fights terrorists in a skyscraper.",
    runtimeMinutes: 132,
    castCrews: [
      { personId: "9c858901-8a57-4791-81fe-4c455b099bc8", role: PersonRole.Cast },
    ],
    genres: ["9c858901-8a57-4791-81fe-4c455b099bc1"],
    synopsis: "Longer synopsis",
    language: "English",
    budget: 28000000,
  };

  it("accepts valid data and normalizes releaseDate to yyyy-MM-dd", () => {
    const result = validateMovieForChangeDto(validChange);
    expect(result.releaseDate).toBe("1988-07-15");
  });

  it("throws a ValidationError listing every missing/invalid field", () => {
    try {
      validateMovieForChangeDto({
        ...validChange,
        title: "",
        plotSummery: "",
        synopsis: "",
        language: "",
        genres: [],
        castCrews: [],
        runtimeMinutes: 0,
        budget: 0,
      });
      expect.unreachable("expected validateMovieForChangeDto to throw");
    } catch (e) {
      expect(e).toBeInstanceOf(ValidationError);
      const issues = (e as ValidationError).issues;
      expect(issues).toContain("Title is required");
      expect(issues).toContain("Plot summary is required");
      expect(issues).toContain("Synopsis is required");
      expect(issues).toContain("Language is required");
      expect(issues).toContain("Must have at least 1 genre");
      expect(issues).toContain("Must have at least 1 person for cast or crew");
      expect(issues).toContain("Runtime must be positive");
      expect(issues).toContain("Budget must be positive");
    }
  });

  it("rejects a release date far in the future", () => {
    const tooFar = new Date();
    tooFar.setFullYear(tooFar.getFullYear() + 20);

    expect(() =>
      validateMovieForChangeDto({
        ...validChange,
        releaseDate: tooFar.toISOString(),
      }),
    ).toThrow(ValidationError);
  });
});
