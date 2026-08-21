import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import Page from "./page";

// RestrictedPage and ProfileSummaryCard each have their own dedicated tests
// (guard behavior and card rendering respectively), so here we only need to
// prove this page composes them together under a heading.
vi.mock("@/components/auth/restrictedPage", () => ({
  default: ({ children }: { children: React.ReactNode }) => (
    <div>restricted-page-stub{children}</div>
  ),
}));
vi.mock("@/components/user/profileSummaryCard", () => ({
  default: () => <div>profile-summary-card-stub</div>,
}));

describe("user Page", () => {
  it("renders the profile summary card behind the restricted-page guard", () => {
    render(<Page />);

    expect(screen.getByText("restricted-page-stub")).toBeInTheDocument();
    expect(
      screen.getByRole("heading", { name: "Your account" }),
    ).toBeInTheDocument();
    expect(screen.getByText("profile-summary-card-stub")).toBeInTheDocument();
  });
});
