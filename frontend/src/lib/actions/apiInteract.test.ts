import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

const { cookies, redirect } = vi.hoisted(() => ({
  cookies: vi.fn(),
  redirect: vi.fn((url: string) => {
    throw new Error(`NEXT_REDIRECT:${url}`);
  }),
}));

vi.mock("next/headers", () => ({ cookies }));
vi.mock("next/navigation", () => ({ redirect }));

const {
  setAuthCookies,
  clearAuthCookies,
  getRefreshToken,
  getAccessToken,
  isAuthenticated,
  login,
  logout,
  apiGet,
  apiPost,
  apiPut,
  apiDelete,
  apiGetPaginated,
} = await import("./apiInteract");

function createCookieJar(initial: Record<string, string> = {}) {
  const store = new Map(Object.entries(initial));
  return {
    get: vi.fn((name: string) =>
      store.has(name) ? { name, value: store.get(name)! } : undefined,
    ),
    set: vi.fn((name: string, value: string) => {
      store.set(name, value);
    }),
    delete: vi.fn((name: string) => {
      store.delete(name);
    }),
    _store: store,
  };
}

function fakeResponse({
  ok = true,
  status = 200,
  headers = {},
  jsonBody,
}: {
  ok?: boolean;
  status?: number;
  headers?: Record<string, string>;
  jsonBody?: unknown;
} = {}) {
  return {
    ok,
    status,
    headers: { get: (name: string) => headers[name] ?? null },
    json: vi.fn().mockResolvedValue(jsonBody),
  } as unknown as Response;
}

let jar: ReturnType<typeof createCookieJar>;

beforeEach(() => {
  jar = createCookieJar();
  cookies.mockResolvedValue(jar);
  vi.stubGlobal("fetch", vi.fn());
});

afterEach(() => {
  vi.unstubAllGlobals();
});

describe("setAuthCookies / clearAuthCookies", () => {
  it("writes all three auth cookies with the expected values", async () => {
    await setAuthCookies({
      access_token: "access-1",
      refresh_token: "refresh-1",
      expires_in: 3600,
    });

    expect(jar.set).toHaveBeenCalledWith(
      "access_token",
      "access-1",
      expect.objectContaining({ maxAge: 3600 }),
    );
    expect(jar.set).toHaveBeenCalledWith(
      "refresh_token",
      "refresh-1",
      expect.objectContaining({ maxAge: 60 * 60 * 24 * 7 }),
    );
    expect(jar.set).toHaveBeenCalledWith(
      "access_token_expires_at",
      expect.any(String),
      expect.objectContaining({ maxAge: 3600 }),
    );
  });

  it("deletes all three auth cookies", async () => {
    await clearAuthCookies();
    expect(jar.delete).toHaveBeenCalledWith("access_token");
    expect(jar.delete).toHaveBeenCalledWith("access_token_expires_at");
    expect(jar.delete).toHaveBeenCalledWith("refresh_token");
  });
});

describe("getRefreshToken", () => {
  it("returns the cookie value when present", async () => {
    jar = createCookieJar({ refresh_token: "refresh-1" });
    cookies.mockResolvedValue(jar);
    await expect(getRefreshToken()).resolves.toBe("refresh-1");
  });

  it("returns null when absent", async () => {
    await expect(getRefreshToken()).resolves.toBeNull();
  });
});

describe("getAccessToken", () => {
  it("returns the cookie value as-is when it isn't close to expiring", async () => {
    const farFuture = Math.floor(Date.now() / 1000) + 3600;
    jar = createCookieJar({
      access_token: "access-1",
      access_token_expires_at: String(farFuture),
    });
    cookies.mockResolvedValue(jar);

    await expect(getAccessToken()).resolves.toBe("access-1");
    expect(fetch).not.toHaveBeenCalled();
  });

  it("refreshes when the token is missing an expiry", async () => {
    jar = createCookieJar({ access_token: "access-1", refresh_token: "refresh-1" });
    cookies.mockResolvedValue(jar);
    vi.mocked(fetch).mockResolvedValue(
      fakeResponse({
        jsonBody: { access_token: "new-access", refresh_token: "new-refresh", expires_in: 3600 },
      }),
    );

    await expect(getAccessToken()).resolves.toBe("new-access");
    expect(jar.set).toHaveBeenCalledWith(
      "access_token",
      "new-access",
      expect.anything(),
    );
  });

  it("returns null when there is no refresh token to fall back on", async () => {
    await expect(getAccessToken()).resolves.toBeNull();
  });
});

