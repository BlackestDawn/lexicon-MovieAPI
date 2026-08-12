export interface User {
  id: string;
  name: string;
  email: string;
  role: UserRoles;
}

export type UserRoles = "User" | "PowerUser" | "Moderator" | "Administrator";

export interface AuthContextValue {
  user: User | null;
  login: (email: string, password: string) => Promise<void>;
  logout: () => Promise<void>;
}
