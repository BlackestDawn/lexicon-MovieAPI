"use client";

import { logout as logoutRequest } from "@/lib/actions/apiInteract";
import {
  loginRequest,
  registerRequest,
  updateProfileRequest,
} from "@/lib/actions/auth";
import type { AccessLevel, AuthContextValue } from "@/lib/data/interfaces/auth";
import { ValidationError } from "@/lib/data/interfaces/errors";
import {
  type UserRoles,
  userRoles,
  type User,
} from "@/lib/data/models/userTypes";
import { createContext, ReactNode, useContext, useState } from "react";

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

interface Props {
  initialUser: User | null;
  children: ReactNode;
}

export default function CommonContext({ children, initialUser }: Props) {
  const [user, setUser] = useState<User | null>(initialUser);

  const login = async (email: string, password: string) => {
    const user = await loginRequest(email, password);
    setUser(user);
  };

  const register = async (
    email: string,
    password: string,
    displayName?: string,
  ) => {
    const result = await registerRequest({ email, password, displayName });
    if (!result.success) {
      if (result.issues && result.issues.length > 0) {
        throw new ValidationError(result.error, result.issues);
      }
      throw new Error(result.error);
    }
    setUser(result.user);
  };

  const updateProfile = async (email: string, displayName?: string) => {
    const formData = new FormData();
    formData.set("email", email);
    if (displayName) formData.set("displayName", displayName);

    const result = await updateProfileRequest(formData);
    if (!result.success) {
      if (result.issues && result.issues.length > 0) {
        throw new ValidationError(result.error, result.issues);
      }
      throw new Error(result.error);
    }
    setUser(result.user);
  };

  const logout = async () => {
    await logoutRequest();
    setUser(null);
  };

  const rank = (role: UserRoles) => userRoles.options.indexOf(role);

  const hasAccess = (level: AccessLevel): boolean => {
    if (!user) return false;

    switch (level) {
      case "LoggedIn":
        return true;
      case "PowerUserAndAbove":
        return rank(user.role) >= rank("PowerUser");
      case "ModeratorAndAbove":
        return rank(user.role) >= rank("Moderator");
      default:
        return user.role === level;
    }
  };

  return (
    <AuthContext.Provider
      value={{ user, hasAccess, login, register, updateProfile, logout }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined)
    throw new Error("useAuth must be used within an AuthProvider");

  return context;
}
