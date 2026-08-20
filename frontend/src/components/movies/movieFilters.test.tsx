import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import MovieFilters from "./movieFilters";

// Async Server Component (awaits fetchGenres) — direct-call pattern, see
// frontend_async_server_components memory / genreList.test.tsx.

const { fetchGenres } = vi.hoisted(() => ({ fetchGenres: vi.fn() }));
vi.mock("@/lib/actions/genre", () => ({ fetchGenres }));

const genres = [
  { id: "9c858901-8a57-4791-81fe-4c455b099bc1", name: "Action", slug: "action" },
];

async function renderFilters(
  props: {
    search?: string;
    genre?: string;
    year?: number;
    minRating?: number;
    maxRating?: number;
  } = {},
) {
  const jsx = await MovieFilters(props);
  return render(jsx);
}

describe("MovieFilters", () => {
  it("renders empty inputs, genre options, and no Clear link with no active filters", async () => {
    fetchGenres.mockResolvedValue(genres);
    await renderFilters();

    expect(screen.getByLabelText("Search")).toHaveValue("");
    expect(screen.getByLabelText("Genre")).toHaveValue("");
    expect(screen.getByLabelText("Release year")).toHaveValue(null);
    expect(screen.getByLabelText("Min rating")).toHaveValue(null);
    expect(screen.getByLabelText("Max rating")).toHaveValue(null);
    expect(screen.getByRole("option", { name: "Action" })).toBeInTheDocument();
    expect(screen.queryByRole("link", { name: "Clear" })).not.toBeInTheDocument();
  });

  it("prefills every field and shows a Clear link when filters are active", async () => {
    fetchGenres.mockResolvedValue(genres);
    await renderFilters({
      search: "hard",
      genre: "action",
      year: 1988,
      minRating: 7,
      maxRating: 9.5,
    });

    expect(screen.getByLabelText("Search")).toHaveValue("hard");
    expect(screen.getByLabelText("Genre")).toHaveValue("action");
    expect(screen.getByLabelText("Release year")).toHaveValue(1988);
    expect(screen.getByLabelText("Min rating")).toHaveValue(7);
    expect(screen.getByLabelText("Max rating")).toHaveValue(9.5);
    expect(screen.getByRole("link", { name: "Clear" })).toHaveAttribute(
      "href",
      "/movies",
    );
  });
});
