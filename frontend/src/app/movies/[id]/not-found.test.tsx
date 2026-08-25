import { describe, expect, it } from "vitest";
import { render, screen } from "@testing-library/react";
import MovieNotFound from "./not-found";

describe("MovieNotFound page", () => {
  it("renders a movie-specific not-found message linking to the movies list", () => {
    render(<MovieNotFound />);

    expect(
      screen.getByRole("heading", { name: "Movie not found" }),
    ).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Browse movies" })).toHaveAttribute(
      "href",
      "/movies",
    );
  });
});
