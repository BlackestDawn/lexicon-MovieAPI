import { describe, expect, it, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import MovieFormFull from "./movieFormFull";
import { PersonRole } from "@/lib/data/models/personRoleTypes";

const { createMovie, updateMovie, fetchGenres, fetchPersons } = vi.hoisted(() => ({
  createMovie: vi.fn(),
  updateMovie: vi.fn(),
  fetchGenres: vi.fn(),
  fetchPersons: vi.fn(),
}));

vi.mock("@/lib/actions/movie", () => ({ createMovie, updateMovie }));
vi.mock("@/lib/actions/genre", () => ({ fetchGenres }));
vi.mock("@/lib/actions/person", () => ({ fetchPersons }));

const genreAction = { id: "genre-1", name: "Action", slug: "action" };
const genreComedy = { id: "genre-2", name: "Comedy", slug: "comedy" };

const personOne = { id: "person-1", givenName: "Bruce", middleName: null, lastName: "Willis" };
const personTwo = { id: "person-2", givenName: "Alan", middleName: null, lastName: "Rickman" };

const existingMovie = {
  id: "9c858901-8a57-4791-81fe-4c455b099bc9",
  createdAt: new Date("2024-01-01"),
  updatedAt: new Date("2024-01-01"),
  title: "Die Hard",
  releaseDate: new Date("1988-07-15"),
  plotSummery: "A cop fights terrorists in a skyscraper.",
  runtimeMinutes: 132,
  averageRating: 8.2,
  genres: [genreAction],
  castCrews: [
    { personId: personOne.id, givenName: "Bruce", lastName: "Willis", role: PersonRole.Cast },
  ],
  reviews: [],
  details: { id: "9c858901-8a57-4791-81fe-4c455b099bc9", synopsis: "Longer synopsis", language: "English", budget: 28000000 },
};

async function fillRequiredFields(user: ReturnType<typeof userEvent.setup>) {
  await user.type(screen.getByLabelText("Title"), "New Movie");
  await user.type(screen.getByLabelText("Language"), "English");
  await user.type(screen.getByLabelText("Plot summary"), "A summary");
  await user.type(screen.getByLabelText("Synopsis"), "A longer synopsis");
}

function parseJsonField(call: unknown, field: string) {
  const formData = call as FormData;
  return JSON.parse(formData.get(field) as string);
}

describe("MovieFormFull", () => {
  it("creates a movie and closes the dialog on success", async () => {
    fetchGenres.mockResolvedValue([genreAction]);
    fetchPersons.mockResolvedValue({ persons: [], pagination: null });
    createMovie.mockResolvedValue({ success: true, movie: {} });
    const onClose = vi.fn();
    const user = userEvent.setup();

    render(<MovieFormFull onClose={onClose} />);

    await waitFor(() =>
      expect(screen.getByRole("checkbox", { name: "Action" })).toBeInTheDocument(),
    );
    await fillRequiredFields(user);
    await user.click(screen.getByRole("checkbox", { name: "Action" }));
    await user.click(screen.getByRole("button", { name: "Create movie" }));

    await waitFor(() => expect(createMovie).toHaveBeenCalled());
    await waitFor(() => expect(onClose).toHaveBeenCalled());
    expect(updateMovie).not.toHaveBeenCalled();
  });

  it("shows returned issues and keeps the dialog open on failure", async () => {
    fetchGenres.mockResolvedValue([]);
    fetchPersons.mockResolvedValue({ persons: [], pagination: null });
    createMovie.mockResolvedValue({
      success: false,
      error: "Invalid movie",
      issues: ["Must have at least 1 genre"],
    });
    const onClose = vi.fn();
    const user = userEvent.setup();

    render(<MovieFormFull onClose={onClose} />);
    await fillRequiredFields(user);
    await user.click(screen.getByRole("button", { name: "Create movie" }));

    expect(
      await screen.findByText("Must have at least 1 genre"),
    ).toBeInTheDocument();
    expect(screen.getByText("Invalid movie")).toBeInTheDocument();
    expect(onClose).not.toHaveBeenCalled();
  });

  it("prefills fields, checked genres and existing cast/crew when editing", async () => {
    fetchGenres.mockResolvedValue([genreAction, genreComedy]);
    fetchPersons.mockResolvedValue({ persons: [personOne, personTwo], pagination: null });

    render(<MovieFormFull onClose={vi.fn()} existingMovie={existingMovie} />);

    expect(screen.getByLabelText("Title")).toHaveValue("Die Hard");
    expect(screen.getByLabelText("Release date")).toHaveValue("1988-07-15");
    expect(screen.getByLabelText("Budget")).toHaveValue(28000000);
    expect(screen.getByLabelText("Language")).toHaveValue("English");

    await waitFor(() =>
      expect(screen.getByRole("checkbox", { name: "Action" })).toBeChecked(),
    );
    expect(screen.getByRole("checkbox", { name: "Comedy" })).not.toBeChecked();
    expect(screen.getByText(/Bruce Willis/)).toBeInTheDocument();

    // Bruce Willis is already cast, so only Alan Rickman should remain selectable.
    await waitFor(() =>
      expect(screen.getByRole("option", { name: "Alan Rickman" })).toBeInTheDocument(),
    );
    expect(screen.queryByRole("option", { name: "Bruce Willis" })).not.toBeInTheDocument();
  });

  it("adds and removes a cast/crew member before submitting", async () => {
    fetchGenres.mockResolvedValue([]);
    fetchPersons.mockResolvedValue({ persons: [personOne], pagination: null });
    const user = userEvent.setup();

    render(<MovieFormFull onClose={vi.fn()} />);

    await waitFor(() =>
      expect(screen.getByRole("option", { name: "Bruce Willis" })).toBeInTheDocument(),
    );

    const [personSelect] = screen.getAllByRole("combobox");
    await user.selectOptions(personSelect, personOne.id);
    await user.click(screen.getByRole("button", { name: "Add" }));

    expect(screen.getByText(/Bruce Willis/)).toBeInTheDocument();

    await user.click(screen.getByRole("button", { name: "Remove Bruce Willis" }));
    expect(
      screen.queryByRole("button", { name: "Remove Bruce Willis" }),
    ).not.toBeInTheDocument();
    expect(screen.getByText("No cast or crew added yet.")).toBeInTheDocument();
  });

  it("includes a selected-but-unadded cast/crew member when submitting", async () => {
    fetchGenres.mockResolvedValue([genreAction]);
    fetchPersons.mockResolvedValue({ persons: [personOne], pagination: null });
    createMovie.mockResolvedValue({ success: true, movie: {} });
    const user = userEvent.setup();

    render(<MovieFormFull onClose={vi.fn()} />);

    await waitFor(() =>
      expect(screen.getByRole("option", { name: "Bruce Willis" })).toBeInTheDocument(),
    );
    const [personSelect] = screen.getAllByRole("combobox");
    await user.selectOptions(personSelect, personOne.id);
    // Note: "Add" is never clicked — the pending selection should still be
    // included in the submitted data (see the comment in movieFormFull.tsx).

    await fillRequiredFields(user);
    await user.click(screen.getByRole("checkbox", { name: "Action" }));
    await user.click(screen.getByRole("button", { name: "Create movie" }));

    await waitFor(() => expect(createMovie).toHaveBeenCalled());
    const submittedCastCrew = parseJsonField(createMovie.mock.calls[0][0], "castCrewData");
    expect(submittedCastCrew).toEqual([{ personId: personOne.id, role: PersonRole.Cast }]);
  });
});
