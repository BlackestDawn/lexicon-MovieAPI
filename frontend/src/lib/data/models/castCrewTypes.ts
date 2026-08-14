import z from "zod";
import { personRoleSchema } from "./personRoleTypes";

export const castCrewDtoSchema = z.object({
  personId: z.guid(),
  givenName: z.string(),
  middleName: z.string().nullable().optional(),
  lastName: z.string(),
  role: personRoleSchema,
});

export type CastCrewDto = z.infer<typeof castCrewDtoSchema>;

export function validateCastCrewDto(
  item: unknown | unknown[],
): CastCrewDto | CastCrewDto[] {
  if (Array.isArray(item)) {
    const result = castCrewDtoSchema.array().safeParse(item);
    if (!result.success) {
      console.error("Invalid CastCrewDto:", result.error);
      throw new Error("invalid CastCrewDto item");
    }
    return result.data;
  }

  const result = castCrewDtoSchema.safeParse(item);
  if (!result.success) {
    console.error("Invalid CastCrewDto:", result.error);
    throw new Error("invalid CastCrewDto item");
  }
  return result.data;
}

export const castCrewForChangeSchema = z.object({
  personId: z.guid(),
  role: personRoleSchema,
});

export type CastCrewForChange = z.infer<typeof castCrewForChangeSchema>;
