import { describe, expect, it } from "vitest";
import { validateCastCrewDto } from "./castCrewTypes";
import { PersonRole } from "./personRoleTypes";

const validCastCrew = {
  personId: "9c858901-8a57-4791-81fe-4c455b099bc9",
  givenName: "Bruce",
  middleName: null,
  lastName: "Willis",
  role: PersonRole.Cast,
};

describe("validateCastCrewDto", () => {
  it("accepts a single valid item", () => {
    expect(validateCastCrewDto(validCastCrew)).toEqual(validCastCrew);
  });

  it("accepts an array of valid items", () => {
    const result = validateCastCrewDto([validCastCrew]) as unknown[];
    expect(result).toHaveLength(1);
  });

  it("throws on an unknown role value", () => {
    expect(() =>
      validateCastCrewDto({ ...validCastCrew, role: 999 }),
    ).toThrow("invalid CastCrewDto item");
  });

  it("throws when an item in an array is invalid", () => {
    expect(() => validateCastCrewDto([validCastCrew, { role: PersonRole.Cast }])).toThrow(
      "invalid CastCrewDto item",
    );
  });
});
