"use server";

import type {
  CurrentUserDto,
  User,
  UserRoles,
} from "../data/models/userTypes";
import { apiGet, isAuthenticated, login } from "./apiInteract";

function toUser(dto: CurrentUserDto): User {
  return {
    id: dto.id,
    email: dto.email,
    name: dto.email.split("@")[0],
    role: dto.role as UserRoles,
  };
}

export async function loginRequest(
  email: string,
  password: string,
): Promise<User> {
  await login(email, password);

  const user = await fetchCurrentUser();
  if (!user) {
    throw new Error("Login succeeded but the user profile could not be loaded");
  }

  return user;
}

export async function fetchCurrentUser(): Promise<User | null> {
  if (!(await isAuthenticated())) return null;

  const dto = await apiGet<CurrentUserDto>("/auth/me");
  return toUser(dto);
}
