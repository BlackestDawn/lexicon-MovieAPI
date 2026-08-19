import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import ReviewEditButton from "./reviewEditButton";

vi.mock("@/lib/actions/review", () => ({
  createReview: vi.fn(),
  updateReview: vi.fn(),
}));

const review = {
  id: "9c858901-8a57-4791-81fe-4c455b099bc9",
  createdAt: new Date("2024-01-01"),
  updatedAt: new Date("2024-01-01"),
  authorName: "Alice",
  body: "Great movie",
  score: 9,
};

describe("ReviewEditButton", () => {
  it("opens the edit dialog prefilled with the review's data", async () => {
    const user = userEvent.setup();
    render(<ReviewEditButton movieId="movie-1" review={review} />);

    await user.click(screen.getByRole("button"));

    const dialog = screen.getByRole("dialog", { name: "Edit review" });
    expect(dialog).toBeInTheDocument();
    expect(screen.getByLabelText("Review")).toHaveValue("Great movie");
    expect(screen.getByLabelText("Score")).toHaveValue(9);
  });
});
