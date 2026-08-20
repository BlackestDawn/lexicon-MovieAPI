import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import GenreList from "./genreList";
import CommonContext from "@/context/commonContext";
import { User } from "@/lib/data/models/userTypes";

// GenreList is an async Server Component. Vitest can't render an unresolved
// async component as JSX (React's client renderer has no RSC pipeline to
// unwrap it), so we call the exported function directly and await the
// element tree it returns, then hand that plain tree to RTL's render().
// See frontend_async_server_components memory for the full pattern.

const { fetchGenres } = vi.hoisted(() => ({ fetchGenres: vi.fn() }));
vi.mock("@/lib/actions/genre", () => ({
  fetchGenres,
  createGenre: vi.fn(),
  updateGenre: vi.fn(),
}));

const genres = [
  { id: "9c858901-8a57-4791-81fe-4c455b099bc1", name: "Action", slug: "action" },
  { id: "9c858901-8a57-4791-81fe-4c455b099bc2", name: "Comedy", slug: "comedy" },
];

const moderator: User = {
  id: "0c858901-8a57-4791-81fe-4c455b099bc9",
  name: "Mod",
  email: "mod@example.com",
  role: "Moderator",
};

async function renderGenreList(user: User | null) {
  const jsx = await GenreList();
  return render(<CommonContext initialUser={user}>{jsx}</CommonContext>);
}

describe("GenreList", () => {
  it("renders a card with a detail link for every fetched genre", async () => {
    fetchGenres.mockResolvedValue(genres);
    await renderGenreList(null);

    for (const g of genres) {
      expect(screen.getByRole("link", { name: g.name })).toHaveAttribute(
        "href",
        `/genres/${g.id}`,
      );
    }
  });

  it("hides the create button for a user without access", async () => {
    fetchGenres.mockResolvedValue(genres);
    await renderGenreList(null);
    expect(
      screen.queryByRole("button", { name: "Create new genre" }),
    ).not.toBeInTheDocument();
  });

  it("shows the create button for a moderator", async () => {
    fetchGenres.mockResolvedValue(genres);
    await renderGenreList(moderator);
    expect(
      screen.getByRole("button", { name: "Create new genre" }),
    ).toBeInTheDocument();
  });
});
