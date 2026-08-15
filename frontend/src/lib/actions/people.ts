import {
  PersonDto,
  PersonExtendedDto,
  validatePersonDto,
  validatePersonExtendedDto,
} from "../data/models/personTypes";
import { apiGet } from "./apiInteract";

export async function fetchPersons(): Promise<PersonDto[]> {
  const result = await apiGet("/people");
  return validatePersonDto(result) as PersonDto[];
}

export async function getperson(id: string): Promise<PersonExtendedDto> {
  const result = await apiGet(`/people/${id}`);
  return validatePersonExtendedDto(result) as PersonExtendedDto;
}
