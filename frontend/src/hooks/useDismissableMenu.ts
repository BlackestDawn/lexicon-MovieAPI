import { useEffect, useRef, useState } from "react";

export function useDismissableMenu<T extends HTMLElement = HTMLElement>() {
  const [isOpen, setIsOpen] = useState<boolean>(false);
  const menuRef = useRef<T>(null);

  const toggle = () => setIsOpen(!isOpen);
  const close = () => setIsOpen(false);

  useEffect(() => {
    if (!isOpen) return;

    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === "Escape") close();
    };

    // mousedown (not click) so this fires before the trigger button's own
    // click handler - otherwise closing here and toggling open again from
    // the same click race and the menu never actually closes.
    const handleClickOutside = (event: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(event.target as Node)) {
        close();
      }
    };

    document.addEventListener("keydown", handleKeyDown);
    document.addEventListener("mousedown", handleClickOutside);
    return () => {
      document.removeEventListener("keydown", handleKeyDown);
      document.removeEventListener("mousedown", handleClickOutside);
    };
  }, [isOpen]);

  return { isOpen, toggle, close, menuRef };
}
