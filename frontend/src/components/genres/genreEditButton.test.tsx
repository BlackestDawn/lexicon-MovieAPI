import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import GenreEditButton from "./genreEditButton";

vi.mock("@/lib/actions/genre", () => ({
  createGenre: vi.fn(),
  updateGenre: vi.fn(),
}));

const genre = {
  id: "9c858901-8a57-4791-81fe-4c455b099bc9",
  name: "Action",
  slug: "action",
  movies: [],
};

describe("GenreEditButton", () => {
  it("opens the edit dialog prefilled with the genre's data", async () => {
    const user = userEvent.setup();
    render(<GenreEditButton genre={genre} />);

    await user.click(screen.getByRole("button"));

    const dialog = screen.getByRole("dialog", { name: "Edit genre" });
    expect(dialog).toBeInTheDocument();
    expect(screen.getByLabelText("Name")).toHaveValue("Action");
    expect(screen.getByLabelText("Slug")).toHaveValue("action");
  });
});
