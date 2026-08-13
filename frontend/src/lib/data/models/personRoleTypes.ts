import z from "zod";

// Mirrors MovieAPI.Domain.Models.PersonRole; serialized as its numeric value
// since the backend has no JsonStringEnumConverter registered.
export enum PersonRole {
  Director = 0,
  Writer = 1,
  Cast = 2,
  Producer = 3,
  Cinematographer = 4,
  Costume = 5,
  EditorialDepartment = 6,
  Composer = 7,
  Makeup = 8,
  Art = 9,
  Sound = 10,
  VisualEffects = 11,
  Stunts = 12,
}

export const personRoleSchema = z.enum(PersonRole);

export type PersonRoleValue = z.infer<typeof personRoleSchema>;

export const personRoleLabels: Record<PersonRole, string> = {
  [PersonRole.Director]: "Director",
  [PersonRole.Writer]: "Writer",
  [PersonRole.Cast]: "Cast",
  [PersonRole.Producer]: "Producer",
  [PersonRole.Cinematographer]: "Cinematographer",
  [PersonRole.Costume]: "Costume",
  [PersonRole.EditorialDepartment]: "Editorial Department",
  [PersonRole.Composer]: "Composer",
  [PersonRole.Makeup]: "Makeup",
  [PersonRole.Art]: "Art",
  [PersonRole.Sound]: "Sound",
  [PersonRole.VisualEffects]: "Visual Effects",
  [PersonRole.Stunts]: "Stunts",
};
