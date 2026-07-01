// src/app/core/interfaces/api-response.interface.ts
export interface ApiErrorResponse {
  message: string;
  errors?: Record<string, string[]>;
  statusCode?: number;
}