import { describe, expect, it, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import ReviewForm from "./reviewForm";

const { createReview, updateReview } = vi.hoisted(() => ({
  createReview: vi.fn(),
  updateReview: vi.fn(),
}));

vi.mock("@/lib/actions/review", () => ({ createReview, updateReview }));

const movieId = "9c858901-8a57-4791-81fe-4c455b099bc0";

const existingReview = {
  id: "9c858901-8a57-4791-81fe-4c455b099bc9",
  createdAt: new Date("2024-01-01"),
  updatedAt: new Date("2024-01-01"),
  authorName: "Alice",
  body: "Great movie",
  score: 9,
};

describe("ReviewForm", () => {
  it("creates a review and closes the dialog on success", async () => {
    createReview.mockResolvedValue({ success: true, review: {} });
    const onClose = vi.fn();
    const user = userEvent.setup();

    render(<ReviewForm onClose={onClose} movieId={movieId} />);

    await user.type(screen.getByLabelText("Review"), "Loved it");
    await user.type(screen.getByLabelText("Score"), "8");
    await user.click(screen.getByRole("button", { name: "Post review" }));

    await waitFor(() =>
      expect(createReview).toHaveBeenCalledWith(movieId, expect.any(FormData)),
    );
    await waitFor(() => expect(onClose).toHaveBeenCalled());
    expect(updateReview).not.toHaveBeenCalled();
  });

  it("shows returned issues and keeps the dialog open on failure", async () => {
    createReview.mockResolvedValue({
      success: false,
      error: "Invalid review",
      issues: ["Body is required"],
    });
    const onClose = vi.fn();
    const user = userEvent.setup();

    render(<ReviewForm onClose={onClose} movieId={movieId} />);

    await user.type(screen.getByLabelText("Score"), "8");
    await user.click(screen.getByRole("button", { name: "Post review" }));

    expect(await screen.findByText("Body is required")).toBeInTheDocument();
    expect(screen.getByText("Invalid review")).toBeInTheDocument();
    expect(onClose).not.toHaveBeenCalled();
  });

  it("prefills fields and calls updateReview when editing an existing review", async () => {
    updateReview.mockResolvedValue({ success: true, review: existingReview });
    const onClose = vi.fn();
    const user = userEvent.setup();

    render(
      <ReviewForm onClose={onClose} movieId={movieId} existingReview={existingReview} />,
    );

    expect(screen.getByLabelText("Review")).toHaveValue("Great movie");
    expect(screen.getByLabelText("Score")).toHaveValue(9);

    await user.click(screen.getByRole("button", { name: "Save changes" }));

    await waitFor(() =>
      expect(updateReview).toHaveBeenCalledWith(
        movieId,
        existingReview.id,
        expect.any(FormData),
      ),
    );
    expect(createReview).not.toHaveBeenCalled();
    await waitFor(() => expect(onClose).toHaveBeenCalled());
  });
});
