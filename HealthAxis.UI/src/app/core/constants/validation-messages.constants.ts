// src/app/core/constants/validation-messages.constants.ts
export const VALIDATION_MESSAGES = {
  REQUIRED: (field: string) => `${field} is required.`,
  EMAIL_INVALID: 'Please enter a valid email address.',
  PASSWORD_PATTERN:
    'Password must be at least 8 characters and include uppercase, lowercase, a number, and a special character.',
  PASSWORD_MISMATCH: 'Passwords do not match.',
  PHONE_INVALID: 'Please enter a valid 10-digit phone number.',
  NAME_INVALID: 'Name should only contain letters and spaces.',
  MIN_LENGTH: (field: string, len: number) => `${field} must be at least ${len} characters.`,
  MAX_LENGTH: (field: string, len: number) => `${field} cannot exceed ${len} characters.`,
  DOB_FUTURE: 'Date of birth cannot be in the future.',
  INVALID_CREDENTIALS: 'Invalid email or password.',
  GENERIC_ERROR: 'Something went wrong. Please try again.'
} as const;