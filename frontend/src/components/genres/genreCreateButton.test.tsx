import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import GenreCreateButton from "./genreCreateButton";

vi.mock("@/lib/actions/genre", () => ({
  createGenre: vi.fn(),
  updateGenre: vi.fn(),
}));

describe("GenreCreateButton", () => {
  it("opens the create-genre dialog on click", async () => {
    const user = userEvent.setup();
    render(<GenreCreateButton />);

    expect(
      screen.queryByRole("dialog", { name: "Create new genre" }),
    ).not.toBeInTheDocument();

    await user.click(screen.getByRole("button", { name: "Create new genre" }));

    expect(
      screen.getByRole("dialog", { name: "Create new genre" }),
    ).toBeInTheDocument();
  });

  it("closes the dialog via the close button", async () => {
    const user = userEvent.setup();
    render(<GenreCreateButton />);

    await user.click(screen.getByRole("button", { name: "Create new genre" }));
    await user.click(screen.getByRole("button", { name: "Close" }));

    expect(
      screen.queryByRole("dialog", { name: "Create new genre" }),
    ).not.toBeInTheDocument();
  });
});
