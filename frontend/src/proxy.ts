import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";

// Replaces next.config.ts's old rewrites()-based proxy: that config function
// runs once at `next build` time and its result is baked verbatim into
// .next/routes-manifest.json, so BACKEND_URL couldn't be overridden per
// environment without rebuilding. Proxy runs the Node.js runtime on every
// matched request in the live server process, so process.env.BACKEND_URL is
// read fresh from whatever the container's environment holds at boot -
// letting the same built image serve staging and prod with different values.
export function proxy(request: NextRequest) {
  const backendUrl = process.env.BACKEND_URL;

  return NextResponse.rewrite(
    new URL(request.nextUrl.pathname + request.nextUrl.search, backendUrl),
  );
}

export const config = {
  matcher: "/api/:path*",
};