describe("isAuthenticated", () => {
  it("is true when an access token is available", async () => {
    jar = createCookieJar({
      access_token: "access-1",
      access_token_expires_at: String(Math.floor(Date.now() / 1000) + 3600),
    });
    cookies.mockResolvedValue(jar);
    await expect(isAuthenticated()).resolves.toBe(true);
  });

  it("is false with no session", async () => {
    await expect(isAuthenticated()).resolves.toBe(false);
  });
});

describe("login", () => {
  it("stores the returned tokens as cookies on success", async () => {
    vi.mocked(fetch).mockResolvedValue(
      fakeResponse({
        jsonBody: { access_token: "access-1", refresh_token: "refresh-1", expires_in: 3600 },
      }),
    );

    await login("a@example.com", "Abcdef1!");

    expect(jar.set).toHaveBeenCalledWith("access_token", "access-1", expect.anything());
  });

  it("throws the API's error_description on failure", async () => {
    vi.mocked(fetch).mockResolvedValue(
      fakeResponse({ ok: false, jsonBody: { error_description: "Invalid credentials" } }),
    );

    await expect(login("a@example.com", "wrong")).rejects.toThrow(
      "Invalid credentials",
    );
  });
});

describe("logout", () => {
  it("revokes the refresh token and clears cookies when one exists", async () => {
    jar = createCookieJar({ refresh_token: "refresh-1" });
    cookies.mockResolvedValue(jar);
    vi.mocked(fetch).mockResolvedValue(fakeResponse());

    await logout();

    expect(fetch).toHaveBeenCalledWith(
      expect.stringContaining("/connect/token/revoke"),
      expect.objectContaining({ method: "POST" }),
    );
    expect(jar.delete).toHaveBeenCalledWith("refresh_token");
  });

  it("still clears cookies when there is no refresh token", async () => {
    await logout();
    expect(fetch).not.toHaveBeenCalled();
    expect(jar.delete).toHaveBeenCalledWith("refresh_token");
  });

  it("clears cookies even if the revoke request fails", async () => {
    jar = createCookieJar({ refresh_token: "refresh-1" });
    cookies.mockResolvedValue(jar);
    vi.mocked(fetch).mockRejectedValue(new Error("network down"));

    await expect(logout()).resolves.toBeUndefined();
    expect(jar.delete).toHaveBeenCalledWith("refresh_token");
  });
});

