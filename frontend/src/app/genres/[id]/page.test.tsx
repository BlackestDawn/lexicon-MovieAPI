import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import { notFound } from "next/navigation";
import Page from "./page";
import { NEXT_NOT_FOUND_MESSAGE } from "@/test-utils/nextNavigation";

// The mock factory can't close over an externally-imported function (Vitest
// hoists vi.mock above all imports — see test-utils/nextNavigation.ts), so
// the throw is inlined here and only the assertion message is shared.
vi.mock("next/navigation", () => ({
  notFound: vi.fn(() => {
    throw new Error("NEXT_NOT_FOUND");
  }),
}));

// GenreDetails is an async Server Component with its own direct test
// (genreDetails.test.tsx); here we only need to check that this page parses
// its route params/searchParams correctly and passes them through, so the
// nested component is stubbed to capture its received props.
vi.mock("@/components/genres/genreDetails", () => ({
  default: (props: { id: string; page?: number }) => (
    <div>genre-details-stub id={props.id} page={String(props.page)}</div>
  ),
}));

describe("genres/[id] Page", () => {
  it("passes the route id and numeric page through to GenreDetails", async () => {
    const jsx = await Page({
      params: Promise.resolve({ id: "genre-1" }),
      searchParams: Promise.resolve({ page: "3" }),
    });
    render(jsx);

    expect(screen.getByText("genre-details-stub", { exact: false })).toBeInTheDocument();
    expect(screen.getByText(/id=genre-1/)).toBeInTheDocument();
    expect(screen.getByText(/page=3/)).toBeInTheDocument();
  });

  it("leaves page undefined when no page query param is present", async () => {
    const jsx = await Page({
      params: Promise.resolve({ id: "genre-1" }),
      searchParams: Promise.resolve({}),
    });
    render(jsx);

    expect(screen.getByText(/page=undefined/)).toBeInTheDocument();
  });

  it("calls notFound() when the route id is missing", async () => {
    await expect(
      Page({
        params: Promise.resolve({ id: "" }),
        searchParams: Promise.resolve({}),
      }),
    ).rejects.toThrow(NEXT_NOT_FOUND_MESSAGE);
    expect(vi.mocked(notFound)).toHaveBeenCalled();
  });
});
