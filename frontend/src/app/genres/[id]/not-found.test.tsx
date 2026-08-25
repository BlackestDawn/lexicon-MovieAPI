import { describe, expect, it } from "vitest";
import { render, screen } from "@testing-library/react";
import GenreNotFound from "./not-found";

describe("GenreNotFound page", () => {
  it("renders a genre-specific not-found message linking to the genres list", () => {
    render(<GenreNotFound />);

    expect(
      screen.getByRole("heading", { name: "Genre not found" }),
    ).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Browse genres" })).toHaveAttribute(
      "href",
      "/genres",
    );
  });
});
