import { describe, expect, it } from "vitest";
import {
  passwordSchema,
  validateCurrentUserDto,
  validateForgotPasswordDto,
  validateRegisterDto,
  validateResetPasswordDto,
  validateUser,
  validateUserDto,
} from "./userTypes";
import { ValidationError } from "../interfaces/errors";

const guid = "9c858901-8a57-4791-81fe-4c455b099bc9";

describe("validateUserDto", () => {
  it("accepts a valid item", () => {
    const item = { id: guid, email: "a@example.com", displayName: "Alice" };
    expect(validateUserDto(item)).toEqual(item);
  });

  it("throws on an invalid item", () => {
    expect(() => validateUserDto({ id: "not-a-guid" })).toThrow(
      "Invalid UserDto item",
    );
  });
});

describe("validateCurrentUserDto", () => {
  it("accepts a valid item including role", () => {
    const item = { id: guid, email: "a@example.com", role: "Moderator", displayName: "Alice" };
    expect(validateCurrentUserDto(item)).toEqual(item);
  });

  it("throws when role is not a known value", () => {
    expect(() =>
      validateCurrentUserDto({ id: guid, email: "a@example.com", role: "Owner", displayName: "Alice" }),
    ).toThrow("Invalid CurrentUserDto item");
  });
});

describe("validateUser", () => {
  it("accepts a valid user", () => {
    const item = { id: guid, name: "Alice", email: "a@example.com", role: "User" };
    expect(validateUser(item)).toEqual(item);
  });

  it("throws when the email is malformed", () => {
    expect(() =>
      validateUser({ id: guid, name: "Alice", email: "not-an-email", role: "User" }),
    ).toThrow("Invalid User item");
  });
});

describe("passwordSchema", () => {
  it("accepts a password meeting every character class", () => {
    expect(passwordSchema.safeParse("Abcdef1!").success).toBe(true);
  });

  it.each([
    ["too short", "Ab1!"],
    ["missing lowercase", "ABCDEF1!"],
    ["missing uppercase", "abcdef1!"],
    ["missing digit", "Abcdefg!"],
    ["missing non-alphanumeric", "Abcdefg1"],
  ])("rejects a password %s", (_label, password) => {
    expect(passwordSchema.safeParse(password).success).toBe(false);
  });
});

describe("validateRegisterDto", () => {
  it("accepts a valid registration", () => {
    const data = { email: "a@example.com", password: "Abcdef1!", displayName: "Alice" };
    expect(validateRegisterDto(data)).toEqual(data);
  });

  it("throws a ValidationError listing every issue", () => {
    try {
      validateRegisterDto({ email: "not-an-email", password: "weak" });
      expect.unreachable("expected validateRegisterDto to throw");
    } catch (e) {
      expect(e).toBeInstanceOf(ValidationError);
      const issues = (e as ValidationError).issues;
      expect(issues).toContain("A valid email is required");
      expect(issues.some((i) => i.includes("6 characters"))).toBe(true);
    }
  });
});

describe("validateForgotPasswordDto", () => {
  it("accepts a valid email", () => {
    expect(validateForgotPasswordDto({ email: "a@example.com" })).toEqual({
      email: "a@example.com",
    });
  });

  it("throws a ValidationError for an invalid email", () => {
    expect(() => validateForgotPasswordDto({ email: "nope" })).toThrow(
      ValidationError,
    );
  });
});

describe("validateResetPasswordDto", () => {
  it("accepts valid reset details", () => {
    const data = { email: "a@example.com", token: "abc123", newPassword: "Abcdef1!" };
    expect(validateResetPasswordDto(data)).toEqual(data);
  });

  it("throws a ValidationError when the token is empty", () => {
    try {
      validateResetPasswordDto({ email: "a@example.com", token: "", newPassword: "Abcdef1!" });
      expect.unreachable("expected validateResetPasswordDto to throw");
    } catch (e) {
      expect(e).toBeInstanceOf(ValidationError);
      expect((e as ValidationError).issues).toContain("Token is required");
    }
  });
});
