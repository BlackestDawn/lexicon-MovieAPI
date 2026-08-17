import { revalidatePath } from "next/cache";
import { QueryParams } from "../data/interfaces/general";
import { PeopleSearchOptions } from "../data/interfaces/people";
import { PaginationMetadata } from "../data/models/paginationTypes";
import {
  PersonDto,
  PersonExtendedDto,
  PersonForChangeDto,
  validatePersonDto,
  validatePersonExtendedDto,
  validatePersonForChangeDto,
} from "../data/models/personTypes";
import { toQueryParams } from "../data/utils/converters";
import { apiDelete, apiGet, apiGetPaginated, apiPost, apiPut } from "./apiInteract";
import { ValidationError } from "../data/interfaces/errors";

export async function fetchPersons(
  options?: PeopleSearchOptions,
): Promise<{ persons: PersonDto[]; pagination: PaginationMetadata | null }> {
  const qs = toQueryParams(options as QueryParams);
  const { data, pagination } = await apiGetPaginated(`/people${qs}`);
  const validated = validatePersonDto(data) as PersonDto[];
  return { persons: validated as PersonDto[], pagination };
}

export async function getPerson(id: string): Promise<PersonExtendedDto> {
  const result = await apiGet(`/people/${id}`);
  return validatePersonExtendedDto(result) as PersonExtendedDto;
}

export async function createPerson(formData: FormData) {
  try {
    const data = formToPersonChangeData(formData);

    const result = await apiPost("/people", data);
    const validated = validatePersonDto(result);
    revalidatePath("/people");
    return { success: true, person: validated };
  } catch (e) {
    console.error("Error creating person:", e);
    return {
      success: false,
      error: e instanceof Error ? e.message : "Person creation failed",
      issues: e instanceof ValidationError ? e.issues : null,
    };
  }
}

export async function updatePerson(id:string,formData: FormData) {
  try {
    const data = formToPersonChangeData(formData);

    const result = await apiPut(`/people/${id}`, data);
    const validated = validatePersonDto(result);
    revalidatePath(`/people/${id}`);
    return { success: true, person: validated };
  } catch (e) {
    console.error("Error updating person:", e);
    return {
      success: false,
      error: e instanceof Error ? e.message : "Person update failed",
      issues: e instanceof ValidationError ? e.issues : null,
    };
  }
}

export async function removePerson(id: string) {
  try {
    await apiDelete<void>(`/people/${id}`);

    revalidatePath("/people");
    return { success: true };
  } catch (e) {
    console.error("Error deleting person:", e);
    return {
      success: false,
      error: e instanceof Error ? e.message : "Person deletion failed",
    };
  }
}

function formToPersonChangeData(data: FormData) {
  const parsed: PersonForChangeDto = {
    givenName: data.get("givenName") as string,
    middleName: data.get("middleName") as string,
    lastName: data.get("lastName") as string,
    dateOfBirth: data.get("dateOfBirth") as string,
    movieRoles: JSON.parse(data.get("movieRolesData") as string),
  };

  return validatePersonForChangeDto(parsed);
}
