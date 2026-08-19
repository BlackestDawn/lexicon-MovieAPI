import { describe, expect, it, vi } from "vitest";
import { ValidationError } from "../data/interfaces/errors";
import { PersonRole } from "../data/models/personRoleTypes";

const { apiGet, apiGetPaginated, apiPost, apiPut, apiDelete } = vi.hoisted(
  () => ({
    apiGet: vi.fn(),
    apiGetPaginated: vi.fn(),
    apiPost: vi.fn(),
    apiPut: vi.fn(),
    apiDelete: vi.fn(),
  }),
);

vi.mock("./apiInteract", () => ({
  apiGet,
  apiGetPaginated,
  apiPost,
  apiPut,
  apiDelete,
}));

vi.mock("next/cache", () => ({
  revalidatePath: vi.fn(),
}));

const { createPerson, updatePerson, removePerson, fetchPersons, getPerson } =
  await import("./person");

// The API returns dates as ISO strings; validatePersonDto coerces them to
// Date instances, so assertions compare against `validatedPerson` instead.
const person = {
  id: "9c858901-8a57-4791-81fe-4c455b099bc9",
  createdAt: "2024-01-01T00:00:00.000Z",
  updatedAt: "2024-01-01T00:00:00.000Z",
  givenName: "Jane",
  middleName: null,
  lastName: "Doe",
  dateOfBirth: "1980-05-15T00:00:00.000Z",
};

const validatedPerson = {
  ...person,
  createdAt: new Date(person.createdAt),
  updatedAt: new Date(person.updatedAt),
  dateOfBirth: new Date(person.dateOfBirth),
};

function formData(fields: Record<string, string>) {
  const data = new FormData();
  for (const [key, value] of Object.entries(fields)) data.set(key, value);
  return data;
}

function validChangeFields() {
  return {
    givenName: "Jane",
    middleName: "",
    lastName: "Doe",
    dateOfBirth: "1980-05-15",
    movieRolesData: JSON.stringify([
      { movieId: "9c858901-8a57-4791-81fe-4c455b099bc8", role: PersonRole.Cast },
    ]),
  };
}

describe("fetchPersons", () => {
  it("returns validated persons and pagination from the API", async () => {
    apiGetPaginated.mockResolvedValue({
      data: [person],
      pagination: { TotalItemCount: 1, TotalPageCount: 1, PageSize: 10, CurrentPage: 1 },
    });

    const result = await fetchPersons({ name: "Jane" });

    expect(apiGetPaginated).toHaveBeenCalledWith("/persons?name=Jane");
    expect(result.persons).toHaveLength(1);
    expect(result.pagination?.TotalItemCount).toBe(1);
  });
});

describe("getPerson", () => {
  it("fetches and validates a single person", async () => {
    const extended = { ...person, movieRoles: [] };
    apiGet.mockResolvedValue(extended);

    const result = await getPerson(person.id);

    expect(apiGet).toHaveBeenCalledWith(`/persons/${person.id}`);
    expect(result).toEqual({ ...validatedPerson, movieRoles: [] });
  });
});

describe("createPerson", () => {
  it("posts the parsed form data and reports success", async () => {
    apiPost.mockResolvedValue(person);

    const result = await createPerson(formData(validChangeFields()));

    expect(apiPost).toHaveBeenCalledWith("/persons", {
      givenName: "Jane",
      middleName: "",
      lastName: "Doe",
      dateOfBirth: "1980-05-15",
      movieRoles: [{ movieId: "9c858901-8a57-4791-81fe-4c455b099bc8", role: PersonRole.Cast }],
    });
    expect(result).toEqual({ success: true, person: validatedPerson });
  });

  it("fails fast on invalid form data without calling the API", async () => {
    const result = await createPerson(
      formData({ ...validChangeFields(), givenName: "", lastName: "" }),
    );

    expect(apiPost).not.toHaveBeenCalled();
    expect(result.success).toBe(false);
    expect(result.issues).toEqual([
      "Given name is required",
      "Last name is required",
    ]);
  });

  it("surfaces API validation issues", async () => {
    apiPost.mockRejectedValue(
      new ValidationError("Person already exists", ["Person already exists"]),
    );

    const result = await createPerson(formData(validChangeFields()));

    expect(result).toEqual({
      success: false,
      error: "Person already exists",
      issues: ["Person already exists"],
    });
  });
});

describe("updatePerson", () => {
  it("puts the parsed form data and reports success", async () => {
    apiPut.mockResolvedValue(undefined);

    const result = await updatePerson(person.id, formData(validChangeFields()));

    expect(apiPut).toHaveBeenCalledWith(
      `/persons/${person.id}`,
      expect.objectContaining({ givenName: "Jane", lastName: "Doe" }),
    );
    expect(result.success).toBe(true);
  });

  it("reports a generic error for a non-ValidationError failure", async () => {
    apiPut.mockRejectedValue(new Error("network down"));

    const result = await updatePerson(person.id, formData(validChangeFields()));

    expect(result).toEqual({
      success: false,
      error: "network down",
      issues: null,
    });
  });
});

describe("removePerson", () => {
  it("deletes the person by id", async () => {
    apiDelete.mockResolvedValue(undefined);
    await removePerson(person.id);
    expect(apiDelete).toHaveBeenCalledWith(`/persons/${person.id}`);
  });
});
