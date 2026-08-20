import { describe, expect, it, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import GenreForm from "./genreForm";

const { createGenre, updateGenre } = vi.hoisted(() => ({
  createGenre: vi.fn(),
  updateGenre: vi.fn(),
}));

vi.mock("@/lib/actions/genre", () => ({ createGenre, updateGenre }));

const existingGenre = {
  id: "9c858901-8a57-4791-81fe-4c455b099bc9",
  name: "Action",
  slug: "action",
  movies: [],
};

describe("GenreForm", () => {
  it("creates a genre and closes the dialog on success", async () => {
    createGenre.mockResolvedValue({ success: true, genre: {} });
    const onClose = vi.fn();
    const user = userEvent.setup();

    render(<GenreForm onClose={onClose} />);

    await user.type(screen.getByLabelText("Name"), "Comedy");
    await user.type(screen.getByLabelText("Slug"), "comedy");
    await user.click(screen.getByRole("button", { name: "Create genre" }));

    await waitFor(() => expect(onClose).toHaveBeenCalled());
    expect(updateGenre).not.toHaveBeenCalled();
  });

  it("shows returned issues and keeps the dialog open on failure", async () => {
    createGenre.mockResolvedValue({
      success: false,
      error: "Invalid genre",
      issues: ["Slug is required"],
    });
    const onClose = vi.fn();
    const user = userEvent.setup();

    render(<GenreForm onClose={onClose} />);

    await user.type(screen.getByLabelText("Name"), "Comedy");
    await user.click(screen.getByRole("button", { name: "Create genre" }));

    expect(await screen.findByText("Slug is required")).toBeInTheDocument();
    expect(screen.getByText("Invalid genre")).toBeInTheDocument();
    expect(onClose).not.toHaveBeenCalled();
  });

  it("prefills fields and calls updateGenre when editing an existing genre", async () => {
    updateGenre.mockResolvedValue({ success: true, genre: existingGenre });
    const onClose = vi.fn();
    const user = userEvent.setup();

    render(<GenreForm onClose={onClose} existingGenre={existingGenre} />);

    expect(screen.getByLabelText("Name")).toHaveValue("Action");
    expect(screen.getByLabelText("Slug")).toHaveValue("action");

    await user.click(screen.getByRole("button", { name: "Save changes" }));

    await waitFor(() =>
      expect(updateGenre).toHaveBeenCalledWith(
        existingGenre.id,
        expect.any(FormData),
      ),
    );
    expect(createGenre).not.toHaveBeenCalled();
    await waitFor(() => expect(onClose).toHaveBeenCalled());
  });
});
