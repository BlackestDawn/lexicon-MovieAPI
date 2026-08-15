// API interaction
export const BACKEND_URL = process.env.BACKEND_URL || "https://localhost:7105"
export const API_BASE_URL = `${BACKEND_URL}/api/v2`;
export const CLIENT_ID = "movieapi-client";

// Pagination
export const defaultPage = 1;
export const defaultPageSize = 10;

// General values
export const defaultBudget = 1000000;
export const defaultRuntime = 120;
