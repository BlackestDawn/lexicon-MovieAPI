"use server";

import { cookies } from "next/headers";
import { redirect } from "next/navigation";
import { API_BASE_URL, BACKEND_URL, CLIENT_ID } from "../data/consts";
import { ApiInteractOptions, TokenResponse } from "../data/interfaces/api";

const ACCESS_TOKEN_COOKIE = "access_token";
const ACCESS_TOKEN_EXPIRY_COOKIE = "access_token_expires_at";
const REFRESH_TOKEN_COOKIE = "refresh_token";
const REFRESH_TOKEN_MAX_AGE = 60 * 60 * 24 * 7; // matches OpenIddict:RefreshTokenLifetimeDays

const baseCookieOptions = {
  httpOnly: true,
  secure: process.env.NODE_ENV === "production",
  sameSite: "lax" as const,
  path: "/",
};

export async function setAuthCookies(tokens: TokenResponse) {
  const jar = await cookies();
  const expiresAt = Math.floor(Date.now() / 1000) + tokens.expires_in;

  jar.set(ACCESS_TOKEN_COOKIE, tokens.access_token, {
    ...baseCookieOptions,
    maxAge: tokens.expires_in,
  });
  jar.set(ACCESS_TOKEN_EXPIRY_COOKIE, String(expiresAt), {
    ...baseCookieOptions,
    maxAge: tokens.expires_in,
  });
  jar.set(REFRESH_TOKEN_COOKIE, tokens.refresh_token, {
    ...baseCookieOptions,
    maxAge: REFRESH_TOKEN_MAX_AGE,
  });
}

export async function clearAuthCookies() {
  const jar = await cookies();
  jar.delete(ACCESS_TOKEN_COOKIE);
  jar.delete(ACCESS_TOKEN_EXPIRY_COOKIE);
  jar.delete(REFRESH_TOKEN_COOKIE);
}

export async function getRefreshToken() {
  const jar = await cookies();
  return jar.get(REFRESH_TOKEN_COOKIE)?.value || null;
}

function isAccessTokenExpiringSoon(expiresAt: string | undefined): boolean {
  if (!expiresAt) return true;
  const now = Math.floor(Date.now() / 1000);
  return Number(expiresAt) - now < 60; // refresh if less than 60 seconds remain
}

export async function getAccessToken(): Promise<string | null> {
  const jar = await cookies();
  const accessToken = jar.get(ACCESS_TOKEN_COOKIE)?.value;
  const expiresAt = jar.get(ACCESS_TOKEN_EXPIRY_COOKIE)?.value;

  if (accessToken && isAccessTokenExpiringSoon(expiresAt)) {
    return refreshAccessToken();
  }
  return accessToken || null;
}

export async function isAuthenticated() {
  return Boolean(await getAccessToken());
}

async function refreshAccessToken(): Promise<string | null> {
  const refreshToken = await getRefreshToken();
  if (!refreshToken) return null;

  try {
    const response = await fetch(`${BACKEND_URL}/connect/token`, {
      method: "POST",
      headers: { "Content-Type": "application/x-www-form-urlencoded" },
      body: new URLSearchParams({
        grant_type: "refresh_token",
        client_id: CLIENT_ID,
        refresh_token: refreshToken,
      }),
      cache: "no-store",
    });

    if (!response.ok) {
      await clearAuthCookies();
      return null;
    }

    const tokens: TokenResponse = await response.json();
    await setAuthCookies(tokens);
    return tokens.access_token;
  } catch (error) {
    console.error("Error refreshing access token:", error);
    return null;
  }
}

export async function login(email: string, password: string) {
  const response = await fetch(`${BACKEND_URL}/connect/token`, {
    method: "POST",
    headers: { "Content-Type": "application/x-www-form-urlencoded" },
    body: new URLSearchParams({
      grant_type: "password",
      client_id: CLIENT_ID,
      username: email,
      password,
    }),
    cache: "no-store",
  });

  if (!response.ok) {
    const error = await response.json().catch(() => ({}));
    throw new Error(error.error_description ?? "Login failed");
  }

  await setAuthCookies(await response.json());
}

