import { describe, expect, it, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import SimpleDeleteButton from "./simpleDeleteButton";

const { push } = vi.hoisted(() => ({ push: vi.fn() }));

vi.mock("next/navigation", () => ({
  useRouter: () => ({ push }),
}));

describe("SimpleDeleteButton", () => {
  it("calls onDelete with the given id", async () => {
    const onDelete = vi.fn().mockResolvedValue(undefined);
    const user = userEvent.setup();

    render(<SimpleDeleteButton id="genre-1" onDelete={onDelete} />);
    await user.click(screen.getByRole("button"));

    await waitFor(() => expect(onDelete).toHaveBeenCalledWith("genre-1"));
    expect(push).not.toHaveBeenCalled();
  });

  it("redirects after a successful delete when redirectTo is set", async () => {
    const onDelete = vi.fn().mockResolvedValue(undefined);
    const user = userEvent.setup();

    render(
      <SimpleDeleteButton id="genre-1" redirectTo="/genres" onDelete={onDelete} />,
    );
    await user.click(screen.getByRole("button"));

    await waitFor(() => expect(push).toHaveBeenCalledWith("/genres"));
  });
});
