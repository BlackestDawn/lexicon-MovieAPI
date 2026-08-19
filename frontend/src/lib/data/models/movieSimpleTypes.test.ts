import { describe, expect, it } from "vitest";
import { validateMovieSimpleDto } from "./movieSimpleTypes";

const validMovie = {
  id: "9c858901-8a57-4791-81fe-4c455b099bc9",
  createdAt: "2024-01-01T00:00:00.000Z",
  updatedAt: "2024-01-01T00:00:00.000Z",
  title: "Die Hard",
  releaseDate: "1988-07-15T00:00:00.000Z",
  runtimeMinutes: 132,
  averageRating: 8.2,
};

describe("validateMovieSimpleDto", () => {
  it("accepts a single valid item and coerces its dates", () => {
    const result = validateMovieSimpleDto(validMovie);
    expect(result).toMatchObject({ title: "Die Hard" });
    expect((result as { releaseDate: Date }).releaseDate).toBeInstanceOf(Date);
  });

  it("accepts an array of valid items", () => {
    const result = validateMovieSimpleDto([validMovie]) as unknown[];
    expect(result).toHaveLength(1);
  });

  it("throws on an invalid item", () => {
    expect(() => validateMovieSimpleDto({ id: "not-a-guid" })).toThrow(
      "invalid MovieSimpleDto item",
    );
  });
});
