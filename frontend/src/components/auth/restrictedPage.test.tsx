import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import RestrictedPage from "./restrictedPage";
import { User } from "@/lib/data/models/userTypes";

const { useAuth, usePathname } = vi.hoisted(() => ({
  useAuth: vi.fn(),
  usePathname: vi.fn(),
}));

vi.mock("@/context/commonContext", () => ({ useAuth }));
vi.mock("next/navigation", () => ({ usePathname }));

function mockAuth(overrides: Partial<ReturnType<typeof useAuth>> = {}) {
  useAuth.mockReturnValue({
    user: null,
    hasAccess: vi.fn().mockReturnValue(true),
    login: vi.fn(),
    register: vi.fn(),
    updateProfile: vi.fn(),
    logout: vi.fn(),
    ...overrides,
  });
}

const regularUser: User = {
  id: "9c858901-8a57-4791-81fe-4c455b099bc9",
  name: "Regular",
  email: "user@example.com",
  role: "User",
  createdAt: new Date("2024-01-01T00:00:00Z"),
};

describe("RestrictedPage", () => {
  it("shows a sign-in prompt linking back to the current page when there is no user", () => {
    mockAuth({ user: null });
    usePathname.mockReturnValue("/user/security");

    render(
      <RestrictedPage>
        <span>secret content</span>
      </RestrictedPage>,
    );

    expect(screen.getByText("Sign in required")).toBeInTheDocument();
    expect(screen.queryByText("secret content")).not.toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Sign in" })).toHaveAttribute(
      "href",
      "/login?redirectTo=%2Fuser%2Fsecurity",
    );
  });

  it("shows an access-denied notice when the user lacks the required access level", () => {
    mockAuth({ user: regularUser, hasAccess: vi.fn().mockReturnValue(false) });
    usePathname.mockReturnValue("/user/security");

    render(
      <RestrictedPage accessLevel="Administrator">
        <span>secret content</span>
      </RestrictedPage>,
    );

    expect(screen.getByText("Access denied")).toBeInTheDocument();
    expect(screen.queryByText("secret content")).not.toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Go back home" })).toHaveAttribute(
      "href",
      "/",
    );
  });

  it("renders the children once the user has the required access", () => {
    mockAuth({ user: regularUser, hasAccess: vi.fn().mockReturnValue(true) });
    usePathname.mockReturnValue("/user/security");

    render(
      <RestrictedPage>
        <span>secret content</span>
      </RestrictedPage>,
    );

    expect(screen.getByText("secret content")).toBeInTheDocument();
    expect(screen.queryByText("Sign in required")).not.toBeInTheDocument();
  });
});
