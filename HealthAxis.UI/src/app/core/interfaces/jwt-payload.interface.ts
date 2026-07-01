export interface JwtPayload {
  exp?: number;

  sub?: string;

  email?: string;

  role?: string;

  patientId?: number;

  doctorId?: number;

  fullName?: string;

  [key: string]: unknown;
}