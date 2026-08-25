import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import Page from "./page";

// RestrictedPage and ChangePasswordForm each have their own dedicated tests
// (guard behavior and form behavior respectively), so here we only need to
// prove this page composes them together under a "Security" heading.
vi.mock("@/components/auth/restrictedPage", () => ({
  default: ({ children }: { children: React.ReactNode }) => (
    <div>restricted-page-stub{children}</div>
  ),
}));
vi.mock("@/components/auth/changePasswordForm", () => ({
  default: () => <div>change-password-form-stub</div>,
}));

describe("user/security Page", () => {
  it("renders the change-password form behind the restricted-page guard", () => {
    render(<Page />);

    expect(screen.getByText("restricted-page-stub")).toBeInTheDocument();
    expect(screen.getByRole("heading", { name: "Security" })).toBeInTheDocument();
    expect(screen.getByText("change-password-form-stub")).toBeInTheDocument();
  });
});
