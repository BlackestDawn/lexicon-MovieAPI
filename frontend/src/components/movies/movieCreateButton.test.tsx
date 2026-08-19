import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import MovieCreateButton from "./movieCreateButton";

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

describe("MovieCreateButton", () => {
  it("opens the create-movie dialog on click", async () => {
    const user = userEvent.setup();
    render(<MovieCreateButton />);

    expect(
      screen.queryByRole("dialog", { name: "Create new Movie" }),
    ).not.toBeInTheDocument();

    await user.click(screen.getByRole("button", { name: "Create new movie" }));

    expect(
      screen.getByRole("dialog", { name: "Create new Movie" }),
    ).toBeInTheDocument();
  });

  it("closes the dialog via the close button", async () => {
    const user = userEvent.setup();
    render(<MovieCreateButton />);

    await user.click(screen.getByRole("button", { name: "Create new movie" }));
    await user.click(screen.getByRole("button", { name: "Close" }));

    expect(
      screen.queryByRole("dialog", { name: "Create new Movie" }),
    ).not.toBeInTheDocument();
  });
});
