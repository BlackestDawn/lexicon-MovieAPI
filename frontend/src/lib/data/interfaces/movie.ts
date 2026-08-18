export interface MovieSearchOptions {
  name?: string;
  search?: string;
  genre?: string;
  year?: number;
  minRating?: number;
  maxRating?: number;
  page?: number;
  pageSize?: number;
}
