import { describe, expect, it } from "vitest";
import { render, screen } from "@testing-library/react";
import Licensing from "./page";

describe("Licensing page", () => {
  it("renders the licensing heading and the CC BY 4.0 link", () => {
    render(<Licensing />);
    expect(screen.getByRole("heading", { name: "Licensing" })).toBeInTheDocument();
    expect(
      screen.getByRole("link", { name: "Creative Commons Attribution 4.0 International License" }),
    ).toHaveAttribute("href", "https://creativecommons.org/licenses/by/4.0/legalcode");
  });
});
