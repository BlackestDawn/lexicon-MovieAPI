import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import GenreDetails from "./genreDetails";
import CommonContext from "@/context/commonContext";
import { User } from "@/lib/data/models/userTypes";

// Async Server Component — see genreList.test.tsx / frontend_async_server_components
// memory for why we call the function directly instead of `render(<GenreDetails ... />)`.

const { getGenre, removeGenre } = vi.hoisted(() => ({
  getGenre: vi.fn(),
  removeGenre: vi.fn(),
}));
vi.mock("@/lib/actions/genre", () => ({ getGenre, removeGenre }));
vi.mock("next/navigation", () => ({ useRouter: () => ({ push: vi.fn() }) }));

const genre = {
  id: "9c858901-8a57-4791-81fe-4c455b099bc9",
  name: "Action",
  slug: "action",
  movies: [
    {
      id: "9c858901-8a57-4791-81fe-4c455b099bc1",
      title: "Die Hard",
      releaseDate: new Date("1988-07-15"),
      runtimeMinutes: 132,
      averageRating: 8.2,
    },
  ],
};

const admin: User = {
  id: "0c858901-8a57-4791-81fe-4c455b099bc9",
  name: "Admin",
  email: "admin@example.com",
  role: "Administrator",
};

async function renderGenreDetails(
  user: User | null,
  page?: number,
) {
  const jsx = await GenreDetails({ id: genre.id, page });
  return render(<CommonContext initialUser={user}>{jsx}</CommonContext>);
}

describe("GenreDetails", () => {
  it("renders the genre's movies with runtime and rating", async () => {
    getGenre.mockResolvedValue({ genre, pagination: null });
    await renderGenreDetails(null);

    expect(
      screen.getByRole("heading", { name: `Movies in ${genre.name}` }),
    ).toBeInTheDocument();
    expect(screen.getByText("Die Hard", { exact: false })).toBeInTheDocument();
    expect(screen.getByText("2h 12m")).toBeInTheDocument();
    expect(screen.getByText("8.2/10")).toBeInTheDocument();
  });

  it("shows an empty state when the genre has no movies", async () => {
    getGenre.mockResolvedValue({ genre: { ...genre, movies: [] }, pagination: null });
    await renderGenreDetails(null);
    expect(screen.getByText("No movies found")).toBeInTheDocument();
  });

  it("renders pagination controls when pagination is present", async () => {
    getGenre.mockResolvedValue({
      genre,
      pagination: { TotalItemCount: 20, TotalPageCount: 2, PageSize: 10, CurrentPage: 1 },
    });
    await renderGenreDetails(null, 1);
    expect(screen.getByText("Page 1 of 2")).toBeInTheDocument();
  });

  it("hides edit/delete controls for a user without access", async () => {
    getGenre.mockResolvedValue({ genre, pagination: null });
    await renderGenreDetails(null);
    expect(screen.queryByRole("button")).not.toBeInTheDocument();
  });

  it("shows edit/delete controls for an administrator", async () => {
    getGenre.mockResolvedValue({ genre, pagination: null });
    await renderGenreDetails(admin);
    expect(screen.getAllByRole("button")).toHaveLength(2);
  });
});
