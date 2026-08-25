import { isFavorited } from "@/lib/actions/favorite";
import FavoriteButton from "./favoriteButton";

// Split out from FavoriteButton so the isFavorited() lookup (which needs an
// authenticated caller) only ever runs as a child of a RestrictedComponent/
// RestrictedPage guard - those guards render nothing at all when the visitor
// isn't logged in, so an unrendered child's fetch never actually fires.
export default async function FavoriteToggle({ movieId }: { movieId: string }) {
  const favorited = await isFavorited(movieId);
  return <FavoriteButton movieId={movieId} initiallyFavorited={favorited} />;
}
