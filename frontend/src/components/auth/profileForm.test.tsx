import { describe, expect, it, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import ProfileForm from "./profileForm";
import { ValidationError } from "@/lib/data/interfaces/errors";
import { User } from "@/lib/data/models/userTypes";

const { useAuth } = vi.hoisted(() => ({ useAuth: vi.fn() }));

vi.mock("@/context/commonContext", () => ({ useAuth }));

const currentUser: User = {
  id: "9c858901-8a57-4791-81fe-4c455b099bc9",
  name: "Alice",
  email: "alice@example.com",
  role: "User",
  createdAt: new Date("2024-01-01T00:00:00Z"),
};

function mockAuth(overrides: Partial<ReturnType<typeof useAuth>> = {}) {
  useAuth.mockReturnValue({
    user: currentUser,
    updateProfile: vi.fn(),
    hasAccess: vi.fn(),
    login: vi.fn(),
    register: vi.fn(),
    logout: vi.fn(),
    ...overrides,
  });
}

describe("ProfileForm", () => {
  it("renders nothing when there is no user", () => {
    mockAuth({ user: null });
    const { container } = render(<ProfileForm />);
    expect(container).toBeEmptyDOMElement();
  });

  it("prefills the email and display name from the current user", () => {
    mockAuth();
    render(<ProfileForm />);

    expect(screen.getByLabelText("Email")).toHaveValue("alice@example.com");
    expect(screen.getByLabelText("Display name")).toHaveValue("Alice");
  });

  it("saves the profile and shows a success message", async () => {
    const updateProfile = vi.fn().mockResolvedValue(undefined);
    mockAuth({ updateProfile });
    const user = userEvent.setup();

    render(<ProfileForm />);
    await user.clear(screen.getByLabelText("Display name"));
    await user.type(screen.getByLabelText("Display name"), "Alice Renamed");
    await user.click(screen.getByRole("button", { name: "Save changes" }));

    await waitFor(() =>
      expect(updateProfile).toHaveBeenCalledWith(
        "alice@example.com",
        "Alice Renamed",
      ),
    );
    expect(
      await screen.findByText("Your profile has been updated."),
    ).toBeInTheDocument();
  });

  it("shows returned issues when the update fails validation", async () => {
    const updateProfile = vi
      .fn()
      .mockRejectedValue(
        new ValidationError("Invalid profile details", [
          "A valid email is required",
        ]),
      );
    mockAuth({ updateProfile });
    const user = userEvent.setup();

    render(<ProfileForm />);
    await user.click(screen.getByRole("button", { name: "Save changes" }));

    expect(
      await screen.findByText("Invalid profile details"),
    ).toBeInTheDocument();
    expect(screen.getByText("A valid email is required")).toBeInTheDocument();
  });

  it("shows a generic error message when the update fails unexpectedly", async () => {
    const updateProfile = vi.fn().mockRejectedValue(new Error("Network error"));
    mockAuth({ updateProfile });
    const user = userEvent.setup();

    render(<ProfileForm />);
    await user.click(screen.getByRole("button", { name: "Save changes" }));

    expect(
      await screen.findByText("Profile update failed: Network error"),
    ).toBeInTheDocument();
  });
});
