// Shared constants for mocking next/navigation's notFound()/redirect().
//
// These are plain data — safe to import anywhere, including into test
// assertions. They are NOT safe to import *into* a vi.mock("next/navigation", …)
// factory: Vitest hoists vi.mock calls above all other imports in the file,
// so a factory that closes over an externally-imported function/value throws
// a "Cannot access '<binding>' before initialization" error at runtime. Each
// test file's factory must therefore inline its own throw using these same
// message strings, e.g.:
//
//   vi.mock("next/navigation", () => ({
//     notFound: vi.fn(() => { throw new Error(NEXT_NOT_FOUND_MESSAGE); }),
//     redirect: vi.fn((url: string) => { throw new Error(nextRedirectMessage(url)); }),
//   }));
//
// ...then assert with `.rejects.toThrow(NEXT_NOT_FOUND_MESSAGE)` outside the
// factory, where importing normally is fine.
export const NEXT_NOT_FOUND_MESSAGE = "NEXT_NOT_FOUND";

export function nextRedirectMessage(url: string): string {
  return `NEXT_REDIRECT:${url}`;
}
