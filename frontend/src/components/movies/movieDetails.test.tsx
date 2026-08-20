import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import MovieDetails from "./movieDetails";
import CommonContext from "@/context/commonContext";
import { User } from "@/lib/data/models/userTypes";
import { PersonRole } from "@/lib/data/models/personRoleTypes";
import { NEXT_NOT_FOUND_MESSAGE } from "@/test-utils/nextNavigation";

// Async Server Component — direct-call pattern (see genreDetails.test.tsx).
// Its nested children (ReviewFilters, ReviewCreateButton, MovieEditButton,
// SimpleDeleteButton) are all sync or "use client", so no nested-async-module
// stubbing is needed here, unlike movieList.test.tsx.

const { getMovie, removeMovie, fetchReviews } = vi.hoisted(() => ({
  getMovie: vi.fn(),
  removeMovie: vi.fn(),
  fetchReviews: vi.fn(),
}));
vi.mock("@/lib/actions/movie", () => ({ getMovie, removeMovie }));
vi.mock("@/lib/actions/review", () => ({ fetchReviews, createReview: vi.fn(), updateReview: vi.fn() }));
vi.mock("next/navigation", () => ({
  useRouter: () => ({ push: vi.fn() }),
  notFound: vi.fn(() => {
    throw new Error("NEXT_NOT_FOUND");
  }),
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
  genres: [{ id: "9c858901-8a57-4791-81fe-4c455b099bc1", name: "Action", slug: "action" }],
  castCrews: [
    {
      personId: "9c858901-8a57-4791-81fe-4c455b099bc2",
      givenName: "Bruce",
      middleName: null,
      lastName: "Willis",
      role: PersonRole.Cast,
    },
  ],
  details: { id: "9c858901-8a57-4791-81fe-4c455b099bc9", synopsis: "Longer synopsis", language: "English", budget: 28000000 },
};

const review = {
  id: "9c858901-8a57-4791-81fe-4c455b099bc3",
  createdAt: new Date("2024-01-01"),
  updatedAt: new Date("2024-01-01"),
  authorName: "Alice",
  body: "Great movie",
  score: 9,
};

const powerUser: User = {
  id: "0c858901-8a57-4791-81fe-4c455b099bc9",
  name: "Power",
  email: "power@example.com",
  role: "PowerUser",
};

const moderator: User = {
  id: "1c858901-8a57-4791-81fe-4c455b099bc9",
  name: "Mod",
  email: "mod@example.com",
  role: "Moderator",
};

const loggedIn: User = {
  id: "2c858901-8a57-4791-81fe-4c455b099bc9",
  name: "Regular",
  email: "regular@example.com",
  role: "User",
};

async function renderMovieDetails(user: User | null) {
  const jsx = await MovieDetails({ id: movie.id });
  return render(<CommonContext initialUser={user}>{jsx}</CommonContext>);
}

describe("MovieDetails", () => {
  it("renders the movie's title, genres, and the details synopsis over the plot summary", async () => {
    getMovie.mockResolvedValue(movie);
    fetchReviews.mockResolvedValue({ reviews: [], pagination: null });
    await renderMovieDetails(null);

    expect(screen.getByRole("heading", { name: "Die Hard" })).toBeInTheDocument();
    expect(screen.getByText("Action")).toBeInTheDocument();
    expect(screen.getByText("Longer synopsis")).toBeInTheDocument();
    expect(screen.queryByText(movie.plotSummery)).not.toBeInTheDocument();
  });

  it("falls back to the plot summary when there are no details", async () => {
    getMovie.mockResolvedValue({ ...movie, details: null });
    fetchReviews.mockResolvedValue({ reviews: [], pagination: null });
    await renderMovieDetails(null);
    expect(screen.getByText(movie.plotSummery)).toBeInTheDocument();
  });

  it("renders cast and crew sorted by role, and its empty state", async () => {
    getMovie.mockResolvedValue(movie);
    fetchReviews.mockResolvedValue({ reviews: [], pagination: null });
    await renderMovieDetails(null);
    expect(screen.getByRole("link", { name: /Bruce Willis/ })).toHaveAttribute(
      "href",
      `/persons/${movie.castCrews[0].personId}`,
    );

    getMovie.mockResolvedValue({ ...movie, castCrews: [] });
    await renderMovieDetails(null);
    expect(screen.getByText("No cast or crew registered")).toBeInTheDocument();
  });

  it("renders reviews, pagination, and the empty-reviews state", async () => {
    getMovie.mockResolvedValue(movie);
    fetchReviews.mockResolvedValue({
      reviews: [review],
      pagination: { TotalItemCount: 20, TotalPageCount: 2, PageSize: 10, CurrentPage: 1 },
    });
    await renderMovieDetails(null);

    expect(screen.getByRole("link", { name: /Alice/ })).toHaveAttribute(
      "href",
      `/movies/${movie.id}/${review.id}`,
    );
    expect(screen.getByText("Page 1 of 2")).toBeInTheDocument();

    fetchReviews.mockResolvedValue({ reviews: [], pagination: null });
    await renderMovieDetails(null);
    expect(
      screen.getByText("No reviews found matching your filters."),
    ).toBeInTheDocument();
  });

  // ReviewFilters (accessible via its own "Search" submit button) always
  // renders regardless of auth, so it's the +1 baseline in every count below.
  // Access levels are cumulative, not exclusive — "LoggedIn" (the review
  // button) is satisfied by every authenticated role, and "PowerUserAndAbove"/
  // "ModeratorAndAbove" both admit a Moderator, so a Moderator sees every
  // control at once.

  it("hides edit/delete/review controls for a logged-out visitor, leaving only the review search", async () => {
    getMovie.mockResolvedValue(movie);
    fetchReviews.mockResolvedValue({ reviews: [], pagination: null });
    await renderMovieDetails(null);
    expect(screen.getAllByRole("button")).toHaveLength(1);
    expect(
      screen.queryByRole("button", { name: "Write a review" }),
    ).not.toBeInTheDocument();
  });

  it("shows the review button for a plain logged-in user, but not edit/delete", async () => {
    getMovie.mockResolvedValue(movie);
    fetchReviews.mockResolvedValue({ reviews: [], pagination: null });
    await renderMovieDetails(loggedIn);
    expect(screen.getByRole("button", { name: "Write a review" })).toBeInTheDocument();
    expect(screen.getAllByRole("button")).toHaveLength(2);
  });

  it("additionally shows the edit button for a power user, but not delete", async () => {
    getMovie.mockResolvedValue(movie);
    fetchReviews.mockResolvedValue({ reviews: [], pagination: null });
    await renderMovieDetails(powerUser);
    expect(screen.getAllByRole("button")).toHaveLength(3);
  });

  it("shows every control at once for a moderator", async () => {
    getMovie.mockResolvedValue(movie);
    fetchReviews.mockResolvedValue({ reviews: [], pagination: null });
    await renderMovieDetails(moderator);
    expect(screen.getAllByRole("button")).toHaveLength(4);
  });

  it("calls notFound() when the movie can't be found", async () => {
    getMovie.mockResolvedValue(null);
    await expect(MovieDetails({ id: "missing" })).rejects.toThrow(
      NEXT_NOT_FOUND_MESSAGE,
    );
  });
});
