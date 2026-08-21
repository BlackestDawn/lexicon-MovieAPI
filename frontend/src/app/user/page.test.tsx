import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import Page from "./page";

// RestrictedPage, ProfileSummaryCard, and MyReviewsList each have their own
// dedicated tests (guard behavior, card rendering, and list rendering
// respectively), so here we only need to prove this page composes them
// together under a heading.
vi.mock("@/components/auth/restrictedPage", () => ({
  default: ({ children }: { children: React.ReactNode }) => (
    <div>restricted-page-stub{children}</div>
  ),
}));
vi.mock("@/components/user/profileSummaryCard", () => ({
  default: () => <div>profile-summary-card-stub</div>,
}));
vi.mock("@/components/reviews/myReviewsList", () => ({
  default: () => <div>my-reviews-list-stub</div>,
}));

describe("user Page", () => {
  it("renders the profile summary card and review list behind the restricted-page guard", async () => {
    const jsx = await Page({ searchParams: Promise.resolve({}) });
    render(jsx);

    expect(screen.getByText("restricted-page-stub")).toBeInTheDocument();
    expect(
      screen.getByRole("heading", { name: "Your account" }),
    ).toBeInTheDocument();
    expect(screen.getByText("profile-summary-card-stub")).toBeInTheDocument();
    expect(screen.getByText("my-reviews-list-stub")).toBeInTheDocument();
  });
});
