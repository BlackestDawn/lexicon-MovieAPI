import { describe, expect, it } from "vitest";
import { render, screen } from "@testing-library/react";
import CookiePolicy from "./page";

describe("CookiePolicy page", () => {
  it("renders the cookie policy heading and repository link", () => {
    render(<CookiePolicy />);
    expect(screen.getByRole("heading", { name: "Cookie Policy" })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "project repository" })).toHaveAttribute(
      "href",
      "https://github.com/BlackestDawn/lexicon-MovieAPI",
    );
  });
});
