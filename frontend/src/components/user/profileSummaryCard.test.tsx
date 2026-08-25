import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import ProfileSummaryCard from "./profileSummaryCard";
import { User } from "@/lib/data/models/userTypes";

const { useAuth } = vi.hoisted(() => ({ useAuth: vi.fn() }));

vi.mock("@/context/commonContext", () => ({ useAuth }));

const currentUser: User = {
  id: "9c858901-8a57-4791-81fe-4c455b099bc9",
  name: "Alice",
  email: "alice@example.com",
  role: "Moderator",
  createdAt: new Date("2024-01-01T00:00:00Z"),
};

function mockAuth(overrides: Partial<ReturnType<typeof useAuth>> = {}) {
  useAuth.mockReturnValue({
    user: currentUser,
    hasAccess: vi.fn(),
    login: vi.fn(),
    register: vi.fn(),
    updateProfile: vi.fn(),
    logout: vi.fn(),
    ...overrides,
  });
}

describe("ProfileSummaryCard", () => {
  it("renders nothing when there is no user", () => {
    mockAuth({ user: null });
    const { container } = render(<ProfileSummaryCard />);
    expect(container).toBeEmptyDOMElement();
  });

  it("renders the current user's name, email, and role", () => {
    mockAuth();
    render(<ProfileSummaryCard />);

    expect(screen.getByText("Alice")).toBeInTheDocument();
    expect(screen.getByText("alice@example.com")).toBeInTheDocument();
    expect(screen.getByText("Moderator")).toBeInTheDocument();
  });

  it("shows the member-since date", () => {
    mockAuth();
    render(<ProfileSummaryCard />);

    expect(
      screen.getByText(`Member since ${currentUser.createdAt.toLocaleDateString()}`),
    ).toBeInTheDocument();
  });
});