describe("apiGet / apiPost / apiPut / apiDelete", () => {
  beforeEach(() => {
    jar = createCookieJar({
      access_token: "access-1",
      access_token_expires_at: String(Math.floor(Date.now() / 1000) + 3600),
    });
    cookies.mockResolvedValue(jar);
  });

  it("attaches the bearer token and parses a JSON body", async () => {
    vi.mocked(fetch).mockResolvedValue(
      fakeResponse({ headers: { "content-type": "application/json" }, jsonBody: { id: 1 } }),
    );

    const result = await apiGet("/genres");

    expect(fetch).toHaveBeenCalledWith(
      expect.stringContaining("/genres"),
      expect.objectContaining({
        headers: expect.objectContaining({ Authorization: "Bearer access-1" }),
      }),
    );
    expect(result).toEqual({ id: 1 });
  });

  it("returns null for a 204 response", async () => {
    vi.mocked(fetch).mockResolvedValue(fakeResponse({ status: 204 }));
    await expect(apiDelete("/genres/1")).resolves.toBeNull();
  });

  it("returns null when the response isn't JSON", async () => {
    vi.mocked(fetch).mockResolvedValue(
      fakeResponse({ headers: { "content-type": "text/plain" } }),
    );
    await expect(apiGet("/genres")).resolves.toBeNull();
  });

  it("serializes the body as JSON for POST/PUT", async () => {
    vi.mocked(fetch).mockResolvedValue(fakeResponse({ status: 204 }));

    await apiPost("/genres", { name: "Action" });

    expect(fetch).toHaveBeenCalledWith(
      expect.any(String),
      expect.objectContaining({ method: "POST", body: JSON.stringify({ name: "Action" }) }),
    );

    await apiPut("/genres/1", { name: "Action" });
    expect(fetch).toHaveBeenCalledWith(
      expect.any(String),
      expect.objectContaining({ method: "PUT", body: JSON.stringify({ name: "Action" }) }),
    );
  });

  it("throws a combined message for a validation problem response", async () => {
    vi.mocked(fetch).mockResolvedValue(
      fakeResponse({
        ok: false,
        status: 400,
        jsonBody: {
          title: "One or more validation errors occurred.",
          errors: { Name: ["Name is required"] },
        },
      }),
    );

    await expect(apiGet("/genres")).rejects.toThrow(
      "One or more validation errors occurred. - Name: Name is required",
    );
  });

  it("refreshes the access token and retries once on a 401", async () => {
    jar = createCookieJar({
      access_token: "expiring-soon",
      access_token_expires_at: String(Math.floor(Date.now() / 1000) + 3600),
      refresh_token: "refresh-1",
    });
    cookies.mockResolvedValue(jar);

    vi.mocked(fetch)
      .mockResolvedValueOnce(fakeResponse({ ok: false, status: 401, jsonBody: {} }))
      .mockResolvedValueOnce(
        fakeResponse({
          jsonBody: { access_token: "new-access", refresh_token: "new-refresh", expires_in: 3600 },
        }),
      )
      .mockResolvedValueOnce(
        fakeResponse({ headers: { "content-type": "application/json" }, jsonBody: { id: 1 } }),
      );

    const result = await apiGet("/genres");

    expect(result).toEqual({ id: 1 });
    const lastCall = vi.mocked(fetch).mock.calls.at(-1);
    expect(lastCall?.[1]).toMatchObject({
      headers: expect.objectContaining({ Authorization: "Bearer new-access" }),
    });
  });

  it("redirects to /login when the refresh token can't be renewed after a 401", async () => {
    jar = createCookieJar({
      access_token: "expiring-soon",
      access_token_expires_at: String(Math.floor(Date.now() / 1000) + 3600),
    });
    cookies.mockResolvedValue(jar);

    vi.mocked(fetch).mockResolvedValueOnce(
      fakeResponse({ ok: false, status: 401, jsonBody: {} }),
    );

    await expect(apiGet("/genres")).rejects.toThrow("NEXT_REDIRECT:/login");
    expect(redirect).toHaveBeenCalledWith("/login");
  });
});

describe("apiGetPaginated", () => {
  beforeEach(() => {
    jar = createCookieJar({
      access_token: "access-1",
      access_token_expires_at: String(Math.floor(Date.now() / 1000) + 3600),
    });
    cookies.mockResolvedValue(jar);
  });

  it("returns the body and the parsed X-Pagination header", async () => {
    vi.mocked(fetch).mockResolvedValue(
      fakeResponse({
        headers: {
          "content-type": "application/json",
          "X-Pagination": JSON.stringify({
            TotalItemCount: 1,
            TotalPageCount: 1,
            PageSize: 10,
            CurrentPage: 1,
          }),
        },
        jsonBody: [{ id: 1 }],
      }),
    );

    const result = await apiGetPaginated("/genres");

    expect(result.data).toEqual([{ id: 1 }]);
    expect(result.pagination?.TotalItemCount).toBe(1);
  });

  it("returns null pagination when the header is absent", async () => {
    vi.mocked(fetch).mockResolvedValue(
      fakeResponse({ headers: { "content-type": "application/json" }, jsonBody: [] }),
    );

    const result = await apiGetPaginated("/genres");
    expect(result.pagination).toBeNull();
  });
});
