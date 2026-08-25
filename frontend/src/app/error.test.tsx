import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import ErrorBoundary from "./error";

describe("Error boundary", () => {
  it("renders a generic message and logs the error", () => {
    const error = new Error("some raw server detail") as Error & {
      digest?: string;
    };
    const consoleError = vi.spyOn(console, "error").mockImplementation(() => {});

    render(<ErrorBoundary error={error} retry={vi.fn()} />);

    expect(
      screen.getByRole("heading", { name: "Something went wrong" }),
    ).toBeInTheDocument();
    expect(screen.queryByText("some raw server detail")).not.toBeInTheDocument();
    expect(consoleError).toHaveBeenCalledWith(error);

    consoleError.mockRestore();
  });

  it("shows the digest when present", () => {
    const error = new Error("boom") as Error & { digest?: string };
    error.digest = "abc123";
    vi.spyOn(console, "error").mockImplementation(() => {});

    render(<ErrorBoundary error={error} retry={vi.fn()} />);

    expect(screen.getByText("Reference: abc123")).toBeInTheDocument();
  });

  it("omits the reference line when there is no digest", () => {
    const error = new Error("boom") as Error & { digest?: string };
    vi.spyOn(console, "error").mockImplementation(() => {});

    render(<ErrorBoundary error={error} retry={vi.fn()} />);

    expect(screen.queryByText(/Reference:/)).not.toBeInTheDocument();
  });

  it("calls retry when the Try again button is clicked", async () => {
    const error = new Error("boom") as Error & { digest?: string };
    const retry = vi.fn();
    vi.spyOn(console, "error").mockImplementation(() => {});
    const user = userEvent.setup();

    render(<ErrorBoundary error={error} retry={retry} />);
    await user.click(screen.getByRole("button", { name: "Try again" }));

    expect(retry).toHaveBeenCalledOnce();
  });

  it("links back to the homepage", () => {
    const error = new Error("boom") as Error & { digest?: string };
    vi.spyOn(console, "error").mockImplementation(() => {});

    render(<ErrorBoundary error={error} retry={vi.fn()} />);

    expect(screen.getByRole("link", { name: "Go home" })).toHaveAttribute(
      "href",
      "/",
    );
  });
});
