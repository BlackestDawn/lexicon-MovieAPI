import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import PersonFilters from "./personFilters";

// Async Server Component (awaits fetchGenres) — direct-call pattern, see
// frontend_async_server_components memory / genreList.test.tsx.

const { fetchGenres } = vi.hoisted(() => ({ fetchGenres: vi.fn() }));
vi.mock("@/lib/actions/genre", () => ({ fetchGenres }));

const genres = [
  { id: "9c858901-8a57-4791-81fe-4c455b099bc1", name: "Action", slug: "action" },
  { id: "9c858901-8a57-4791-81fe-4c455b099bc2", name: "Comedy", slug: "comedy" },
];

async function renderFilters(props: {
  name?: string;
  genre?: string;
  year?: number;
} = {}) {
  const jsx = await PersonFilters(props);
  return render(jsx);
}

describe("PersonFilters", () => {
  it("renders empty inputs, every genre option, and no Clear link with no active filters", async () => {
    fetchGenres.mockResolvedValue(genres);
    await renderFilters();

    expect(screen.getByLabelText("Name")).toHaveValue("");
    expect(screen.getByLabelText("Genre")).toHaveValue("");
    expect(screen.getByLabelText("Birth year")).toHaveValue(null);
    expect(screen.getByRole("option", { name: "Action" })).toBeInTheDocument();
    expect(screen.getByRole("option", { name: "Comedy" })).toBeInTheDocument();
    expect(screen.queryByRole("link", { name: "Clear" })).not.toBeInTheDocument();
  });

  it("prefills inputs and shows a Clear link when filters are active", async () => {
    fetchGenres.mockResolvedValue(genres);
    await renderFilters({ name: "Bruce", genre: "action", year: 1955 });

    expect(screen.getByLabelText("Name")).toHaveValue("Bruce");
    expect(screen.getByLabelText("Genre")).toHaveValue("action");
    expect(screen.getByLabelText("Birth year")).toHaveValue(1955);
    expect(screen.getByRole("link", { name: "Clear" })).toHaveAttribute(
      "href",
      "/persons",
    );
  });
});
