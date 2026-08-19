import { describe, expect, it } from "vitest";
import { ValidationError } from "./errors";

describe("ValidationError", () => {
  it("carries a message, name and issues, and survives instanceof checks", () => {
    const error = new ValidationError("Invalid input", ["Name is required"]);

    expect(error).toBeInstanceOf(Error);
    expect(error).toBeInstanceOf(ValidationError);
    expect(error.name).toBe("ValidationError");
    expect(error.message).toBe("Invalid input");
    expect(error.issues).toEqual(["Name is required"]);
  });

  it("is distinguishable from a plain Error via instanceof after being caught", () => {
    function throwIt(): never {
      throw new ValidationError("bad", ["issue"]);
    }

    try {
      throwIt();
      expect.unreachable("expected throwIt to throw");
    } catch (e) {
      expect(e instanceof ValidationError).toBe(true);
    }
  });
});
