import { describe, expect, it } from "vitest";
import { render, screen } from "@testing-library/react";
import { NotFoundCard } from "./notFoundCard";

describe("NotFoundCard", () => {
  it("renders the heading, message and link", () => {
    render(
      <NotFoundCard
        heading="Movie not found"
        message="We couldn't find the movie you're looking for."
        linkHref="/movies"
        linkLabel="Browse movies"
      />,
    );

    expect(
      screen.getByRole("heading", { name: "Movie not found" }),
    ).toBeInTheDocument();
    expect(
      screen.getByText("We couldn't find the movie you're looking for."),
    ).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Browse movies" })).toHaveAttribute(
      "href",
      "/movies",
    );
  });
});
