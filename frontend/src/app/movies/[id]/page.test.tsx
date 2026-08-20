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

vi.mock("@/components/movies/movieDetails", () => ({
  default: (props: {
    id: string;
    page?: number;
    search?: string;
    minScore?: number;
    maxScore?: number;
  }) => (
    <div>
      movie-details-stub id={props.id} page={String(props.page)}
      search={String(props.search)} minScore={String(props.minScore)}
      maxScore={String(props.maxScore)}
    </div>
  ),
}));

describe("movies/[id] Page", () => {
  it("passes the route id and parsed searchParams through to MovieDetails", async () => {
    const jsx = await Page({
      params: Promise.resolve({ id: "movie-1" }),
      searchParams: Promise.resolve({ page: "2", search: "hard", minScore: "5", maxScore: "9" }),
    });
    render(jsx);

    expect(screen.getByText(/movie-details-stub/)).toBeInTheDocument();
    expect(screen.getByText(/id=movie-1/)).toBeInTheDocument();
    expect(screen.getByText(/page=2/)).toBeInTheDocument();
    expect(screen.getByText(/search=hard/)).toBeInTheDocument();
    expect(screen.getByText(/minScore=5/)).toBeInTheDocument();
    expect(screen.getByText(/maxScore=9/)).toBeInTheDocument();
  });

  it("leaves optional fields undefined with no query params", async () => {
    const jsx = await Page({
      params: Promise.resolve({ id: "movie-1" }),
      searchParams: Promise.resolve({}),
    });
    render(jsx);

    expect(screen.getByText(/page=undefined/)).toBeInTheDocument();
  });

  it("calls notFound() when the route id is missing", async () => {
    await expect(
      Page({ params: Promise.resolve({ id: "" }), searchParams: Promise.resolve({}) }),
    ).rejects.toThrow(NEXT_NOT_FOUND_MESSAGE);
  });
});
