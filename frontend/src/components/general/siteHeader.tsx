import { BaseContainer } from "./baseContainer";
import Menubar from "./menubar";

export function SiteHeader() {
  return (
    <BaseContainer className="pb-4">
      <div className="relative w-full flex md:flex-col-reverse items-center justify-between md:justify-center py-4">
        <h1 className="text-4xl font-semibold tracking-tight text-black dark:text-zinc-50">
          MovieAPI
        </h1>
        <div className="md:w-full">
          <Menubar />
        </div>
      </div>
      <p className="max-w-xl text-lg leading-8 text-zinc-600 dark:text-zinc-400">
        A small-scale IMDB clone: browse and manage movies, people, genres, and
        reviews behind a JWT-secured, role-based API.
      </p>
    </BaseContainer>
  );
}
