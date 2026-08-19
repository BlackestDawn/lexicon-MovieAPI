import { afterEach, describe, expect, it } from "vitest";
import { NextRequest } from "next/server";
import { proxy } from "./proxy";

describe("proxy", () => {
  const originalBackendUrl = process.env.BACKEND_URL;

  afterEach(() => {
    process.env.BACKEND_URL = originalBackendUrl;
  });

  it("rewrites the request to BACKEND_URL, preserving path and query", () => {
    process.env.BACKEND_URL = "https://api.example.com";
    const request = new NextRequest(
      "https://frontend.example.com/api/genres?page=2",
    );

    const response = proxy(request);

    expect(response.headers.get("x-middleware-rewrite")).toBe(
      "https://api.example.com/api/genres?page=2",
    );
  });

  it("reads BACKEND_URL fresh on every call rather than at import time", () => {
    process.env.BACKEND_URL = "https://staging.example.com";
    const staging = proxy(new NextRequest("https://frontend.example.com/api/genres"));
    expect(staging.headers.get("x-middleware-rewrite")).toBe(
      "https://staging.example.com/api/genres",
    );

    process.env.BACKEND_URL = "https://prod.example.com";
    const prod = proxy(new NextRequest("https://frontend.example.com/api/genres"));
    expect(prod.headers.get("x-middleware-rewrite")).toBe(
      "https://prod.example.com/api/genres",
    );
  });
});
