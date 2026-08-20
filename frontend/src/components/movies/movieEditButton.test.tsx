import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import MovieEditButton from "./movieEditButton";

vi.mock("@/lib/actions/movie", () => ({
  createMovie: vi.fn(),
  updateMovie: vi.fn(),
}));
vi.mock("@/lib/actions/genre", () => ({
  fetchGenres: vi.fn().mockResolvedValue([]),
}));
vi.mock("@/lib/actions/person", () => ({
  fetchPersons: vi.fn().mockResolvedValue({ persons: [], pagination: null }),
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
  castCrews: [],
  reviews: [],
  details: null,
};

describe("MovieEditButton", () => {
  it("opens the edit dialog prefilled with the movie's data", async () => {
    const user = userEvent.setup();
    render(<MovieEditButton movie={movie} />);

    await user.click(screen.getByRole("button"));

    const dialog = screen.getByRole("dialog", { name: "Edit movie" });
    expect(dialog).toBeInTheDocument();
    expect(screen.getByLabelText("Title")).toHaveValue("Die Hard");
    expect(screen.getByLabelText("Release date")).toHaveValue("1988-07-15");
    expect(screen.getByLabelText("Runtime minutes")).toHaveValue(132);
  });
});
