import { BaseContainer } from "@/components/general/baseContainer";
import { NotFoundCard } from "@/components/general/notFoundCard";

export default function NotFound() {
  return (
    <BaseContainer className="py-12">
      <NotFoundCard
        heading="Page not found"
        message="The page you're looking for doesn't exist or may have been moved."
        linkHref="/"
        linkLabel="Go home"
      />
    </BaseContainer>
  );
}
