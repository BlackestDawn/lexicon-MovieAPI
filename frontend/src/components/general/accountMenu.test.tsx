import { describe, expect, it, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { AccountMenu } from "./accountMenu";
import CommonContext from "@/context/commonContext";
import { User } from "@/lib/data/models/userTypes";

const { logout } = vi.hoisted(() => ({ logout: vi.fn() }));

vi.mock("@/lib/actions/apiInteract", () => ({ logout }));
vi.mock("@/lib/actions/auth", () => ({
  loginRequest: vi.fn(),
  registerRequest: vi.fn(),
}));
vi.mock("../auth/loginForm", () => ({
  default: ({ onClose }: { onClose?: () => void }) => (
    <div>
      login-form
      <button onClick={onClose}>close-login-form</button>
    </div>
  ),
}));

const user: User = {
  id: "9c858901-8a57-4791-81fe-4c455b099bc9",
  name: "Alice",
  email: "alice@example.com",
  role: "User",
};

describe("AccountMenu", () => {
  it("shows a Login button and opens the login dialog when logged out", async () => {
    const testUser = userEvent.setup();
    render(
      <CommonContext initialUser={null}>
        <AccountMenu />
      </CommonContext>,
    );

    expect(screen.queryByText("login-form")).not.toBeInTheDocument();
    await testUser.click(screen.getByRole("button", { name: "Login" }));
    expect(screen.getByText("login-form")).toBeInTheDocument();
  });

  it("shows the user's name and a menu of account links when logged in", async () => {
    const testUser = userEvent.setup();
    render(
      <CommonContext initialUser={user}>
        <AccountMenu />
      </CommonContext>,
    );

    await testUser.click(screen.getByRole("button", { name: "Alice" }));

    expect(screen.getByRole("link", { name: "User page" })).toHaveAttribute(
      "href",
      "/user",
    );
    expect(screen.getByRole("button", { name: "Sign out" })).toBeInTheDocument();
  });

  it("signs out and clears the user on Sign out", async () => {
    logout.mockResolvedValue(undefined);
    const testUser = userEvent.setup();
    render(
      <CommonContext initialUser={user}>
        <AccountMenu />
      </CommonContext>,
    );

    await testUser.click(screen.getByRole("button", { name: "Alice" }));
    await testUser.click(screen.getByRole("button", { name: "Sign out" }));

    await waitFor(() => expect(logout).toHaveBeenCalled());
    await waitFor(() =>
      expect(screen.getByRole("button", { name: "Login" })).toBeInTheDocument(),
    );
  });
});
