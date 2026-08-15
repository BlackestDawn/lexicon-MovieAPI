import { GenreDto, validateGenreDto } from "../data/models/genreTypes";
import { apiGet } from "./apiInteract";

export async function fetchGenres(): Promise<GenreDto[]> {
  const result = await apiGet<GenreDto>("/genres");
  return validateGenreDto(result) as GenreDto[];
}
