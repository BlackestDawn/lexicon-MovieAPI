"use server";

import type {
  CurrentUserDto,
  ForgotPasswordDto,
  RegisterDto,
  ResetPasswordDto,
  User,
  UserRoles,
} from "../data/models/userTypes";
import {
  validateChangePassword,
  validateCurrentUserDto,
  validateForgotPasswordDto,
  validateRegisterDto,
  validateResetPasswordDto,
  validateUserForUpdateDto,
} from "../data/models/userTypes";
import { ValidationError } from "../data/interfaces/errors";
import { apiGet, apiPost, apiPut, isAuthenticated, login } from "./apiInteract";

function toUser(dto: CurrentUserDto): User {
  return {
    id: dto.id,
    email: dto.email,
    name: dto.displayName,
    role: dto.role as UserRoles,
    createdAt: dto.createdAt,
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

type ActionResult =
  | { success: true }
  | { success: false; error: string; issues: string[] | null };

// Always reports success regardless of whether the email is registered - the
// backend deliberately doesn't reveal that, see AuthService.ForgotPassword.
export async function forgotPasswordRequest(
  data: ForgotPasswordDto,
): Promise<ActionResult> {
  try {
    const validated = validateForgotPasswordDto(data);
    await apiPost("/auth/forgot-password", validated);
    return { success: true };
  } catch (e) {
    console.error("Error requesting password reset:", e);
    return {
      success: false,
      error: e instanceof Error ? e.message : "Password reset request failed",
      issues: e instanceof ValidationError ? e.issues : null,
    };
  }
}

export async function resetPasswordRequest(
  data: ResetPasswordDto,
): Promise<ActionResult> {
  try {
    const validated = validateResetPasswordDto(data);
    await apiPost("/auth/reset-password", validated);
    return { success: true };
  } catch (e) {
    console.error("Error resetting password:", e);
    return {
      success: false,
      error: e instanceof Error ? e.message : "Password reset failed",
      issues: e instanceof ValidationError ? e.issues : null,
    };
  }
}

export async function changePasswordRequest(
  formData: FormData,
): Promise<ActionResult> {
  try {
    const validated = validateChangePassword({
      currentPassword: formData.get("currentPassword"),
      newPassword: formData.get("newPassword"),
    });
    await apiPut("/auth/me/password", validated);
    return { success: true };
  } catch (e) {
    console.error("Error changing password:", e);
    return {
      success: false,
      error: e instanceof Error ? e.message : "Password change failed",
      issues: e instanceof ValidationError ? e.issues : null,
    };
  }
}

type UpdateProfileResult =
  | { success: true; user: User }
  | { success: false; error: string; issues: string[] | null };

export async function updateProfileRequest(
  formData: FormData,
): Promise<UpdateProfileResult> {
  try {
    const validated = validateUserForUpdateDto({
      email: formData.get("email"),
      displayName: formData.get("displayName") || undefined,
    });
    await apiPut("/auth/me", validated);

    const user = await fetchCurrentUser();
    if (!user) {
      throw new Error("Profile updated but the user profile could not be reloaded");
    }
    return { success: true, user };
  } catch (e) {
    console.error("Error updating profile:", e);
    return {
      success: false,
      error: e instanceof Error ? e.message : "Profile update failed",
      issues: e instanceof ValidationError ? e.issues : null,
    };
  }
}

export async function fetchCurrentUser(): Promise<User | null> {
  if (!(await isAuthenticated())) return null;

  const dto = validateCurrentUserDto(await apiGet("/auth/me"));
  return toUser(dto);
}
