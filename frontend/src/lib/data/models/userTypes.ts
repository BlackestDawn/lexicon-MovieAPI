import z from "zod";
import { ValidationError } from "../interfaces/errors";

export const userRoles = z.enum([
  "User",
  "PowerUser",
  "Moderator",
  "Administrator",
]);

export type UserRoles = z.infer<typeof userRoles>;

// Mirrors ASP.NET Identity's default IdentityOptions.Password rules (unmodified
// in Program.cs): min length 6, and at least one of each character class.
export const passwordSchema = z
  .string()
  .min(6, "Password must be at least 6 characters")
  .regex(/[a-z]/, "Password must contain a lowercase letter")
  .regex(/[A-Z]/, "Password must contain an uppercase letter")
  .regex(/[0-9]/, "Password must contain a digit")
  .regex(/[^a-zA-Z0-9]/, "Password must contain a non-alphanumeric character");

const userDtoSchema = z.object({
  id: z.guid(),
  email: z.string(),
  displayName: z.string(),
});

export type UserDto = z.infer<typeof userDtoSchema>;

export function validateUserDto(item: unknown): UserDto {
  const result = userDtoSchema.safeParse(item);
  if (!result.success) {
    console.error(`Invalid UserDto item:`, result.error);
    throw new Error("Invalid UserDto item");
  }

  return result.data as UserDto;
}

const currentUserDtoSchema = z.object({
  id: z.guid(),
  email: z.string(),
  role: userRoles,
  displayName: z.string(),
});

export type CurrentUserDto = z.infer<typeof currentUserDtoSchema>;

export function validateCurrentUserDto(item: unknown): CurrentUserDto {
  const result = currentUserDtoSchema.safeParse(item);
  if (!result.success) {
    console.error(`Invalid CurrentUserDto item:`, result.error);
    throw new Error("Invalid CurrentUserDto item");
  }

  return result.data as CurrentUserDto;
}

const userSchema = z.object({
  id: z.guid(),
  name: z.string(),
  email: z.email(),
  role: userRoles,
});

export type User = z.infer<typeof userSchema>;

export function validateUser(item: unknown): User {
  const result = userSchema.safeParse(item);
  if (!result.success) {
    console.error(`Invalid user item:`, result.error);
    throw new Error("Invalid User item");
  }

  return result.data as User;
}

// Mirrors UserUpdateValidator. displayName is optional - left unchanged when omitted.
export const userForUpdateSchema = z.object({
  email: z.email("A valid email is required"),
  displayName: z.string().max(100).optional(),
});

export type UserForUpdate = z.infer<typeof userForUpdateSchema>;

// Mirrors RegisterValidator. displayName is optional - falls back to the email's
// local part server-side when omitted.
export const registerDtoSchema = z.object({
  email: z.email("A valid email is required"),
  password: passwordSchema,
  displayName: z.string().max(100).optional(),
});

export type RegisterDto = z.infer<typeof registerDtoSchema>;

export function validateRegisterDto(item: unknown): RegisterDto {
  const result = registerDtoSchema.safeParse(item);
  if (!result.success) {
    console.error("Invalid Register item:", result.error);
    throw new ValidationError(
      "Invalid registration details",
      result.error.issues.map((e) => e.message),
    );
  }
  return result.data;
}

// Mirrors ChangePasswordValidator: new password must differ from the current one.
export const changePasswordSchema = z
  .object({
    currentPassword: z.string().min(1, "Current password is required"),
    newPassword: passwordSchema,
  })
  .refine((data) => data.newPassword !== data.currentPassword, {
    message: "New password must be different from the current password",
    path: ["newPassword"],
  });

export type ChangePassword = z.infer<typeof changePasswordSchema>;

// Mirrors ForgotPasswordValidator.
export const forgotPasswordDtoSchema = z.object({
  email: z.email("A valid email is required"),
});

export type ForgotPasswordDto = z.infer<typeof forgotPasswordDtoSchema>;

export function validateForgotPasswordDto(item: unknown): ForgotPasswordDto {
  const result = forgotPasswordDtoSchema.safeParse(item);
  if (!result.success) {
    console.error("Invalid ForgotPassword item:", result.error);
    throw new ValidationError(
      "Invalid password reset request",
      result.error.issues.map((e) => e.message),
    );
  }
  return result.data;
}

// Mirrors ResetPasswordValidator.
export const resetPasswordDtoSchema = z.object({
  email: z.email("A valid email is required"),
  token: z.string().min(1, "Token is required"),
  newPassword: passwordSchema,
});

export type ResetPasswordDto = z.infer<typeof resetPasswordDtoSchema>;

export function validateResetPasswordDto(item: unknown): ResetPasswordDto {
  const result = resetPasswordDtoSchema.safeParse(item);
  if (!result.success) {
    console.error("Invalid ResetPassword item:", result.error);
    throw new ValidationError(
      "Invalid password reset details",
      result.error.issues.map((e) => e.message),
    );
  }
  return result.data;
}
