import { describe, expect, it } from "vitest";
import { render, screen } from "@testing-library/react";
import ReviewFilters from "./reviewFilters";

describe("ReviewFilters", () => {
  it("renders empty inputs and no Clear link with no active filters", () => {
    render(<ReviewFilters movieId="movie-1" />);

    expect(screen.getByLabelText("Search")).toHaveValue("");
    expect(screen.getByLabelText("Min score")).toHaveValue(null);
    expect(screen.getByLabelText("Max score")).toHaveValue(null);
    expect(screen.queryByRole("link", { name: "Clear" })).not.toBeInTheDocument();
  });

  it("prefills inputs and shows a Clear link back to the movie page when filters are active", () => {
    render(
      <ReviewFilters movieId="movie-1" search="great" minScore={5} maxScore={9} />,
    );

    expect(screen.getByLabelText("Search")).toHaveValue("great");
    expect(screen.getByLabelText("Min score")).toHaveValue(5);
    expect(screen.getByLabelText("Max score")).toHaveValue(9);
    expect(screen.getByRole("link", { name: "Clear" })).toHaveAttribute(
      "href",
      "/movies/movie-1",
    );
  });
});
