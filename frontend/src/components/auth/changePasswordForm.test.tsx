import { describe, expect, it, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import ChangePasswordForm from "./changePasswordForm";

const { changePasswordRequest } = vi.hoisted(() => ({
  changePasswordRequest: vi.fn(),
}));

vi.mock("@/lib/actions/auth", () => ({ changePasswordRequest }));

async function fillAndSubmit(
  user: ReturnType<typeof userEvent.setup>,
  { current = "Current1!", next = "Newpass1!", confirm = "Newpass1!" } = {},
) {
  await user.type(screen.getByLabelText("Current password"), current);
  await user.type(screen.getByLabelText("New password"), next);
  await user.type(screen.getByLabelText("Confirm new password"), confirm);
  await user.click(screen.getByRole("button", { name: "Change password" }));
}

describe("ChangePasswordForm", () => {
  it("blocks submission client-side when the confirmation does not match", async () => {
    const user = userEvent.setup();
    render(<ChangePasswordForm />);

    await fillAndSubmit(user, { confirm: "Different1!" });

    expect(
      await screen.findByText("New password and confirmation do not match"),
    ).toBeInTheDocument();
    expect(changePasswordRequest).not.toHaveBeenCalled();
  });

  it("submits the entered passwords and shows a success message", async () => {
    changePasswordRequest.mockResolvedValue({ success: true });
    const user = userEvent.setup();
    render(<ChangePasswordForm />);

    await fillAndSubmit(user);

    await waitFor(() => expect(changePasswordRequest).toHaveBeenCalledWith(expect.any(FormData)));
    const formData = changePasswordRequest.mock.calls[0][0] as FormData;
    expect(formData.get("currentPassword")).toBe("Current1!");
    expect(formData.get("newPassword")).toBe("Newpass1!");

    expect(
      await screen.findByText("Your password has been changed."),
    ).toBeInTheDocument();
    expect(screen.getByLabelText("Current password")).toHaveValue("");
    expect(screen.getByLabelText("New password")).toHaveValue("");
  });

  it("shows returned issues when the request fails", async () => {
    changePasswordRequest.mockResolvedValue({
      success: false,
      error: "Password change failed",
      issues: ["Incorrect password"],
    });
    const user = userEvent.setup();
    render(<ChangePasswordForm />);

    await fillAndSubmit(user);

    expect(await screen.findByText("Incorrect password")).toBeInTheDocument();
    expect(screen.getByText("Password change failed")).toBeInTheDocument();
    expect(
      screen.queryByText("Your password has been changed."),
    ).not.toBeInTheDocument();
  });
});
