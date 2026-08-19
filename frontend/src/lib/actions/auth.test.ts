import { describe, expect, it, vi } from "vitest";
import { ValidationError } from "../data/interfaces/errors";

const { apiGet, apiPost, isAuthenticated, login } = vi.hoisted(() => ({
  apiGet: vi.fn(),
  apiPost: vi.fn(),
  isAuthenticated: vi.fn(),
  login: vi.fn(),
}));

vi.mock("./apiInteract", () => ({ apiGet, apiPost, isAuthenticated, login }));

const {
  loginRequest,
  registerRequest,
  forgotPasswordRequest,
  resetPasswordRequest,
  fetchCurrentUser,
} = await import("./auth");

const currentUserDto = {
  id: "9c858901-8a57-4791-81fe-4c455b099bc9",
  email: "a@example.com",
  role: "Moderator",
  displayName: "Alice",
};

describe("fetchCurrentUser", () => {
  it("returns null when there is no valid session", async () => {
    isAuthenticated.mockResolvedValue(false);
    await expect(fetchCurrentUser()).resolves.toBeNull();
    expect(apiGet).not.toHaveBeenCalled();
  });

  it("maps the current-user DTO to a User when authenticated", async () => {
    isAuthenticated.mockResolvedValue(true);
    apiGet.mockResolvedValue(currentUserDto);

    const result = await fetchCurrentUser();

    expect(apiGet).toHaveBeenCalledWith("/auth/me");
    expect(result).toEqual({
      id: currentUserDto.id,
      email: currentUserDto.email,
      name: currentUserDto.displayName,
      role: currentUserDto.role,
    });
  });
});

describe("loginRequest", () => {
  it("logs in and returns the freshly fetched user", async () => {
    login.mockResolvedValue(undefined);
    isAuthenticated.mockResolvedValue(true);
    apiGet.mockResolvedValue(currentUserDto);

    const result = await loginRequest("a@example.com", "Abcdef1!");

    expect(login).toHaveBeenCalledWith("a@example.com", "Abcdef1!");
    expect(result.name).toBe("Alice");
  });

  it("throws when the profile can't be loaded after login", async () => {
    login.mockResolvedValue(undefined);
    isAuthenticated.mockResolvedValue(false);

    await expect(loginRequest("a@example.com", "Abcdef1!")).rejects.toThrow(
      "Login succeeded but the user profile could not be loaded",
    );
  });
});

describe("registerRequest", () => {
  it("registers, logs in, and returns the user on success", async () => {
    apiPost.mockResolvedValue(undefined);
    login.mockResolvedValue(undefined);
    isAuthenticated.mockResolvedValue(true);
    apiGet.mockResolvedValue(currentUserDto);

    const result = await registerRequest({
      email: "a@example.com",
      password: "Abcdef1!",
      displayName: "Alice",
    });

    expect(apiPost).toHaveBeenCalledWith(
      "/auth/register",
      expect.objectContaining({ email: "a@example.com" }),
    );
    expect(result).toEqual({ success: true, user: expect.objectContaining({ name: "Alice" }) });
  });

  it("fails fast on invalid registration data without calling the API", async () => {
    const result = await registerRequest({ email: "not-an-email", password: "weak" });

    expect(apiPost).not.toHaveBeenCalled();
    expect(result.success).toBe(false);
    if (!result.success) {
      expect(result.issues).toContain("A valid email is required");
    }
  });

  it("surfaces a rejected registration from the API", async () => {
    apiPost.mockRejectedValue(new ValidationError("Email already in use", ["Email already in use"]));

    const result = await registerRequest({ email: "a@example.com", password: "Abcdef1!" });

    expect(result).toEqual({
      success: false,
      error: "Email already in use",
      issues: ["Email already in use"],
    });
  });
});

describe("forgotPasswordRequest", () => {
  it("posts the validated email", async () => {
    apiPost.mockResolvedValue(undefined);

    const result = await forgotPasswordRequest({ email: "a@example.com" });

    expect(apiPost).toHaveBeenCalledWith("/auth/forgot-password", { email: "a@example.com" });
    expect(result).toEqual({ success: true });
  });

  it("fails fast on an invalid email", async () => {
    const result = await forgotPasswordRequest({ email: "nope" });
    expect(apiPost).not.toHaveBeenCalled();
    expect(result.success).toBe(false);
  });
});

describe("resetPasswordRequest", () => {
  it("posts the validated reset details", async () => {
    apiPost.mockResolvedValue(undefined);
    const data = { email: "a@example.com", token: "abc", newPassword: "Abcdef1!" };

    const result = await resetPasswordRequest(data);

    expect(apiPost).toHaveBeenCalledWith("/auth/reset-password", data);
    expect(result).toEqual({ success: true });
  });

  it("fails fast when the token is missing", async () => {
    const result = await resetPasswordRequest({
      email: "a@example.com",
      token: "",
      newPassword: "Abcdef1!",
    });
    expect(apiPost).not.toHaveBeenCalled();
    expect(result.success).toBe(false);
  });
});
