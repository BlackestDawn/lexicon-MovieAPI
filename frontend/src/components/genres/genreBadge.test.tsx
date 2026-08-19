import { describe, expect, it } from "vitest";
import { render, screen } from "@testing-library/react";
import GenreBadge from "./genreBadge";

describe("GenreBadge", () => {
  it("renders the genre name", () => {
    render(<GenreBadge name="Action" />);
    expect(screen.getByText("Action")).toBeInTheDocument();
  });
});
