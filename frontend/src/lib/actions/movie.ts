"use server";

import { QueryParams } from "../data/interfaces/general";
import { MovieSearchOptions } from "../data/interfaces/movie";
import {
  MovieDto,
  MovieExtendedDto,
  validateMovieDto,
  validateMovieExtendedDto,
} from "../data/models/movieTypes";
import { toQueryParams } from "../data/utils/converters";
import { apiGet } from "./apiInteract";

export async function fetchMovies(
  options?: MovieSearchOptions,
): Promise<MovieDto[]> {
  const qs = toQueryParams(options as QueryParams);
  const url = `/movies${qs ? "?" + qs : ""}`;

  const result = await apiGet<MovieDto[]>(url);
  const validated = validateMovieDto(result);

  return validated as MovieDto[];
}

export async function getMovie(
  id: string,
  includePeople?: boolean,
): Promise<MovieExtendedDto> {
  const qs = toQueryParams({ includePeople });
  const url = `/movies/${id}${qs ? "?" + qs : ""}`;

  const result = await apiGet<MovieExtendedDto>(url);
  const validated = validateMovieExtendedDto(result);

  return validated as MovieExtendedDto;
}
