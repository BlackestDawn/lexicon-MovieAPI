import { ReactNode } from "react";

interface BaseContainerProps {
  children: ReactNode,
  className?: string
}

export function BaseContainer({ children, className }: BaseContainerProps) {
  return (
    <div className={`max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 ${className}`}>
      {children}
    </div>
  );
}