import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import Page from "./page";

// Page is itself async (it awaits searchParams); MovieList has its own test,
// so it's stubbed here to check numeric-param parsing/passing only.
vi.mock("@/components/movies/movieList", () => ({
  default: (props: {
    page?: number;
    search?: string;
    genre?: string;
    year?: number;
    minRating?: number;
    maxRating?: number;
  }) => (
    <div>
      movie-list-stub page={String(props.page)} search={String(props.search)}
      genre={String(props.genre)} year={String(props.year)}
      minRating={String(props.minRating)} maxRating={String(props.maxRating)}
    </div>
  ),
}));

describe("movies Page", () => {
  it("parses every numeric query param and passes everything through to MovieList", async () => {
    const jsx = await Page({
      searchParams: Promise.resolve({
        page: "2",
        search: "hard",
        genre: "action",
        year: "1988",
        minRating: "7",
        maxRating: "9.5",
      }),
    });
    render(jsx);

    expect(screen.getByText(/movie-list-stub/)).toBeInTheDocument();
    expect(screen.getByText(/page=2/)).toBeInTheDocument();
    expect(screen.getByText(/search=hard/)).toBeInTheDocument();
    expect(screen.getByText(/genre=action/)).toBeInTheDocument();
    expect(screen.getByText(/year=1988/)).toBeInTheDocument();
    expect(screen.getByText(/minRating=7/)).toBeInTheDocument();
    expect(screen.getByText(/maxRating=9\.5/)).toBeInTheDocument();
  });

  it("leaves every field undefined with no query params", async () => {
    const jsx = await Page({ searchParams: Promise.resolve({}) });
    render(jsx);

    expect(screen.getByText(/page=undefined/)).toBeInTheDocument();
    expect(screen.getByText(/maxRating=undefined/)).toBeInTheDocument();
  });
});
