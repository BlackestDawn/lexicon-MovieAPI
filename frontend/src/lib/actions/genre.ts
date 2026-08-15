import { QueryParams } from "../data/interfaces/general";
import { GenreSearchOptions } from "../data/interfaces/genre";
import {
  GenreDto,
  GenreExtendedDto,
  validateGenreDto,
  validateGenreExtendedDto,
} from "../data/models/genreTypes";
import { PaginationMetadata } from "../data/models/paginationTypes";
import { toQueryParams } from "../data/utils/converters";
import { apiGet, apiGetPaginated } from "./apiInteract";

export async function fetchGenres(): Promise<GenreDto[]> {
  const result = await apiGet<GenreDto>("/genres");
  return validateGenreDto(result) as GenreDto[];
}

export async function getGenre(
  id: string,
  options?: GenreSearchOptions,
): Promise<{ genre: GenreExtendedDto; pagination: PaginationMetadata | null }> {
  const qs = toQueryParams(options as QueryParams);

  const { data, pagination } = await apiGetPaginated(`/genres/${id}${qs}`);
  const validated = validateGenreExtendedDto(data);

  return { genre: validated as GenreExtendedDto, pagination };
}
