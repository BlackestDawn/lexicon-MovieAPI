import { describe, expect, it } from "vitest";
import { render, screen } from "@testing-library/react";
import PaginationControls from "./paginationControls";

describe("PaginationControls", () => {
  it("renders nothing when there is only one page", () => {
    const { container } = render(
      <PaginationControls
        pagination={{ TotalItemCount: 3, TotalPageCount: 1, PageSize: 10, CurrentPage: 1 }}
        basePath="/genres"
      />,
    );
    expect(container).toBeEmptyDOMElement();
  });

  it("shows the current page and total pages", () => {
    render(
      <PaginationControls
        pagination={{ TotalItemCount: 30, TotalPageCount: 3, PageSize: 10, CurrentPage: 2 }}
        basePath="/genres/1"
      />,
    );
    expect(screen.getByText("Page 2 of 3")).toBeInTheDocument();
  });

  it("disables the previous link on the first page and links forward correctly", () => {
    render(
      <PaginationControls
        pagination={{ TotalItemCount: 30, TotalPageCount: 3, PageSize: 10, CurrentPage: 1 }}
        basePath="/genres/1"
      />,
    );
    const links = screen.getAllByRole("link");
    expect(links[0]).toHaveAttribute("aria-disabled", "true");
    expect(links[1]).toHaveAttribute("href", "/genres/1?page=2");
  });

  it("includes extra query params in generated links", () => {
    render(
      <PaginationControls
        pagination={{ TotalItemCount: 30, TotalPageCount: 3, PageSize: 10, CurrentPage: 2 }}
        basePath="/genres/1"
        queryParams={{ sort: "name" }}
      />,
    );
    const links = screen.getAllByRole("link");
    expect(links[1]).toHaveAttribute("href", "/genres/1?sort=name&page=3");
  });
});
