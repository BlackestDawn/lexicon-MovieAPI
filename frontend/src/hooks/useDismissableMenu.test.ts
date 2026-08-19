import { describe, expect, it } from "vitest";
import { act, renderHook } from "@testing-library/react";
import { useDismissableMenu } from "./useDismissableMenu";

describe("useDismissableMenu", () => {
  it("starts closed", () => {
    const { result } = renderHook(() => useDismissableMenu());
    expect(result.current.isOpen).toBe(false);
  });

  it("toggle flips the open state", () => {
    const { result } = renderHook(() => useDismissableMenu());

    act(() => result.current.toggle());
    expect(result.current.isOpen).toBe(true);

    act(() => result.current.toggle());
    expect(result.current.isOpen).toBe(false);
  });

  it("close sets isOpen to false", () => {
    const { result } = renderHook(() => useDismissableMenu());

    act(() => result.current.toggle());
    expect(result.current.isOpen).toBe(true);

    act(() => result.current.close());
    expect(result.current.isOpen).toBe(false);
  });

  it("closes on an Escape keydown while open", () => {
    const { result } = renderHook(() => useDismissableMenu());
    act(() => result.current.toggle());

    act(() => {
      document.dispatchEvent(new KeyboardEvent("keydown", { key: "Escape" }));
    });

    expect(result.current.isOpen).toBe(false);
  });

  it("closes on a mousedown outside the referenced element", () => {
    const outside = document.createElement("div");
    document.body.appendChild(outside);

    const { result } = renderHook(() => useDismissableMenu<HTMLDivElement>());
    const menuEl = document.createElement("div");
    document.body.appendChild(menuEl);
    // The hook attaches its ref via `.current`; simulate that assignment.
    act(() => {
      (result.current.menuRef as { current: HTMLDivElement | null }).current = menuEl;
      result.current.toggle();
    });

    act(() => {
      outside.dispatchEvent(new MouseEvent("mousedown", { bubbles: true }));
    });

    expect(result.current.isOpen).toBe(false);

    document.body.removeChild(outside);
    document.body.removeChild(menuEl);
  });

  it("does not close on a mousedown inside the referenced element", () => {
    const { result } = renderHook(() => useDismissableMenu<HTMLDivElement>());
    const menuEl = document.createElement("div");
    document.body.appendChild(menuEl);
    act(() => {
      (result.current.menuRef as { current: HTMLDivElement | null }).current = menuEl;
      result.current.toggle();
    });

    act(() => {
      menuEl.dispatchEvent(new MouseEvent("mousedown", { bubbles: true }));
    });

    expect(result.current.isOpen).toBe(true);

    document.body.removeChild(menuEl);
  });
});
