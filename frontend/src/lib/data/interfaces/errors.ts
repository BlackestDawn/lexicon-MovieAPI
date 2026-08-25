export class ValidationError extends Error {
  issues: string[];

  constructor(message: string, issues: string[]) {
    super(message);
    this.name = "ValidationError";
    this.issues = issues;
    Object.setPrototypeOf(this, ValidationError.prototype);
  }
}

export class ApiError extends Error {
  status: number;

  constructor(message: string, status: number) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    Object.setPrototypeOf(this, ApiError.prototype);
  }
}
