export interface ApiInteractOptions extends RequestInit {
  skipAuth?: boolean;
  skipRefresh?: boolean;
}

export interface TokenResponse {
  access_token: string;
  refresh_token: string;
  expires_in: number;
}
