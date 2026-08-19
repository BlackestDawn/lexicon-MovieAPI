import { describe, expect, it } from "vitest";
import { render } from "@testing-library/react";
import LoadingSpinner from "./loadingSpinner";

describe("LoadingSpinner", () => {
  it("renders a spinning indicator", () => {
    const { container } = render(<LoadingSpinner />);
    expect(container.querySelector(".animate-spin")).toBeInTheDocument();
  });
});
