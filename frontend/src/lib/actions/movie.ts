"use server";

import { MovieSearchOptions } from "../data/interfaces/movie";
import { MovieDto, validateMovieDto } from "../data/models/movieTypes";
import { toQueryParams } from "../data/utils/converters";
import { apiGet } from "./apiInteract";

export async function fetchMovies(options?: MovieSearchOptions): Promise<MovieDto[]> {
  const qs = toQueryParams(
    options as Record<string, string | number | undefined>,
  );
  const url = `/movies?${qs}`;

  const result = await apiGet<MovieDto[]>(url);
  const validated = validateMovieDto(result);

  return validated as MovieDto[];
}
