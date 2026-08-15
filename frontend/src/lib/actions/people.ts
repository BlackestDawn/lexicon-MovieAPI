import { QueryParams } from "../data/interfaces/general";
import { PeopleSearchOptions } from "../data/interfaces/people";
import { PaginationMetadata } from "../data/models/paginationTypes";
import {
  PersonDto,
  PersonExtendedDto,
  validatePersonDto,
  validatePersonExtendedDto,
} from "../data/models/personTypes";
import { toQueryParams } from "../data/utils/converters";
import { apiGet, apiGetPaginated } from "./apiInteract";

export async function fetchPersons(
  options?: PeopleSearchOptions,
): Promise<{ persons: PersonDto[]; pagination: PaginationMetadata | null }> {
  const qs = toQueryParams(options as QueryParams);
  const { data, pagination } = await apiGetPaginated(`/people${qs}`);
  const validated = validatePersonDto(data) as PersonDto[];
  return { persons: validated as PersonDto[], pagination };
}

export async function getperson(id: string): Promise<PersonExtendedDto> {
  const result = await apiGet(`/people/${id}`);
  return validatePersonExtendedDto(result) as PersonExtendedDto;
}
