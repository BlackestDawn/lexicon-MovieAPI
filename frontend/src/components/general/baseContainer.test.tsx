import { describe, expect, it } from "vitest";
import { render, screen } from "@testing-library/react";
import { BaseContainer } from "./baseContainer";

describe("BaseContainer", () => {
  it("renders its children", () => {
    render(<BaseContainer>content</BaseContainer>);
    expect(screen.getByText("content")).toBeInTheDocument();
  });

  it("appends a custom className alongside the base classes", () => {
    render(<BaseContainer className="py-8">content</BaseContainer>);
    const el = screen.getByText("content");
    expect(el.className).toContain("py-8");
    expect(el.className).toContain("max-w-5xl");
  });
});
