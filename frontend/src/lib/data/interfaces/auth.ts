import { User } from "../models/userTypes";

export interface AuthContextValue {
  user: User | null;
  hasAccess: (level: AccessLevel) => boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (
    email: string,
    password: string,
    displayName?: string,
  ) => Promise<void>;
  logout: () => Promise<void>;
}

export type AccessLevel =
  | "User"
  | "PowerUser"
  | "Moderator"
  | "Administrator"
  | "PowerUserAndAbove"
  | "ModeratorAndAbove"
  | "LoggedIn";
