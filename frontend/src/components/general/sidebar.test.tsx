import { describe, expect, it } from "vitest";
import { render, screen } from "@testing-library/react";
import { Sidebar } from "./sidebar";
import menuData from "@/lib/data/consts/menuOptions.json";

describe("Sidebar", () => {
  it("links the brand name to the home page", () => {
    render(<Sidebar />);
    expect(screen.getByRole("link", { name: "MovieAPI" })).toHaveAttribute("href", "/");
  });

  it("renders the main navigation links", () => {
    render(<Sidebar />);
    for (const link of menuData.main) {
      expect(screen.getByRole("link", { name: link.label })).toHaveAttribute(
        "href",
        link.href,
      );
    }
  });
});
