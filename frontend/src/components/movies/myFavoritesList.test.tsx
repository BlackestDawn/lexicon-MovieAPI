import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import MyFavoritesList from "./myFavoritesList";

const { fetchMyFavorites } = vi.hoisted(() => ({ fetchMyFavorites: vi.fn() }));
vi.mock("@/lib/actions/favorite", () => ({
  fetchMyFavorites,
  addFavorite: vi.fn(),
  removeFavorite: vi.fn(),
}));

const movies = [
  {
    id: "9c858901-8a57-4791-81fe-4c455b099bc1",
    createdAt: new Date("2024-01-01"),
    updatedAt: new Date("2024-01-01"),
    title: "Die Hard",
    releaseDate: new Date("1988-07-15"),
    plotSummery: "A cop fights terrorists in a skyscraper.",
    runtimeMinutes: 132,
    averageRating: 8.2,
    genres: [{ id: "9c858901-8a57-4791-81fe-4c455b099bc2", name: "Action", slug: "action" }],
  },
];

async function renderList(page?: number) {
  const jsx = await MyFavoritesList({ page });
  return render(jsx);
}

describe("MyFavoritesList", () => {
  it("shows an empty state when there are no favorites", async () => {
    fetchMyFavorites.mockResolvedValue({ movies: [], pagination: null });

    await renderList();

    expect(
      screen.getByText("You haven't favorited any movies yet."),
    ).toBeInTheDocument();
  });

  it("renders a card with an already-favorited toggle for each movie", async () => {
    fetchMyFavorites.mockResolvedValue({ movies, pagination: null });

    await renderList();

    expect(screen.getByText("Die Hard", { exact: false })).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: "Remove from favorites" }),
    ).toBeInTheDocument();
    expect(screen.getByRole("link")).toHaveAttribute(
      "href",
      `/movies/${movies[0].id}`,
    );
  });

  it("passes the requested page through to fetchMyFavorites", async () => {
    fetchMyFavorites.mockResolvedValue({ movies: [], pagination: null });

    await renderList(2);

    expect(fetchMyFavorites).toHaveBeenCalledWith(2);
  });

  it("paginates under a favoritesPage param and preserves other lists' page state", async () => {
    fetchMyFavorites.mockResolvedValue({
      movies,
      pagination: { TotalItemCount: 30, PageSize: 10, CurrentPage: 1, TotalPageCount: 3 },
    });

    const jsx = await MyFavoritesList({ page: 1, otherParams: { reviewsPage: 2 } });
    render(jsx);

    const links = screen
      .getAllByRole("link")
      .map((l) => l.getAttribute("href"))
      .filter((href): href is string => !!href?.startsWith("/user?"));
    expect(links).toContain("/user?reviewsPage=2&favoritesPage=2");
  });
});
