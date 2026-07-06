import { ReactNode } from "react";

interface BaseContainerProps {
  children: ReactNode
}

export function BaseContainer({ children }: BaseContainerProps) {
  return (
    <div className="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      {children}
    </div>
  );
}