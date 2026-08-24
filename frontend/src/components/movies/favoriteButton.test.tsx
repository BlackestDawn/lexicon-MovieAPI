import { describe, expect, it, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import FavoriteButton from "./favoriteButton";

const { addFavorite, removeFavorite } = vi.hoisted(() => ({
  addFavorite: vi.fn(),
  removeFavorite: vi.fn(),
}));

vi.mock("@/lib/actions/favorite", () => ({ addFavorite, removeFavorite }));

describe("FavoriteButton", () => {
  it("adds the movie to favorites when not yet favorited", async () => {
    addFavorite.mockResolvedValue({ success: true });
    const user = userEvent.setup();

    render(<FavoriteButton movieId="movie-1" initiallyFavorited={false} />);
    expect(
      screen.getByRole("button", { name: "Add to favorites" }),
    ).toBeInTheDocument();

    await user.click(screen.getByRole("button"));

    await waitFor(() => expect(addFavorite).toHaveBeenCalledWith("movie-1"));
    expect(removeFavorite).not.toHaveBeenCalled();
    expect(
      await screen.findByRole("button", { name: "Remove from favorites" }),
    ).toBeInTheDocument();
  });

  it("removes the movie from favorites when already favorited", async () => {
    removeFavorite.mockResolvedValue({ success: true });
    const user = userEvent.setup();

    render(<FavoriteButton movieId="movie-1" initiallyFavorited />);
    expect(
      screen.getByRole("button", { name: "Remove from favorites" }),
    ).toBeInTheDocument();

    await user.click(screen.getByRole("button"));

    await waitFor(() => expect(removeFavorite).toHaveBeenCalledWith("movie-1"));
    expect(addFavorite).not.toHaveBeenCalled();
    expect(
      await screen.findByRole("button", { name: "Add to favorites" }),
    ).toBeInTheDocument();
  });

  it("leaves the toggled state unchanged when the request fails", async () => {
    addFavorite.mockResolvedValue({ success: false, error: "Network error" });
    const user = userEvent.setup();

    render(<FavoriteButton movieId="movie-1" initiallyFavorited={false} />);
    await user.click(screen.getByRole("button"));

    await waitFor(() => expect(addFavorite).toHaveBeenCalled());
    expect(
      screen.getByRole("button", { name: "Add to favorites" }),
    ).toBeInTheDocument();
  });
});
