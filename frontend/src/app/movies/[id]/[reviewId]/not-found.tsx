import { BaseContainer } from "@/components/general/baseContainer";
import { NotFoundCard } from "@/components/general/notFoundCard";

export default function ReviewNotFound() {
  return (
    <BaseContainer className="py-12">
      <NotFoundCard
        heading="Review not found"
        message="We couldn't find the review you're looking for. It may have been removed."
        linkHref="/movies"
        linkLabel="Browse movies"
      />
    </BaseContainer>
  );
}
