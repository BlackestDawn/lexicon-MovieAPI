import z from "zod";
import { personRoleSchema } from "./personRoleTypes";

export const movieRoleDtoSchema = z.object({
  movieId: z.guid(),
  title: z.string(),
  role: personRoleSchema,
});

export type MovieRoleDto = z.infer<typeof movieRoleDtoSchema>;

export const movieRoleForCreationSchema = z.object({
  movieId: z.guid(),
  role: personRoleSchema,
});

export type MovieRoleForCreation = z.infer<typeof movieRoleForCreationSchema>;

const personDtoSchema = z.object({
  id: z.guid(),
  createdAt: z.coerce.date(),
  updatedAt: z.coerce.date(),
  givenName: z.string(),
  middleName: z.string().nullable().optional(),
  lastName: z.string(),
  dateOfBirth: z.coerce.date(),
});

export type PersonDto = z.infer<typeof personDtoSchema>;

export function validatePersonDto(
  item: unknown | unknown[],
): PersonDto | PersonDto[] {
  if (Array.isArray(item)) {
    const result = personDtoSchema.array().safeParse(item);
    if (!result.success) {
      console.error("Invalid PersonDto:", result.error);
      throw new Error("invalid PersonDto item");
    }
    return result.data;
  }

  const result = personDtoSchema.safeParse(item);
  if (!result.success) {
    console.error("Invalid PersonDto:", result.error);
    throw new Error("invalid PersonDto item");
  }
  return result.data;
}

const personExtendedDtoSchema = z.object({
  id: z.guid(),
  createdAt: z.coerce.date(),
  updatedAt: z.coerce.date(),
  givenName: z.string(),
  middleName: z.string().nullable().optional(),
  lastName: z.string(),
  dateOfBirth: z.coerce.date(),
  movieRoles: z.array(movieRoleDtoSchema),
});

export type PersonExtendedDto = z.infer<typeof personExtendedDtoSchema>;

export function validatePersonExtendedDto(item: unknown): PersonExtendedDto {
  const result = personExtendedDtoSchema.safeParse(item);
  if (!result.success) {
    console.error("Invalid PersonExtendedDto:", result.error);
    throw new Error("invalid PersonExtendedDto item");
  }
  return result.data;
}

// Mirrors PersonChangeValidator: date-of-birth window, and middle name capped at 50 chars.
export const personForChangeSchema = z.object({
  givenName: z.string().min(1, "Given name is required"),
  middleName: z.string().max(50).nullable().optional(),
  lastName: z.string().min(1, "Last name is required"),
  dateOfBirth: z.coerce.date().min(new Date(1750, 0, 1)).max(new Date()),
  movieRoles: z.array(movieRoleForCreationSchema),
});

export type PersonForChange = z.infer<typeof personForChangeSchema>;
