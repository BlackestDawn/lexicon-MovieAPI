import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import PersonsList from "./personList";
import CommonContext from "@/context/commonContext";
import { User } from "@/lib/data/models/userTypes";

// Async Server Component — direct-call pattern (see genreList.test.tsx).
// PersonFilters is itself async (it fetches genres), so — like a page nesting
// an async child — it's stubbed here rather than resolved through; it has
// its own dedicated test in personFilters.test.tsx.

const { fetchPersons } = vi.hoisted(() => ({ fetchPersons: vi.fn() }));
vi.mock("@/lib/actions/person", () => ({
  fetchPersons,
  createPerson: vi.fn(),
  updatePerson: vi.fn(),
}));
vi.mock("@/lib/actions/movie", () => ({
  fetchMovies: vi.fn().mockResolvedValue({ movies: [], pagination: null }),
}));
vi.mock("./personFilters", () => ({
  default: () => <div>person-filters-stub</div>,
}));

const persons = [
  {
    id: "9c858901-8a57-4791-81fe-4c455b099bc1",
    givenName: "Bruce",
    middleName: null,
    lastName: "Willis",
    dateOfBirth: new Date("1955-03-19"),
  },
];

const powerUser: User = {
  id: "0c858901-8a57-4791-81fe-4c455b099bc9",
  name: "Power",
  email: "power@example.com",
  role: "PowerUser",
};

async function renderPersonsList(
  user: User | null,
  props: { page?: number; name?: string; genre?: string; year?: number } = {},
) {
  const jsx = await PersonsList(props);
  return render(<CommonContext initialUser={user}>{jsx}</CommonContext>);
}

describe("PersonsList", () => {
  it("renders a card with a detail link for every fetched person", async () => {
    fetchPersons.mockResolvedValue({ persons, pagination: null });
    await renderPersonsList(null);

    expect(screen.getByRole("link", { name: /Bruce Willis/ })).toHaveAttribute(
      "href",
      `/persons/${persons[0].id}`,
    );
  });

  it("renders the (stubbed) filters", async () => {
    fetchPersons.mockResolvedValue({ persons: [], pagination: null });
    await renderPersonsList(null);
    expect(screen.getByText("person-filters-stub")).toBeInTheDocument();
  });

  it("shows an empty state when no persons match", async () => {
    fetchPersons.mockResolvedValue({ persons: [], pagination: null });
    await renderPersonsList(null);
    expect(
      screen.getByText("No persons found matching your filters."),
    ).toBeInTheDocument();
  });

  it("renders pagination controls carrying the active filters", async () => {
    fetchPersons.mockResolvedValue({
      persons,
      pagination: { TotalItemCount: 20, TotalPageCount: 2, PageSize: 10, CurrentPage: 1 },
    });
    await renderPersonsList(null, { name: "Bruce" });

    expect(screen.getByText("Page 1 of 2")).toBeInTheDocument();
    const links = screen.getAllByRole("link");
    const nextLink = links.find((el) =>
      el.getAttribute("href")?.includes("page=2"),
    );
    expect(nextLink).toHaveAttribute("href", "/persons?name=Bruce&page=2");
  });

  it("hides the create button for a user without access", async () => {
    fetchPersons.mockResolvedValue({ persons: [], pagination: null });
    await renderPersonsList(null);
    expect(
      screen.queryByRole("button", { name: "Create new person" }),
    ).not.toBeInTheDocument();
  });

  it("shows the create button for a power user", async () => {
    fetchPersons.mockResolvedValue({ persons: [], pagination: null });
    await renderPersonsList(powerUser);
    expect(
      screen.getByRole("button", { name: "Create new person" }),
    ).toBeInTheDocument();
  });
});
