import { describe, expect, it } from "vitest";
import { render, screen } from "@testing-library/react";
import ReviewNotFound from "./not-found";

describe("ReviewNotFound page", () => {
  it("renders a review-specific not-found message linking to the movies list", () => {
    render(<ReviewNotFound />);

    expect(
      screen.getByRole("heading", { name: "Review not found" }),
    ).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Browse movies" })).toHaveAttribute(
      "href",
      "/movies",
    );
  });
});
