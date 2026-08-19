import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import PersonEditButton from "./personEditButton";

vi.mock("@/lib/actions/person", () => ({
  createPerson: vi.fn(),
  updatePerson: vi.fn(),
}));

vi.mock("@/lib/actions/movie", () => ({
  fetchMovies: vi.fn().mockResolvedValue({ movies: [], pagination: null }),
}));

const person = {
  id: "9c858901-8a57-4791-81fe-4c455b099bc9",
  createdAt: new Date("2024-01-01"),
  updatedAt: new Date("2024-01-01"),
  givenName: "Jane",
  middleName: null,
  lastName: "Doe",
  dateOfBirth: new Date("1980-05-15"),
  movieRoles: [],
};

describe("PersonEditButton", () => {
  it("opens the edit dialog prefilled with the person's data", async () => {
    const user = userEvent.setup();
    render(<PersonEditButton person={person} />);

    await user.click(screen.getByRole("button"));

    const dialog = screen.getByRole("dialog", { name: "Edit person" });
    expect(dialog).toBeInTheDocument();
    expect(screen.getByLabelText("Given name")).toHaveValue("Jane");
    expect(screen.getByLabelText("Last name")).toHaveValue("Doe");
    expect(screen.getByLabelText("Date of birth")).toHaveValue("1980-05-15");
  });
});
