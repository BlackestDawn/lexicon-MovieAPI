import { describe, expect, it } from "vitest";
import { minsToDisplayRuntime, toQueryParams } from "./converters";

describe("toQueryParams", () => {
  it("returns an empty string when params are undefined", () => {
    expect(toQueryParams(undefined)).toBe("");
  });

  it("serializes primitive values", () => {
    expect(toQueryParams({ page: 2, search: "matrix" })).toBe(
      "?page=2&search=matrix",
    );
  });

  it("repeats the key for array values", () => {
    expect(toQueryParams({ genreIds: [1, 2, 3] })).toBe(
      "?genreIds=1&genreIds=2&genreIds=3",
    );
  });

  it("skips undefined and null values", () => {
    expect(toQueryParams({ page: undefined, search: null })).toBe("");
  });
});

describe("minsToDisplayRuntime", () => {
  it("formats minutes as hours and minutes", () => {
    expect(minsToDisplayRuntime(125)).toBe("2h 5m");
  });

  it("handles runtimes under an hour", () => {
    expect(minsToDisplayRuntime(45)).toBe("0h 45m");
  });
});
