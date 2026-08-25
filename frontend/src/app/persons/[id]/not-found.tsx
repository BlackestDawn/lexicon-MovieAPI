import { BaseContainer } from "@/components/general/baseContainer";
import { NotFoundCard } from "@/components/general/notFoundCard";

export default function PersonNotFound() {
  return (
    <BaseContainer className="py-12">
      <NotFoundCard
        heading="Person not found"
        message="We couldn't find the person you're looking for. They may have been removed."
        linkHref="/persons"
        linkLabel="Browse persons"
      />
    </BaseContainer>
  );
}
