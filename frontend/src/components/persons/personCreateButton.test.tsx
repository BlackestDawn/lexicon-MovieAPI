import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import PersonCreateButton from "./personCreateButton";

vi.mock("@/lib/actions/person", () => ({
  createPerson: vi.fn(),
  updatePerson: vi.fn(),
}));

vi.mock("@/lib/actions/movie", () => ({
  fetchMovies: vi.fn().mockResolvedValue({ movies: [], pagination: null }),
}));

describe("PersonCreateButton", () => {
  it("opens the create-person dialog on click", async () => {
    const user = userEvent.setup();
    render(<PersonCreateButton />);

    expect(
      screen.queryByRole("dialog", { name: "Create new Person" }),
    ).not.toBeInTheDocument();

    await user.click(screen.getByRole("button", { name: "Create new person" }));

    expect(
      screen.getByRole("dialog", { name: "Create new Person" }),
    ).toBeInTheDocument();
  });

  it("closes the dialog via the close button", async () => {
    const user = userEvent.setup();
    render(<PersonCreateButton />);

    await user.click(screen.getByRole("button", { name: "Create new person" }));
    await user.click(screen.getByRole("button", { name: "Close" }));

    expect(
      screen.queryByRole("dialog", { name: "Create new Person" }),
    ).not.toBeInTheDocument();
  });
});
