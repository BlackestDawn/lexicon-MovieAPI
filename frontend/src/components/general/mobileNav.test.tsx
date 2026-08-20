import { describe, expect, it } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import MobileNav from "./mobileNav";
import menuData from "@/lib/data/consts/menuOptions.json";

// Note: the menu links panel is always in the DOM and only hidden via
// opacity/pointer-events classes when closed (not conditional rendering), so
// these tests assert on aria-expanded and the backdrop rather than link
// presence for open/closed state.

describe("MobileNav", () => {
  it("renders a link for every main menu entry", () => {
    render(<MobileNav />);
    for (const link of menuData.main) {
      expect(screen.getByRole("link", { name: link.label })).toHaveAttribute(
        "href",
        link.href,
      );
    }
  });

  it("toggles aria-expanded and the backdrop when the hamburger is clicked", async () => {
    const user = userEvent.setup();
    const { container } = render(<MobileNav />);
    const toggleButton = screen.getByRole("button", { name: "Toggle menu" });

    expect(toggleButton).toHaveAttribute("aria-expanded", "false");
    expect(container.querySelector('div[aria-hidden="true"]')).not.toBeInTheDocument();

    await user.click(toggleButton);

    expect(toggleButton).toHaveAttribute("aria-expanded", "true");
    expect(container.querySelector('div[aria-hidden="true"]')).toBeInTheDocument();
  });

  it("closes when the backdrop is clicked", async () => {
    const user = userEvent.setup();
    const { container } = render(<MobileNav />);
    const toggleButton = screen.getByRole("button", { name: "Toggle menu" });

    await user.click(toggleButton);
    expect(toggleButton).toHaveAttribute("aria-expanded", "true");

    const backdrop = container.querySelector('div[aria-hidden="true"]') as Element;
    await user.click(backdrop);

    expect(toggleButton).toHaveAttribute("aria-expanded", "false");
  });

  it("closes when a menu link is clicked", async () => {
    const user = userEvent.setup();
    render(<MobileNav />);
    const toggleButton = screen.getByRole("button", { name: "Toggle menu" });

    await user.click(toggleButton);
    await user.click(screen.getByRole("link", { name: menuData.main[0].label }));

    expect(toggleButton).toHaveAttribute("aria-expanded", "false");
  });
});
