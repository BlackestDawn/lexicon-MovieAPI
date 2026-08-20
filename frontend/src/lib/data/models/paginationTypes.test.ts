import { describe, expect, it } from "vitest";
import { parsePaginationHeader } from "./paginationTypes";

describe("parsePaginationHeader", () => {
  it("returns null when the header is missing", () => {
    expect(parsePaginationHeader(null)).toBeNull();
    expect(parsePaginationHeader(undefined)).toBeNull();
  });

  it("parses a valid X-Pagination header", () => {
    const header = JSON.stringify({
      TotalItemCount: 42,
      TotalPageCount: 5,
      PageSize: 10,
      CurrentPage: 2,
    });

    expect(parsePaginationHeader(header)).toEqual({
      TotalItemCount: 42,
      TotalPageCount: 5,
      PageSize: 10,
      CurrentPage: 2,
    });
  });

  it("throws on a header that fails schema validation", () => {
    const header = JSON.stringify({ CurrentPage: 2 });
    expect(() => parsePaginationHeader(header)).toThrow(
      "invalid X-Pagination header",
    );
  });
});
