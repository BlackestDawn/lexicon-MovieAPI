import { describe, expect, it } from "vitest";
import { PersonRole, personRoleLabels, personRoleSchema } from "./personRoleTypes";

describe("personRoleLabels", () => {
  it("has a label for every PersonRole value", () => {
    const roleValues = Object.values(PersonRole).filter(
      (v): v is PersonRole => typeof v === "number",
    );
    for (const role of roleValues) {
      expect(personRoleLabels[role]).toBeTruthy();
    }
  });
});

describe("personRoleSchema", () => {
  it("accepts every known role value", () => {
    for (const role of Object.values(PersonRole).filter(
      (v): v is number => typeof v === "number",
    )) {
      expect(personRoleSchema.safeParse(role).success).toBe(true);
    }
  });

  it("rejects an unknown role value", () => {
    expect(personRoleSchema.safeParse(999).success).toBe(false);
  });
});
