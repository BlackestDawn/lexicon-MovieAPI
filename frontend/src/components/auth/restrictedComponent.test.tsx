import { describe, expect, it } from "vitest";
import { render, screen } from "@testing-library/react";
import RestrictedComponent from "./restrictedComponent";
import CommonContext from "@/context/commonContext";
import { User } from "@/lib/data/models/userTypes";

function renderAs(user: User | null, accessLevel: Parameters<typeof RestrictedComponent>[0]["accessLevel"], id?: string) {
  return render(
    <CommonContext initialUser={user}>
      <RestrictedComponent accessLevel={accessLevel} id={id}>
        <span>secret content</span>
      </RestrictedComponent>
    </CommonContext>,
  );
}

const moderator: User = {
  id: "9c858901-8a57-4791-81fe-4c455b099bc9",
  name: "Mod",
  email: "mod@example.com",
  role: "Moderator",
};

const regularUser: User = {
  id: "0c858901-8a57-4791-81fe-4c455b099bc9",
  name: "Regular",
  email: "user@example.com",
  role: "User",
};

describe("RestrictedComponent", () => {
  it("renders nothing when there is no user", () => {
    renderAs(null, "LoggedIn");
    expect(screen.queryByText("secret content")).not.toBeInTheDocument();
  });

  it("renders children when the user meets a ranked access level", () => {
    renderAs(moderator, "ModeratorAndAbove");
    expect(screen.getByText("secret content")).toBeInTheDocument();
  });

  it("hides children when the user is below a ranked access level", () => {
    renderAs(regularUser, "ModeratorAndAbove");
    expect(screen.queryByText("secret content")).not.toBeInTheDocument();
  });

  it("renders children for the resource owner regardless of role", () => {
    renderAs(regularUser, "Administrator", regularUser.id);
    expect(screen.getByText("secret content")).toBeInTheDocument();
  });
});
