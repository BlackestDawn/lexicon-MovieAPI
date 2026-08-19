import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import ReviewCreateButton from "./reviewCreateButton";

vi.mock("@/lib/actions/review", () => ({
  createReview: vi.fn(),
  updateReview: vi.fn(),
}));

describe("ReviewCreateButton", () => {
  it("opens the create-review dialog on click", async () => {
    const user = userEvent.setup();
    render(<ReviewCreateButton movieId="movie-1" />);

    expect(
      screen.queryByRole("dialog", { name: "Write a review" }),
    ).not.toBeInTheDocument();

    await user.click(screen.getByRole("button", { name: "Write a review" }));

    expect(
      screen.getByRole("dialog", { name: "Write a review" }),
    ).toBeInTheDocument();
  });

  it("closes the dialog via the close button", async () => {
    const user = userEvent.setup();
    render(<ReviewCreateButton movieId="movie-1" />);

    await user.click(screen.getByRole("button", { name: "Write a review" }));
    await user.click(screen.getByRole("button", { name: "Close" }));

    expect(
      screen.queryByRole("dialog", { name: "Write a review" }),
    ).not.toBeInTheDocument();
  });
});
