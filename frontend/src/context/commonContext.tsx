"use client";

import { logout as logoutRequest } from "@/lib/actions/apiInteract";
import { loginRequest } from "@/lib/actions/auth";
import type { AuthContextValue } from "@/lib/data/interfaces/auth";
import type { User } from "@/lib/data/models/userTypes";
import {
  createContext,
  ReactNode,
  useContext,
  useState,
} from "react";

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

  const logout = async () => {
    await logoutRequest();
    setUser(null);
  };

  return (
    <AuthContext.Provider value={{ user, login, logout }}>
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
