import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import Page from "./page";

// Page itself is sync — it just wires GenreList (an async Server Component)
// into a Suspense boundary. Rendering GenreList unresolved would hit the same
// "async component in JSX" limitation genreList.test.tsx works around, so
// here we stub the nested component's module instead: it has its own direct
// test, this one only needs to prove the page composes it correctly.
vi.mock("@/components/genres/genreList", () => ({
  default: () => <div>genre-list-stub</div>,
}));

describe("genres Page", () => {
  it("renders the genre list inside the page", () => {
    render(<Page />);
    expect(screen.getByText("genre-list-stub")).toBeInTheDocument();
  });
});
