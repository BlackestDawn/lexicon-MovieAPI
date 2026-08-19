import { describe, expect, it, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import CommonContext, { useAuth } from "./commonContext";
import { ValidationError } from "@/lib/data/interfaces/errors";
import { User } from "@/lib/data/models/userTypes";

const { loginRequest, registerRequest, logoutRequest } = vi.hoisted(() => ({
  loginRequest: vi.fn(),
  registerRequest: vi.fn(),
  logoutRequest: vi.fn(),
}));

vi.mock("@/lib/actions/auth", () => ({ loginRequest, registerRequest }));
vi.mock("@/lib/actions/apiInteract", () => ({ logout: logoutRequest }));

const moderator: User = {
  id: "9c858901-8a57-4791-81fe-4c455b099bc9",
  name: "Mod",
  email: "mod@example.com",
  role: "Moderator",
};

function Harness() {
  const { user, hasAccess, login, register, logout } = useAuth();
  return (
    <div>
      <p>user: {user ? user.name : "none"}</p>
      <p>LoggedIn: {String(hasAccess("LoggedIn"))}</p>
      <p>PowerUserAndAbove: {String(hasAccess("PowerUserAndAbove"))}</p>
      <p>Administrator: {String(hasAccess("Administrator"))}</p>
      <button onClick={() => login("a@example.com", "Abcdef1!")}>login</button>
      <button onClick={() => register("a@example.com", "Abcdef1!", "Alice")}>register</button>
      <button onClick={() => logout()}>logout</button>
    </div>
  );
}

describe("useAuth", () => {
  it("throws when used outside a CommonContext provider", () => {
    const RawHarness = () => {
      useAuth();
      return null;
    };
    expect(() => render(<RawHarness />)).toThrow(
      "useAuth must be used within an AuthProvider",
    );
  });

  it("hasAccess returns false for every level with no user", () => {
    render(
      <CommonContext initialUser={null}>
        <Harness />
      </CommonContext>,
    );
    expect(screen.getByText("LoggedIn: false")).toBeInTheDocument();
    expect(screen.getByText("PowerUserAndAbove: false")).toBeInTheDocument();
    expect(screen.getByText("Administrator: false")).toBeInTheDocument();
  });

  it("hasAccess ranks PowerUserAndAbove correctly and requires an exact match for a bare role", () => {
    render(
      <CommonContext initialUser={moderator}>
        <Harness />
      </CommonContext>,
    );
    expect(screen.getByText("LoggedIn: true")).toBeInTheDocument();
    expect(screen.getByText("PowerUserAndAbove: true")).toBeInTheDocument();
    expect(screen.getByText("Administrator: false")).toBeInTheDocument();
  });

  it("login sets the user returned by loginRequest", async () => {
    loginRequest.mockResolvedValue(moderator);
    const user = userEvent.setup();
    render(
      <CommonContext initialUser={null}>
        <Harness />
      </CommonContext>,
    );

    await user.click(screen.getByRole("button", { name: "login" }));
    await waitFor(() => expect(screen.getByText("user: Mod")).toBeInTheDocument());
  });

  it("register sets the user returned by registerRequest on success", async () => {
    registerRequest.mockResolvedValue({ success: true, user: moderator });
    const user = userEvent.setup();
    render(
      <CommonContext initialUser={null}>
        <Harness />
      </CommonContext>,
    );

    await user.click(screen.getByRole("button", { name: "register" }));
    await waitFor(() => expect(screen.getByText("user: Mod")).toBeInTheDocument());
  });

  it("register throws a ValidationError when the API returns field issues", async () => {
    registerRequest.mockResolvedValue({
      success: false,
      error: "Invalid registration",
      issues: ["A valid email is required"],
    });

    let caught: unknown;
    function ThrowingHarness() {
      const { register } = useAuth();
      return (
        <button
          onClick={() =>
            register("a@example.com", "Abcdef1!", "Alice").catch((e) => {
              caught = e;
            })
          }
        >
          register
        </button>
      );
    }

    const user = userEvent.setup();
    render(
      <CommonContext initialUser={null}>
        <ThrowingHarness />
      </CommonContext>,
    );
    await user.click(screen.getByRole("button", { name: "register" }));

    await waitFor(() => expect(caught).toBeInstanceOf(ValidationError));
    expect((caught as ValidationError).issues).toEqual(["A valid email is required"]);
  });

  it("logout calls the underlying request and clears the user", async () => {
    logoutRequest.mockResolvedValue(undefined);
    const user = userEvent.setup();
    render(
      <CommonContext initialUser={moderator}>
        <Harness />
      </CommonContext>,
    );

    expect(screen.getByText("user: Mod")).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "logout" }));

    await waitFor(() => expect(logoutRequest).toHaveBeenCalled());
    await waitFor(() => expect(screen.getByText("user: none")).toBeInTheDocument());
  });
});
