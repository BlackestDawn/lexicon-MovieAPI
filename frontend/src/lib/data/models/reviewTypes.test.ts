import { describe, expect, it } from "vitest";
import { validateReviewDto, validateReviewForChangeDto } from "./reviewTypes";
import { ValidationError } from "../interfaces/errors";

const validReview = {
  id: "9c858901-8a57-4791-81fe-4c455b099bc9",
  createdAt: "2024-01-01T00:00:00.000Z",
  updatedAt: "2024-01-01T00:00:00.000Z",
  authorName: "Alice",
  body: "Great movie",
  score: 9,
};

describe("validateReviewDto", () => {
  it("accepts a single valid review", () => {
    const result = validateReviewDto(validReview);
    expect(result).toMatchObject({ authorName: "Alice", score: 9 });
  });

  it("accepts an array of valid reviews", () => {
    const result = validateReviewDto([validReview]) as unknown[];
    expect(result).toHaveLength(1);
  });

  it("accepts a review without a userId (nullable/optional)", () => {
    const result = validateReviewDto(validReview);
    expect((result as { userId?: string }).userId).toBeUndefined();
  });

  it("throws on an invalid item", () => {
    expect(() => validateReviewDto({ id: "not-a-guid" })).toThrow(
      "invalid ReviewDto item",
    );
  });
});

describe("validateReviewForChangeDto", () => {
  it("accepts a valid body and score", () => {
    const data = { body: "Great movie", score: 8 };
    expect(validateReviewForChangeDto(data)).toEqual(data);
  });

  it("throws a ValidationError when body is empty", () => {
    try {
      validateReviewForChangeDto({ body: "", score: 8 });
      expect.unreachable("expected validateReviewForChangeDto to throw");
    } catch (e) {
      expect(e).toBeInstanceOf(ValidationError);
      expect((e as ValidationError).issues).toContain("Body is required");
    }
  });

  it("rejects a score of 0 or below", () => {
    expect(() => validateReviewForChangeDto({ body: "ok", score: 0 })).toThrow(
      ValidationError,
    );
  });

  it("rejects a score above 10", () => {
    expect(() => validateReviewForChangeDto({ body: "ok", score: 11 })).toThrow(
      ValidationError,
    );
  });

  it("rejects a non-integer score", () => {
    expect(() => validateReviewForChangeDto({ body: "ok", score: 7.5 })).toThrow(
      ValidationError,
    );
  });
});
