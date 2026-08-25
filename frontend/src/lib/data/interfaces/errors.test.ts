import { describe, expect, it } from "vitest";
import { ApiError, ValidationError } from "./errors";

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

describe("ApiError", () => {
  it("carries a message, name and status, and survives instanceof checks", () => {
    const error = new ApiError("Not Found", 404);

    expect(error).toBeInstanceOf(Error);
    expect(error).toBeInstanceOf(ApiError);
    expect(error.name).toBe("ApiError");
    expect(error.message).toBe("Not Found");
    expect(error.status).toBe(404);
  });

  it("is distinguishable from a plain Error via instanceof after being caught", () => {
    function throwIt(): never {
      throw new ApiError("Not Found", 404);
    }

    try {
      throwIt();
      expect.unreachable("expected throwIt to throw");
    } catch (e) {
      expect(e instanceof ApiError).toBe(true);
    }
  });
});
