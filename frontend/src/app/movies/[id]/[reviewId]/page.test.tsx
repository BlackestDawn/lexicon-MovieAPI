import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import Page from "./page";
import { NEXT_NOT_FOUND_MESSAGE } from "@/test-utils/nextNavigation";

// Inline throw — a vi.mock factory can't close over an externally-imported
// function (see test-utils/nextNavigation.ts).
vi.mock("next/navigation", () => ({
  notFound: vi.fn(() => {
    throw new Error("NEXT_NOT_FOUND");
  }),
}));

vi.mock("@/components/reviews/reviewDetails", () => ({
  default: (props: { movieId: string; id: string }) => (
    <div>review-details-stub movieId={props.movieId} id={props.id}</div>
  ),
}));

describe("movies/[id]/[reviewId] Page", () => {
  it("passes the route's movie id and review id through to ReviewDetails", async () => {
    const jsx = await Page({
      params: Promise.resolve({ id: "movie-1", reviewId: "review-1" }),
    });
    render(jsx);

    expect(screen.getByText(/review-details-stub/)).toBeInTheDocument();
    expect(screen.getByText(/movieId=movie-1/)).toBeInTheDocument();
    expect(screen.getByText(/id=review-1/)).toBeInTheDocument();
  });

  it("calls notFound() when the movie id is missing", async () => {
    await expect(
      Page({ params: Promise.resolve({ id: "", reviewId: "review-1" }) }),
    ).rejects.toThrow(NEXT_NOT_FOUND_MESSAGE);
  });

  it("calls notFound() when the review id is missing", async () => {
    await expect(
      Page({ params: Promise.resolve({ id: "movie-1", reviewId: "" }) }),
    ).rejects.toThrow(NEXT_NOT_FOUND_MESSAGE);
  });
});
