import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import MovieCard from "./movieCard";
import CommonContext from "@/context/commonContext";
import { User } from "@/lib/data/models/userTypes";

vi.mock("next/navigation", () => ({
  useRouter: () => ({ push: vi.fn() }),
}));

const movie = {
  id: "9c858901-8a57-4791-81fe-4c455b099bc9",
  createdAt: new Date("2024-01-01"),
  updatedAt: new Date("2024-01-01"),
  title: "Die Hard",
  releaseDate: new Date("1988-07-15"),
  plotSummery: "A cop fights terrorists in a skyscraper.",
  runtimeMinutes: 132,
  averageRating: 8.2,
  genres: [{ id: "9c858901-8a57-4791-81fe-4c455b099bc1", name: "Action", slug: "action" }],
};

const moderator: User = {
  id: "0c858901-8a57-4791-81fe-4c455b099bc9",
  name: "Mod",
  email: "mod@example.com",
  role: "Moderator",
};

describe("MovieCard", () => {
  it("renders the title, year, runtime, rating and genres", () => {
    render(<MovieCard movie={movie} />);

    expect(screen.getByText("Die Hard", { exact: false })).toBeInTheDocument();
    expect(screen.getByText("(1988)", { exact: false })).toBeInTheDocument();
    expect(screen.getByText("2h 12m")).toBeInTheDocument();
    expect(screen.getByText("8.2/10")).toBeInTheDocument();
    expect(screen.getByText("Action")).toBeInTheDocument();
  });

  it("links to the movie's detail page", () => {
    render(<MovieCard movie={movie} />);
    expect(screen.getByRole("link")).toHaveAttribute(
      "href",
      `/movies/${movie.id}`,
    );
  });

  it("hides the delete button when not manageable", () => {
    render(<MovieCard movie={movie} />);
    expect(screen.queryByRole("button")).not.toBeInTheDocument();
  });

  it("shows the delete button when manageable and the user has access", () => {
    render(
      <CommonContext initialUser={moderator}>
        <MovieCard movie={movie} manageable />
      </CommonContext>,
    );
    expect(screen.getByRole("button")).toBeInTheDocument();
  });

  it("hides the delete button when manageable but the user lacks access", () => {
    render(
      <CommonContext initialUser={null}>
        <MovieCard movie={movie} manageable />
      </CommonContext>,
    );
    expect(screen.queryByRole("button")).not.toBeInTheDocument();
  });
});
