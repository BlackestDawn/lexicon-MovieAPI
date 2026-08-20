import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import DialogBase from "./dialogBase";

describe("DialogBase", () => {
  it("renders its children", () => {
    render(
      <DialogBase onClose={vi.fn()}>
        <p>dialog body</p>
      </DialogBase>,
    );
    expect(screen.getByText("dialog body")).toBeInTheDocument();
  });

  it("calls onClose when the close button is clicked", async () => {
    const onClose = vi.fn();
    const user = userEvent.setup();
    render(
      <DialogBase onClose={onClose}>
        <p>dialog body</p>
      </DialogBase>,
    );

    await user.click(screen.getByRole("button", { name: "Close" }));
    expect(onClose).toHaveBeenCalledOnce();
  });

  it("calls onClose when clicking the backdrop", async () => {
    const onClose = vi.fn();
    const user = userEvent.setup();
    render(
      <DialogBase onClose={onClose}>
        <p>dialog body</p>
      </DialogBase>,
    );

    // The backdrop is the dialog's parent; the dialog itself stops propagation.
    const backdrop = screen.getByRole("dialog").parentElement;
    await user.click(backdrop as HTMLElement);
    expect(onClose).toHaveBeenCalledOnce();
  });

  it("does not call onClose when clicking inside the dialog content", async () => {
    const onClose = vi.fn();
    const user = userEvent.setup();
    render(
      <DialogBase onClose={onClose}>
        <p>dialog body</p>
      </DialogBase>,
    );

    await user.click(screen.getByText("dialog body"));
    expect(onClose).not.toHaveBeenCalled();
  });

  it("calls onClose when Escape is pressed", async () => {
    const onClose = vi.fn();
    const user = userEvent.setup();
    render(
      <DialogBase onClose={onClose}>
        <p>dialog body</p>
      </DialogBase>,
    );

    await user.keyboard("{Escape}");
    expect(onClose).toHaveBeenCalledOnce();
  });
});
