import { describe, expect, it } from "vitest";
import { render, screen } from "@testing-library/react";
import { SiteFooter } from "./siteFooter";

describe("SiteFooter", () => {
  it("shows the current year in the copyright notice", () => {
    render(<SiteFooter />);
    const year = new Date().getUTCFullYear();
    expect(screen.getByText(new RegExp(`© ${year}`))).toBeInTheDocument();
  });

  it("links to the licensing and cookie policy pages", () => {
    render(<SiteFooter />);
    expect(
      screen.getByRole("link", { name: "Creative commons Attribution 4.0 International" }),
    ).toHaveAttribute("href", "/licensing");
    expect(screen.getByRole("link", { name: "Cookie Policy" })).toHaveAttribute(
      "href",
      "/cookie-policy",
    );
  });
});
