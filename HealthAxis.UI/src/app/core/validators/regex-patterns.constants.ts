// src/app/core/validators/regex-patterns.constants.ts
export const REGEX_PATTERNS = {
  EMAIL: /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/,
  // Min 8 chars, 1 uppercase, 1 lowercase, 1 number, 1 special char
  PASSWORD: /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$/,
  PHONE: /^[6-9]\d{9}$/,
  NAME: /^[a-zA-Z\s]{2,50}$/
} as const;