import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import Page from "./page";

// RestrictedPage and ProfileForm each have their own dedicated tests (guard
// behavior and form behavior respectively), so here we only need to prove
// this page composes them together under a "Settings" heading.
vi.mock("@/components/auth/restrictedPage", () => ({
  default: ({ children }: { children: React.ReactNode }) => (
    <div>restricted-page-stub{children}</div>
  ),
}));
vi.mock("@/components/auth/profileForm", () => ({
  default: () => <div>profile-form-stub</div>,
}));

describe("user/settings Page", () => {
  it("renders the profile form behind the restricted-page guard", () => {
    render(<Page />);

    expect(screen.getByText("restricted-page-stub")).toBeInTheDocument();
    expect(screen.getByRole("heading", { name: "Settings" })).toBeInTheDocument();
    expect(screen.getByText("profile-form-stub")).toBeInTheDocument();
  });
});
