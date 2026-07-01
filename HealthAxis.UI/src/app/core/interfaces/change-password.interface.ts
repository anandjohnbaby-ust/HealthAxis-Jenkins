// src/app/core/interfaces/change-password.interface.ts

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}