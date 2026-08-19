import { describe, expect, it } from "vitest";
import { render, screen } from "@testing-library/react";
import { DesktopNav } from "./desktopNav";
import menuData from "@/lib/data/consts/menuOptions.json";

describe("DesktopNav", () => {
  it("renders a link for every main menu entry", () => {
    render(<DesktopNav />);

    for (const link of menuData.main) {
      expect(screen.getByRole("link", { name: link.label })).toHaveAttribute(
        "href",
        link.href,
      );
    }
  });
});
