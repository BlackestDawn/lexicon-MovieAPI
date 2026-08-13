import z from "zod";
import { passwordSchema, userRoles } from "./userTypes";

const adminUserDtoSchema = z.object({
  id: z.guid(),
  email: z.string(),
  role: z.string(),
  createdAt: z.coerce.date(),
});

export type AdminUserDto = z.infer<typeof adminUserDtoSchema>;

export function validateAdminUserDto(
  item: unknown | unknown[],
): AdminUserDto | AdminUserDto[] {
  if (Array.isArray(item)) {
    const result = adminUserDtoSchema.array().safeParse(item);
    if (!result.success) {
      console.error("Invalid AdminUserDto:", result.error);
      throw new Error("invalid AdminUserDto item");
    }
    return result.data;
  }

  const result = adminUserDtoSchema.safeParse(item);
  if (!result.success) {
    console.error("Invalid AdminUserDto:", result.error);
    throw new Error("invalid AdminUserDto item");
  }
  return result.data;
}

// Mirrors AdminUserCreationValidator.
export const adminUserForCreationSchema = z.object({
  email: z.email("A valid email is required"),
  password: passwordSchema,
  role: userRoles,
});

export type AdminUserForCreation = z.infer<typeof adminUserForCreationSchema>;

// Mirrors AdminUserUpdateValidator.
export const adminUserForUpdateSchema = z.object({
  email: z.email("A valid email is required"),
  role: userRoles,
});

export type AdminUserForUpdate = z.infer<typeof adminUserForUpdateSchema>;
