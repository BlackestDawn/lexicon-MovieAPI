import { BaseContainer } from "@/components/general/baseContainer";
import { NotFoundCard } from "@/components/general/notFoundCard";

export default function GenreNotFound() {
  return (
    <BaseContainer className="py-12">
      <NotFoundCard
        heading="Genre not found"
        message="We couldn't find the genre you're looking for. It may have been removed."
        linkHref="/genres"
        linkLabel="Browse genres"
      />
    </BaseContainer>
  );
}
