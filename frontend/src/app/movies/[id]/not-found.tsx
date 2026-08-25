import { BaseContainer } from "@/components/general/baseContainer";
import { NotFoundCard } from "@/components/general/notFoundCard";

export default function MovieNotFound() {
  return (
    <BaseContainer className="py-12">
      <NotFoundCard
        heading="Movie not found"
        message="We couldn't find the movie you're looking for. It may have been removed."
        linkHref="/movies"
        linkLabel="Browse movies"
      />
    </BaseContainer>
  );
}
