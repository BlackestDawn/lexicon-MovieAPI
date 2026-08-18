"use server";

import type {
  CurrentUserDto,
  RegisterDto,
  User,
  UserRoles,
} from "../data/models/userTypes";
import { validateRegisterDto } from "../data/models/userTypes";
import { ValidationError } from "../data/interfaces/errors";
import { apiGet, apiPost, isAuthenticated, login } from "./apiInteract";

function toUser(dto: CurrentUserDto): User {
  return {
    id: dto.id,
    email: dto.email,
    name: dto.displayName,
    role: dto.role as UserRoles,
  };
}

export async function loginRequest(
  email: string,
  password: string,
): Promise<User> {
  await login(email, password);

  const user = await fetchCurrentUser();
  if (!user) {
    throw new Error("Login succeeded but the user profile could not be loaded");
  }

  return user;
}

type RegisterResult =
  | { success: true; user: User }
  | { success: false; error: string; issues: string[] | null };

// Registration doesn't log the new user in server-side, so we follow up with
// the same password grant used by loginRequest - see AuthController.Register.
export async function registerRequest(data: RegisterDto): Promise<RegisterResult> {
  try {
    const validated = validateRegisterDto(data);
    await apiPost("/auth/register", validated);
    const user = await loginRequest(validated.email, validated.password);
    return { success: true, user };
  } catch (e) {
    console.error("Error registering user:", e);
    return {
      success: false,
      error: e instanceof Error ? e.message : "Registration failed",
      issues: e instanceof ValidationError ? e.issues : null,
    };
  }
}

export async function fetchCurrentUser(): Promise<User | null> {
  if (!(await isAuthenticated())) return null;

  const dto = await apiGet<CurrentUserDto>("/auth/me");
  return toUser(dto);
}
