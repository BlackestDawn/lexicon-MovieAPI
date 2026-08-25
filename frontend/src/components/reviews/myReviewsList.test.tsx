import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import MyReviewsList from "./myReviewsList";

const { fetchMyReviews } = vi.hoisted(() => ({ fetchMyReviews: vi.fn() }));
vi.mock("@/lib/actions/review", () => ({ fetchMyReviews }));

const reviews = [
  {
    id: "9c858901-8a57-4791-81fe-4c455b099bc1",
    createdAt: new Date("2024-01-01T00:00:00Z"),
    updatedAt: new Date("2024-01-01T00:00:00Z"),
    authorName: "Alice",
    body: "A gripping thriller from start to finish.",
    score: 8,
    userId: "9c858901-8a57-4791-81fe-4c455b099bc9",
    movieId: "9c858901-8a57-4791-81fe-4c455b099bc2",
    movieTitle: "Die Hard",
  },
];

async function renderList(page?: number) {
  const jsx = await MyReviewsList({ page });
  return render(jsx);
}

describe("MyReviewsList", () => {
  it("shows an empty state when there are no reviews", async () => {
    fetchMyReviews.mockResolvedValue({ reviews: [], pagination: null });

    await renderList();

    expect(
      screen.getByText("You haven't posted any reviews yet."),
    ).toBeInTheDocument();
  });

  it("renders each review linking to its movie/review detail page", async () => {
    fetchMyReviews.mockResolvedValue({ reviews, pagination: null });

    await renderList();

    expect(screen.getByText("Die Hard")).toBeInTheDocument();
    expect(screen.getByText("8/10")).toBeInTheDocument();
    expect(
      screen.getByText("A gripping thriller from start to finish."),
    ).toBeInTheDocument();
    expect(screen.getByRole("link")).toHaveAttribute(
      "href",
      `/movies/${reviews[0].movieId}/${reviews[0].id}`,
    );
  });

  it("passes the requested page through to fetchMyReviews", async () => {
    fetchMyReviews.mockResolvedValue({ reviews: [], pagination: null });

    await renderList(2);

    expect(fetchMyReviews).toHaveBeenCalledWith(2);
  });

  it("renders pagination controls when there is more than one page", async () => {
    fetchMyReviews.mockResolvedValue({
      reviews,
      pagination: { TotalItemCount: 30, PageSize: 10, CurrentPage: 1, TotalPageCount: 3 },
    });

    await renderList();

    expect(screen.getByText("Page 1 of 3")).toBeInTheDocument();
  });

  // Reviews and favorites can both be paginated on /user at once - a shared
  // "page" param would make paging one list silently reset the other.
  it("paginates under a reviewsPage param and preserves other lists' page state", async () => {
    fetchMyReviews.mockResolvedValue({
      reviews,
      pagination: { TotalItemCount: 30, PageSize: 10, CurrentPage: 1, TotalPageCount: 3 },
    });

    const jsx = await MyReviewsList({ page: 1, otherParams: { favoritesPage: 2 } });
    render(jsx);

    const links = screen
      .getAllByRole("link")
      .map((l) => l.getAttribute("href"))
      .filter((href): href is string => !!href?.startsWith("/user?"));
    expect(links).toContain("/user?favoritesPage=2&reviewsPage=2");
  });
});
