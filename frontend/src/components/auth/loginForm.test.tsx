import { describe, expect, it, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import LoginForm from "./loginForm";

const { useAuth, push, forgotPasswordRequest, resetPasswordRequest } = vi.hoisted(() => ({
  useAuth: vi.fn(),
  push: vi.fn(),
  forgotPasswordRequest: vi.fn(),
  resetPasswordRequest: vi.fn(),
}));

vi.mock("@/context/commonContext", () => ({ useAuth }));
vi.mock("next/navigation", () => ({ useRouter: () => ({ push }) }));
vi.mock("@/lib/actions/auth", () => ({ forgotPasswordRequest, resetPasswordRequest }));

function mockAuth(overrides: Partial<ReturnType<typeof useAuth>> = {}) {
  useAuth.mockReturnValue({
    user: null,
    login: vi.fn(),
    register: vi.fn(),
    logout: vi.fn(),
    hasAccess: vi.fn(),
    ...overrides,
  });
}

describe("LoginForm", () => {
  it("renders the sign-in form by default", () => {
    mockAuth();
    render(<LoginForm />);
    expect(screen.getByRole("heading", { name: "Sign in" })).toBeInTheDocument();
    expect(screen.getByLabelText("Email")).toBeInTheDocument();
    expect(screen.getByLabelText("Password")).toBeInTheDocument();
  });

  it("logs in and calls onClose on success", async () => {
    const login = vi.fn().mockResolvedValue(undefined);
    mockAuth({ login });
    const onClose = vi.fn();
    const user = userEvent.setup();

    render(<LoginForm onClose={onClose} />);

    await user.type(screen.getByLabelText("Email"), "alice@example.com");
    await user.type(screen.getByLabelText("Password"), "Abcdef1!");
    await user.click(screen.getByRole("button", { name: "Sign in" }));

    await waitFor(() =>
      expect(login).toHaveBeenCalledWith("alice@example.com", "Abcdef1!"),
    );
    await waitFor(() => expect(onClose).toHaveBeenCalled());
  });

  it("shows an error message when login fails", async () => {
    const login = vi.fn().mockRejectedValue(new Error("Invalid credentials"));
    mockAuth({ login });
    const onClose = vi.fn();
    const user = userEvent.setup();

    render(<LoginForm onClose={onClose} />);

    await user.type(screen.getByLabelText("Email"), "alice@example.com");
    await user.type(screen.getByLabelText("Password"), "wrong");
    await user.click(screen.getByRole("button", { name: "Sign in" }));

    expect(
      await screen.findByText("Login failed: Invalid credentials"),
    ).toBeInTheDocument();
    expect(onClose).not.toHaveBeenCalled();
  });

  it("switches to register mode and shows the display name field", async () => {
    mockAuth();
    const user = userEvent.setup();
    render(<LoginForm />);

    await user.click(screen.getByRole("button", { name: "Register" }));

    expect(screen.getByRole("heading", { name: "Create account" })).toBeInTheDocument();
    expect(screen.getByLabelText("Display name")).toBeInTheDocument();
  });

  it("registers with the entered details", async () => {
    const register = vi.fn().mockResolvedValue(undefined);
    mockAuth({ register });
    const onClose = vi.fn();
    const user = userEvent.setup();

    render(<LoginForm onClose={onClose} />);
    await user.click(screen.getByRole("button", { name: "Register" }));

    await user.type(screen.getByLabelText("Display name"), "Alice");
    await user.type(screen.getByLabelText("Email"), "alice@example.com");
    await user.type(screen.getByLabelText("Password"), "Abcdef1!");
    await user.click(screen.getByRole("button", { name: "Create account" }));

    await waitFor(() =>
      expect(register).toHaveBeenCalledWith("alice@example.com", "Abcdef1!", "Alice"),
    );
    await waitFor(() => expect(onClose).toHaveBeenCalled());
  });

  it("walks through the forgot-password request and reset steps", async () => {
    mockAuth();
    forgotPasswordRequest.mockResolvedValue({ success: true });
    resetPasswordRequest.mockResolvedValue({ success: true });
    const user = userEvent.setup();

    render(<LoginForm />);
    await user.click(screen.getByRole("button", { name: "Reset it" }));
    expect(screen.getByRole("heading", { name: "Reset your password" })).toBeInTheDocument();

    await user.type(screen.getByLabelText("Email"), "alice@example.com");
    await user.click(screen.getByRole("button", { name: "Send reset instructions" }));

    expect(
      await screen.findByText(/we've sent password reset instructions/),
    ).toBeInTheDocument();
    expect(forgotPasswordRequest).toHaveBeenCalledWith({ email: "alice@example.com" });
    expect(
      screen.getByRole("heading", { name: "Choose a new password" }),
    ).toBeInTheDocument();

    await user.type(screen.getByLabelText("Reset code"), "123456");
    await user.type(screen.getByLabelText("New password"), "Newpass1!");
    await user.click(screen.getByRole("button", { name: "Reset password" }));

    await waitFor(() =>
      expect(resetPasswordRequest).toHaveBeenCalledWith({
        email: "alice@example.com",
        token: "123456",
        newPassword: "Newpass1!",
      }),
    );
    expect(
      await screen.findByRole("heading", { name: "Sign in" }),
    ).toBeInTheDocument();
  });

  it("shows returned issues when the forgot-password request fails validation", async () => {
    mockAuth();
    forgotPasswordRequest.mockResolvedValue({
      success: false,
      error: "Invalid request",
      issues: ["A valid email is required"],
    });
    const user = userEvent.setup();

    render(<LoginForm />);
    await user.click(screen.getByRole("button", { name: "Reset it" }));
    await user.type(screen.getByLabelText("Email"), "alice@example.com");
    await user.click(screen.getByRole("button", { name: "Send reset instructions" }));

    expect(await screen.findByText("Invalid request")).toBeInTheDocument();
    expect(screen.getByText("A valid email is required")).toBeInTheDocument();
  });

  it("redirects once the user is set when opened from a page with fromPage", async () => {
    mockAuth({
      user: {
        id: "9c858901-8a57-4791-81fe-4c455b099bc9",
        name: "Alice",
        email: "alice@example.com",
        role: "User",
      },
    });

    render(<LoginForm fromPage redirectTo="/movies" />);

    await waitFor(() => expect(push).toHaveBeenCalledWith("/movies"));
  });
});
