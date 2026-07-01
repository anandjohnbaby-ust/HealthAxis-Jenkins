// src/app/core/interfaces/auth.interface.ts
import { Gender } from '../enums/gender.enum';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  message: string;
  expiresIn: number;
}

export interface RegisterRequest {
  fullName: string;
  dateOfBirth: string; // yyyy-MM-dd
  gender: Gender;
  phoneNumber: string;
  email: string;
  password: string;
  confirmPassword: string;
}

export interface AuthenticatedUser {
  id: string;
  email: string;
  role: string;
}