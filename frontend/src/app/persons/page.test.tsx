import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import Page from "./page";

// Page is itself async (it awaits searchParams), so it's called directly
// too; PersonsList has its own test, so it's stubbed here to check prop
// parsing/passing only.
vi.mock("@/components/persons/personList", () => ({
  default: (props: {
    page?: number;
    name?: string;
    genre?: string;
    year?: number;
  }) => (
    <div>
      persons-list-stub page={String(props.page)} name={String(props.name)}
      genre={String(props.genre)} year={String(props.year)}
    </div>
  ),
}));

describe("persons Page", () => {
  it("parses numeric query params and passes everything through to PersonsList", async () => {
    const jsx = await Page({
      searchParams: Promise.resolve({
        page: "2",
        name: "Bruce",
        genre: "action",
        year: "1955",
      }),
    });
    render(jsx);

    expect(screen.getByText(/persons-list-stub/)).toBeInTheDocument();
    expect(screen.getByText(/page=2/)).toBeInTheDocument();
    expect(screen.getByText(/name=Bruce/)).toBeInTheDocument();
    expect(screen.getByText(/genre=action/)).toBeInTheDocument();
    expect(screen.getByText(/year=1955/)).toBeInTheDocument();
  });

  it("leaves every field undefined with no query params", async () => {
    const jsx = await Page({ searchParams: Promise.resolve({}) });
    render(jsx);

    expect(screen.getByText(/page=undefined/)).toBeInTheDocument();
    expect(screen.getByText(/year=undefined/)).toBeInTheDocument();
  });
});
