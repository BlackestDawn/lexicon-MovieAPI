export interface MenuOption {
  label: string;
  href: string;
  action?: () => void;
}

export type QueryValue =
  | string
  | number
  | boolean
  | Date
  | QueryValue[]
  | null
  | undefined;
export type QueryParams = Record<string, unknown>;
