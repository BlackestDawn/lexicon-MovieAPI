import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import MovieList from "./movieList";
import CommonContext from "@/context/commonContext";
import { User } from "@/lib/data/models/userTypes";

// Async Server Component — direct-call pattern (see genreList.test.tsx).
// MovieFilters is itself async (it fetches genres), so it's stubbed here
// rather than resolved through, same as PersonFilters in personList.test.tsx;
// it has its own dedicated test in movieFilters.test.tsx.

const { fetchMovies, removeMovie } = vi.hoisted(() => ({
  fetchMovies: vi.fn(),
  removeMovie: vi.fn(),
}));
vi.mock("@/lib/actions/movie", () => ({ fetchMovies, removeMovie }));
vi.mock("next/navigation", () => ({ useRouter: () => ({ push: vi.fn() }) }));
vi.mock("./movieFilters", () => ({
  default: () => <div>movie-filters-stub</div>,
}));

const movie = {
  id: "9c858901-8a57-4791-81fe-4c455b099bc9",
  createdAt: new Date("2024-01-01"),
  updatedAt: new Date("2024-01-01"),
  title: "Die Hard",
  releaseDate: new Date("1988-07-15"),
  plotSummery: "A cop fights terrorists in a skyscraper.",
  runtimeMinutes: 132,
  averageRating: 8.2,
  genres: [],
};

const powerUser: User = {
  id: "0c858901-8a57-4791-81fe-4c455b099bc9",
  name: "Power",
  email: "power@example.com",
  role: "PowerUser",
};

async function renderMovieList(
  user: User | null,
  props: {
    page?: number;
    search?: string;
    genre?: string;
    year?: number;
    minRating?: number;
    maxRating?: number;
  } = {},
) {
  const jsx = await MovieList(props);
  return render(<CommonContext initialUser={user}>{jsx}</CommonContext>);
}

describe("MovieList", () => {
  it("renders a card with a detail link for every fetched movie", async () => {
    fetchMovies.mockResolvedValue({ movies: [movie], pagination: null });
    await renderMovieList(null);
    expect(screen.getByRole("link", { name: /Die Hard/ })).toHaveAttribute(
      "href",
      `/movies/${movie.id}`,
    );
  });

  it("renders the (stubbed) filters", async () => {
    fetchMovies.mockResolvedValue({ movies: [], pagination: null });
    await renderMovieList(null);
    expect(screen.getByText("movie-filters-stub")).toBeInTheDocument();
  });

  it("shows an empty state when no movies match", async () => {
    fetchMovies.mockResolvedValue({ movies: [], pagination: null });
    await renderMovieList(null);
    expect(
      screen.getByText("No movies found matching your filters."),
    ).toBeInTheDocument();
  });

  it("renders pagination controls carrying the active filters", async () => {
    fetchMovies.mockResolvedValue({
      movies: [movie],
      pagination: { TotalItemCount: 20, TotalPageCount: 2, PageSize: 10, CurrentPage: 1 },
    });
    await renderMovieList(null, { search: "hard" });

    expect(screen.getByText("Page 1 of 2")).toBeInTheDocument();
    const links = screen.getAllByRole("link");
    const nextLink = links.find((el) => el.getAttribute("href")?.includes("page=2"));
    expect(nextLink).toHaveAttribute("href", "/movies?search=hard&page=2");
  });

  it("hides the create button for a user without access", async () => {
    fetchMovies.mockResolvedValue({ movies: [], pagination: null });
    await renderMovieList(null);
    expect(
      screen.queryByRole("button", { name: "Create new movie" }),
    ).not.toBeInTheDocument();
  });

  it("shows the create button for a power user", async () => {
    fetchMovies.mockResolvedValue({ movies: [], pagination: null });
    await renderMovieList(powerUser);
    expect(
      screen.getByRole("button", { name: "Create new movie" }),
    ).toBeInTheDocument();
  });
});
