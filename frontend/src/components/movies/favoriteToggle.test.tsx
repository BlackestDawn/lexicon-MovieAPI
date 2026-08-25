import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import FavoriteToggle from "./favoriteToggle";

const { isFavorited } = vi.hoisted(() => ({ isFavorited: vi.fn() }));
vi.mock("@/lib/actions/favorite", () => ({ isFavorited, addFavorite: vi.fn(), removeFavorite: vi.fn() }));

describe("FavoriteToggle", () => {
  it("renders the button already toggled on when the movie is favorited", async () => {
    isFavorited.mockResolvedValue(true);

    const jsx = await FavoriteToggle({ movieId: "movie-1" });
    render(jsx);

    expect(isFavorited).toHaveBeenCalledWith("movie-1");
    expect(
      screen.getByRole("button", { name: "Remove from favorites" }),
    ).toBeInTheDocument();
  });

  it("renders the button off when the movie is not favorited", async () => {
    isFavorited.mockResolvedValue(false);

    const jsx = await FavoriteToggle({ movieId: "movie-1" });
    render(jsx);

    expect(
      screen.getByRole("button", { name: "Add to favorites" }),
    ).toBeInTheDocument();
  });
});
