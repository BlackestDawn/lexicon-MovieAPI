import { describe, expect, it } from "vitest";
import {
  validatePersonDto,
  validatePersonExtendedDto,
  validatePersonForChangeDto,
} from "./personTypes";
import { PersonRole } from "./personRoleTypes";
import { ValidationError } from "../interfaces/errors";

const validPerson = {
  id: "9c858901-8a57-4791-81fe-4c455b099bc9",
  createdAt: "2024-01-01T00:00:00.000Z",
  updatedAt: "2024-01-01T00:00:00.000Z",
  givenName: "Jane",
  middleName: null,
  lastName: "Doe",
  dateOfBirth: "1980-05-15T00:00:00.000Z",
};

describe("validatePersonDto", () => {
  it("accepts a single valid person", () => {
    const result = validatePersonDto(validPerson);
    expect(result).toMatchObject({ givenName: "Jane", lastName: "Doe" });
  });

  it("accepts an array of valid persons", () => {
    const result = validatePersonDto([validPerson]) as unknown[];
    expect(result).toHaveLength(1);
  });

  it("throws on an invalid item", () => {
    expect(() => validatePersonDto({ id: "not-a-guid" })).toThrow(
      "invalid PersonDto item",
    );
  });
});

describe("validatePersonExtendedDto", () => {
  it("accepts a person with movie roles", () => {
    const item = {
      ...validPerson,
      movieRoles: [
        {
          movieId: "9c858901-8a57-4791-81fe-4c455b099bc8",
          title: "Die Hard",
          role: PersonRole.Cast,
        },
      ],
    };
    const result = validatePersonExtendedDto(item);
    expect(result.movieRoles).toHaveLength(1);
  });

  it("throws when movieRoles is missing", () => {
    expect(() => validatePersonExtendedDto(validPerson)).toThrow(
      "invalid PersonExtendedDto item",
    );
  });
});

describe("validatePersonForChangeDto", () => {
  const validChange = {
    givenName: "Jane",
    middleName: null,
    lastName: "Doe",
    dateOfBirth: "1980-05-15",
    movieRoles: [
      { movieId: "9c858901-8a57-4791-81fe-4c455b099bc8", role: PersonRole.Cast },
    ],
  };

  it("accepts a valid change and normalizes dateOfBirth to yyyy-MM-dd", () => {
    const result = validatePersonForChangeDto(validChange);
    expect(result.dateOfBirth).toBe("1980-05-15");
  });

  it("throws a ValidationError with per-field issues for missing required fields", () => {
    try {
      validatePersonForChangeDto({
        ...validChange,
        givenName: "",
        lastName: "",
      });
      expect.unreachable("expected validatePersonForChangeDto to throw");
    } catch (e) {
      expect(e).toBeInstanceOf(ValidationError);
      const issues = (e as ValidationError).issues;
      expect(issues).toContain("Given name is required");
      expect(issues).toContain("Last name is required");
    }
  });

  it("rejects a date of birth in the future", () => {
    const futureDate = new Date();
    futureDate.setFullYear(futureDate.getFullYear() + 1);

    expect(() =>
      validatePersonForChangeDto({
        ...validChange,
        dateOfBirth: futureDate.toISOString(),
      }),
    ).toThrow(ValidationError);
  });
});
