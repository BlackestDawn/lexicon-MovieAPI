import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import ReviewDetails from "./reviewDetails";
import CommonContext from "@/context/commonContext";
import { User } from "@/lib/data/models/userTypes";
import { NEXT_NOT_FOUND_MESSAGE } from "@/test-utils/nextNavigation";

// Async Server Component — direct-call pattern (see genreDetails.test.tsx).

const { getReview, removeReview } = vi.hoisted(() => ({
  getReview: vi.fn(),
  removeReview: vi.fn(),
}));
vi.mock("@/lib/actions/review", () => ({ getReview, removeReview }));
vi.mock("next/navigation", () => ({
  useRouter: () => ({ push: vi.fn() }),
  notFound: vi.fn(() => {
    throw new Error("NEXT_NOT_FOUND");
  }),
}));

const movieId = "9c858901-8a57-4791-81fe-4c455b099bc0";
const authorId = "9c858901-8a57-4791-81fe-4c455b099bc1";

const review = {
  id: "9c858901-8a57-4791-81fe-4c455b099bc9",
  createdAt: new Date("2024-01-01"),
  updatedAt: new Date("2024-01-01"),
  authorName: "Alice",
  body: "Great movie",
  score: 9,
  userId: authorId,
};

const moderator: User = {
  id: "0c858901-8a57-4791-81fe-4c455b099bc9",
  name: "Mod",
  email: "mod@example.com",
  role: "Moderator",
};

const otherUser: User = {
  id: "1c858901-8a57-4791-81fe-4c455b099bc9",
  name: "Other",
  email: "other@example.com",
  role: "User",
};

const author: User = {
  id: authorId,
  name: "Alice",
  email: "alice@example.com",
  role: "User",
};

async function renderReviewDetails(user: User | null) {
  const jsx = await ReviewDetails({ movieId, id: review.id });
  return render(<CommonContext initialUser={user}>{jsx}</CommonContext>);
}

describe("ReviewDetails", () => {
  it("renders the author, score, and body", async () => {
    getReview.mockResolvedValue(review);
    await renderReviewDetails(null);

    expect(screen.getByRole("heading", { name: "Alice" })).toBeInTheDocument();
    expect(screen.getByText("9/10")).toBeInTheDocument();
    expect(screen.getByText("Great movie")).toBeInTheDocument();
    expect(screen.getByText(/Posted/)).toBeInTheDocument();
    expect(screen.queryByText(/updated/)).not.toBeInTheDocument();
  });

  it("shows an 'updated' suffix when the review was edited after creation", async () => {
    getReview.mockResolvedValue({
      ...review,
      updatedAt: new Date("2024-02-01"),
    });
    await renderReviewDetails(null);
    expect(screen.getByText(/updated/)).toBeInTheDocument();
  });

  it("hides edit/delete controls for a user who isn't the author or a moderator", async () => {
    getReview.mockResolvedValue(review);
    await renderReviewDetails(otherUser);
    expect(screen.queryByRole("button")).not.toBeInTheDocument();
  });

  it("shows edit/delete controls for a moderator, even when not the author", async () => {
    getReview.mockResolvedValue(review);
    await renderReviewDetails(moderator);
    expect(screen.getAllByRole("button")).toHaveLength(2);
  });

  it("shows edit/delete controls for the review's own author regardless of role", async () => {
    getReview.mockResolvedValue(review);
    await renderReviewDetails(author);
    expect(screen.getAllByRole("button")).toHaveLength(2);
  });

  it("calls notFound() when the review can't be found", async () => {
    getReview.mockResolvedValue(null);
    await expect(ReviewDetails({ movieId, id: "missing" })).rejects.toThrow(
      NEXT_NOT_FOUND_MESSAGE,
    );
  });
});
