import { Menu, X } from "lucide-react";

interface Props {
  isOpen: boolean;
  onClick: () => void;
}

export default function HamburgerButton({ isOpen, onClick }: Props) {
  return (
    <button
      onClick={onClick}
      className="md:hidden inline-flex items-center justify-center p-2 rounded-lg text-foreground hover:bg-background focus:outline-none focus:ring-2 focus:ring-inset focus:ring-primary transition-colors duration-200"
      aria-label="Toggle menu"
      aria-expanded={isOpen}
    >
      <span className="sr-only">Toggle main menu</span>
      {isOpen ? <X className="h-5 w-5" /> : <Menu className="h-5 w-5" />}
    </button>
  );
}
