import { describe, expect, it, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import PersonFullForm from "./personFormFull";
import { PersonRole } from "@/lib/data/models/personRoleTypes";

const { createPerson, updatePerson, fetchMovies } = vi.hoisted(() => ({
  createPerson: vi.fn(),
  updatePerson: vi.fn(),
  fetchMovies: vi.fn(),
}));

vi.mock("@/lib/actions/person", () => ({ createPerson, updatePerson }));
vi.mock("@/lib/actions/movie", () => ({ fetchMovies }));

const movieOne = { id: "movie-1", title: "Movie One" };
const movieTwo = { id: "movie-2", title: "Movie Two" };

const existingPerson = {
  id: "9c858901-8a57-4791-81fe-4c455b099bc9",
  createdAt: new Date("2024-01-01"),
  updatedAt: new Date("2024-01-01"),
  givenName: "Jane",
  middleName: null,
  lastName: "Doe",
  dateOfBirth: new Date("1980-05-15"),
  movieRoles: [{ movieId: movieOne.id, title: movieOne.title, role: PersonRole.Cast }],
};

function parseMovieRoles(call: unknown) {
  const formData = call as FormData;
  return JSON.parse(formData.get("movieRolesData") as string);
}

describe("PersonFullForm", () => {
  it("creates a person and closes the dialog on success", async () => {
    fetchMovies.mockResolvedValue({ movies: [movieOne], pagination: null });
    createPerson.mockResolvedValue({ success: true, person: {} });
    const onClose = vi.fn();
    const user = userEvent.setup();

    render(<PersonFullForm onClose={onClose} />);

    await user.type(screen.getByLabelText("Given name"), "John");
    await user.type(screen.getByLabelText("Last name"), "Smith");
    await user.click(screen.getByRole("button", { name: "Create person" }));

    await waitFor(() => expect(createPerson).toHaveBeenCalled());
    await waitFor(() => expect(onClose).toHaveBeenCalled());
    expect(updatePerson).not.toHaveBeenCalled();
  });

  it("shows returned issues and keeps the dialog open on failure", async () => {
    fetchMovies.mockResolvedValue({ movies: [], pagination: null });
    createPerson.mockResolvedValue({
      success: false,
      error: "Invalid person",
      issues: ["Given name is required"],
    });
    const onClose = vi.fn();
    const user = userEvent.setup();

    render(<PersonFullForm onClose={onClose} />);

    await user.click(screen.getByRole("button", { name: "Create person" }));

    expect(await screen.findByText("Given name is required")).toBeInTheDocument();
    expect(screen.getByText("Invalid person")).toBeInTheDocument();
    expect(onClose).not.toHaveBeenCalled();
  });

  it("prefills fields and existing movie roles when editing", async () => {
    fetchMovies.mockResolvedValue({ movies: [movieOne, movieTwo], pagination: null });

    render(<PersonFullForm onClose={vi.fn()} existingPerson={existingPerson} />);

    expect(screen.getByLabelText("Given name")).toHaveValue("Jane");
    expect(screen.getByLabelText("Date of birth")).toHaveValue("1980-05-15");
    expect(screen.getByText(/Movie One/)).toBeInTheDocument();

    // Movie One is already added, so only Movie Two should remain selectable.
    await waitFor(() =>
      expect(
        screen.queryByRole("option", { name: "Movie Two" }),
      ).toBeInTheDocument(),
    );
    expect(screen.queryByRole("option", { name: "Movie One" })).not.toBeInTheDocument();
  });

  it("adds and removes a movie role before submitting", async () => {
    fetchMovies.mockResolvedValue({ movies: [movieOne, movieTwo], pagination: null });
    createPerson.mockResolvedValue({ success: true, person: {} });
    const user = userEvent.setup();

    render(<PersonFullForm onClose={vi.fn()} />);

    await waitFor(() =>
      expect(screen.getByRole("option", { name: "Movie One" })).toBeInTheDocument(),
    );

    const [movieSelect] = screen.getAllByRole("combobox");
    await user.selectOptions(movieSelect, movieOne.id);
    await user.click(screen.getByRole("button", { name: "Add" }));

    expect(screen.getByText(/Movie One/)).toBeInTheDocument();

    await user.click(screen.getByRole("button", { name: "Remove Movie One" }));
    expect(
      screen.queryByRole("button", { name: "Remove Movie One" }),
    ).not.toBeInTheDocument();
    expect(screen.getByText("No movie roles added yet.")).toBeInTheDocument();
  });

  it("includes a selected-but-unadded movie role when submitting", async () => {
    fetchMovies.mockResolvedValue({ movies: [movieOne], pagination: null });
    createPerson.mockResolvedValue({ success: true, person: {} });
    const user = userEvent.setup();

    render(<PersonFullForm onClose={vi.fn()} />);

    await waitFor(() =>
      expect(screen.getByRole("option", { name: "Movie One" })).toBeInTheDocument(),
    );

    const [movieSelect] = screen.getAllByRole("combobox");
    await user.selectOptions(movieSelect, movieOne.id);
    // Note: "Add" is never clicked here — the pending selection should still
    // be included in the submitted data (see the comment in personFormFull.tsx).
    await user.type(screen.getByLabelText("Given name"), "John");
    await user.type(screen.getByLabelText("Last name"), "Smith");
    await user.click(screen.getByRole("button", { name: "Create person" }));

    await waitFor(() => expect(createPerson).toHaveBeenCalled());
    const submittedRoles = parseMovieRoles(createPerson.mock.calls[0][0]);
    expect(submittedRoles).toEqual([{ movieId: movieOne.id, role: PersonRole.Cast }]);
  });
});