export async function logout() {
  const refreshToken = await getRefreshToken();

  if (refreshToken) {
    await fetch(`${BACKEND_URL}/connect/token/revoke`, {
      method: "POST",
      headers: { "Content-Type": "application/x-www-form-urlencoded" },
      body: new URLSearchParams({ token: refreshToken, client_id: CLIENT_ID }),
      cache: "no-store",
    }).catch(() => {});
  }

  await clearAuthCookies();
}

async function parseResponse<T>(response: Response): Promise<T> {
  if (response.status === 204) return null as T;

  const contentType = response.headers.get("content-type");
  const contentLength = response.headers.get("content-length");

  if (contentLength === "0" || !contentType?.includes("application/json"))
    return null as T;

  return response.json() as T;
}

async function extractErrorMessage(response: Response): Promise<string> {
  const problem = await response.json().catch(() => null);
  const base = problem?.detail || problem?.title || `API Error: ${response.status}`;

  // ValidationProblemDetails (from [ApiController] model-binding failures, and
  // FluentValidation's ProblemDetailsFactory integration) carries the actual
  // per-field messages in `errors`, which `detail`/`title` alone don't surface.
  const fieldErrors = problem?.errors as Record<string, string[]> | undefined;
  if (fieldErrors) {
    const details = Object.entries(fieldErrors)
      .map(([field, messages]) => `${field}: ${messages.join(", ")}`)
      .join("; ");
    if (details) return `${base} - ${details}`;
  }

  return base;
}

async function apiInteract<T>(
  endpoint: string,
  options: ApiInteractOptions = {},
): Promise<T> {
  const { skipAuth = false, skipRefresh = false, ...fetchOptions } = options;

  const accessToken = skipAuth ? null : await getAccessToken();

  const headers: HeadersInit = {
    "Content-Type": "application/json",
    ...(accessToken ? { Authorization: `Bearer ${accessToken}` } : {}),
    ...fetchOptions.headers,
  };

  const url = endpoint.startsWith("http")
    ? endpoint
    : `${API_BASE_URL}${endpoint}`;

  const response = await fetch(url, {
    ...fetchOptions,
    headers,
    cache: fetchOptions.cache || "no-store",
  });

  if (response.status === 401 && !skipRefresh && !skipAuth) {
    const newAccessToken = await refreshAccessToken();
    if (!newAccessToken) {
      redirect("/login");
    }

    const retryResponse = await fetch(url, {
      ...fetchOptions,
      headers: { ...headers, Authorization: `Bearer ${newAccessToken}` },
      cache: fetchOptions.cache || "no-store",
    });

    if (!retryResponse.ok) {
      throw new Error(await extractErrorMessage(retryResponse));
    }

    return parseResponse<T>(retryResponse);
  }

  if (!response.ok) {
    throw new Error(await extractErrorMessage(response));
  }

  return parseResponse<T>(response);
}

export async function apiGet<T>(
  endpoint: string,
  options?: ApiInteractOptions,
): Promise<T> {
  return apiInteract<T>(endpoint, { ...options, method: "GET" });
}

export async function apiPost<T>(
  endpoint: string,
  data?: unknown,
  options?: ApiInteractOptions,
): Promise<T> {
  return apiInteract<T>(endpoint, {
    ...options,
    method: "POST",
    body: data ? JSON.stringify(data) : undefined,
  });
}

export async function apiPut<T>(
  endpoint: string,
  data?: unknown,
  options?: ApiInteractOptions,
): Promise<T> {
  return apiInteract<T>(endpoint, {
    ...options,
    method: "PUT",
    body: data ? JSON.stringify(data) : undefined,
  });
}

export async function apiDelete<T>(
  endpoint: string,
  options?: ApiInteractOptions,
): Promise<T> {
  return apiInteract<T>(endpoint, { ...options, method: "DELETE" });
}
