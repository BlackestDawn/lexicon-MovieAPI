import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import Page from "./page";
import { NEXT_NOT_FOUND_MESSAGE } from "@/test-utils/nextNavigation";

// Inline throw — a vi.mock factory can't close over an externally-imported
// function (see test-utils/nextNavigation.ts and app/genres/[id]/page.test.tsx).
vi.mock("next/navigation", () => ({
  notFound: vi.fn(() => {
    throw new Error("NEXT_NOT_FOUND");
  }),
}));

vi.mock("@/components/persons/personDetails", () => ({
  default: (props: { id: string }) => <div>person-details-stub id={props.id}</div>,
}));

describe("persons/[id] Page", () => {
  it("passes the route id through to PersonDetails", async () => {
    const jsx = await Page({ params: Promise.resolve({ id: "person-1" }) });
    render(jsx);

    expect(screen.getByText(/person-details-stub/)).toBeInTheDocument();
    expect(screen.getByText(/id=person-1/)).toBeInTheDocument();
  });

  it("calls notFound() when the route id is missing", async () => {
    await expect(
      Page({ params: Promise.resolve({ id: "" }) }),
    ).rejects.toThrow(NEXT_NOT_FOUND_MESSAGE);
  });
});
