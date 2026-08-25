import { describe, expect, it } from "vitest";
import { render, screen } from "@testing-library/react";
import PersonNotFound from "./not-found";

describe("PersonNotFound page", () => {
  it("renders a person-specific not-found message linking to the persons list", () => {
    render(<PersonNotFound />);

    expect(
      screen.getByRole("heading", { name: "Person not found" }),
    ).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Browse persons" })).toHaveAttribute(
      "href",
      "/persons",
    );
  });
});
