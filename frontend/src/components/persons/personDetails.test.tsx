import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import PersonDetails from "./personDetails";
import CommonContext from "@/context/commonContext";
import { User } from "@/lib/data/models/userTypes";
import { PersonRole } from "@/lib/data/models/personRoleTypes";
import { NEXT_NOT_FOUND_MESSAGE } from "@/test-utils/nextNavigation";

// Async Server Component — direct-call pattern (see genreDetails.test.tsx).

const { getPerson, removePerson } = vi.hoisted(() => ({
  getPerson: vi.fn(),
  removePerson: vi.fn(),
}));
vi.mock("@/lib/actions/person", () => ({ getPerson, removePerson }));
vi.mock("next/navigation", () => ({
  useRouter: () => ({ push: vi.fn() }),
  notFound: vi.fn(() => {
    throw new Error("NEXT_NOT_FOUND");
  }),
}));

const person = {
  id: "9c858901-8a57-4791-81fe-4c455b099bc9",
  givenName: "Bruce",
  middleName: null,
  lastName: "Willis",
  dateOfBirth: new Date("1955-03-19"),
  movieRoles: [
    { movieId: "9c858901-8a57-4791-81fe-4c455b099bc1", title: "Die Hard", role: PersonRole.Cast },
  ],
};

const powerUser: User = {
  id: "0c858901-8a57-4791-81fe-4c455b099bc9",
  name: "Power",
  email: "power@example.com",
  role: "PowerUser",
};

const moderator: User = {
  id: "1c858901-8a57-4791-81fe-4c455b099bc9",
  name: "Mod",
  email: "mod@example.com",
  role: "Moderator",
};

async function renderPersonDetails(user: User | null) {
  const jsx = await PersonDetails({ id: person.id });
  return render(<CommonContext initialUser={user}>{jsx}</CommonContext>);
}

describe("PersonDetails", () => {
  it("renders the person's name, date of birth, and filmography", async () => {
    getPerson.mockResolvedValue(person);
    await renderPersonDetails(null);

    expect(
      screen.getByRole("heading", { name: "Bruce Willis" }),
    ).toBeInTheDocument();
    expect(screen.getByText(/Born/)).toBeInTheDocument();
    expect(screen.getByRole("link", { name: /Die Hard/ })).toHaveAttribute(
      "href",
      `/movies/${person.movieRoles[0].movieId}`,
    );
  });

  it("shows an empty state when there are no movie roles", async () => {
    getPerson.mockResolvedValue({ ...person, movieRoles: [] });
    await renderPersonDetails(null);
    expect(screen.getByText("No movie roles registered")).toBeInTheDocument();
  });

  it("hides edit/delete controls for a user without access", async () => {
    getPerson.mockResolvedValue(person);
    await renderPersonDetails(null);
    expect(screen.queryByRole("button")).not.toBeInTheDocument();
  });

  it("shows the edit button for a power user but not the delete button", async () => {
    getPerson.mockResolvedValue(person);
    await renderPersonDetails(powerUser);
    expect(screen.getAllByRole("button")).toHaveLength(1);
  });

  it("shows both edit and delete controls for a moderator", async () => {
    getPerson.mockResolvedValue(person);
    await renderPersonDetails(moderator);
    expect(screen.getAllByRole("button")).toHaveLength(2);
  });

  it("calls notFound() when the person can't be found", async () => {
    getPerson.mockResolvedValue(null);
    await expect(PersonDetails({ id: "missing" })).rejects.toThrow(
      NEXT_NOT_FOUND_MESSAGE,
    );
  });
});
