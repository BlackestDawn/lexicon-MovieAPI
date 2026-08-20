import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import { SiteHeader } from "./siteHeader";

vi.mock("./mobileNav", () => ({
  default: () => <div>mobile-nav</div>,
}));
vi.mock("./accountMenu", () => ({
  AccountMenu: () => <div>account-menu</div>,
}));

describe("SiteHeader", () => {
  it("renders both the mobile nav and the account menu", () => {
    render(<SiteHeader />);
    expect(screen.getByText("mobile-nav")).toBeInTheDocument();
    expect(screen.getByText("account-menu")).toBeInTheDocument();
  });
});
