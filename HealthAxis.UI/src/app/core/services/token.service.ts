  // src/app/core/services/token.service.ts
  import { Injectable } from '@angular/core';
  import { STORAGE_KEYS } from '../constants/storage-keys.constants';
  import { JwtPayload } from '../interfaces/jwt-payload.interface';
  import { Role } from '../enums/role.enum'; // Ensure this matches your enum path

  @Injectable({ providedIn: 'root' })
  export class TokenService {
    private readonly ROLE_CLAIM_URI = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
    private readonly NAME_ID_CLAIM_URI = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier';

    // ---------------------------------------------------------------------------
    // Storage & Management
    // ---------------------------------------------------------------------------

    setTokens(accessToken: string, refreshToken: string): void {
      localStorage.setItem(STORAGE_KEYS.ACCESS_TOKEN, accessToken);
      localStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, refreshToken);
    }

    getAccessToken(): string | null {
      return localStorage.getItem(STORAGE_KEYS.ACCESS_TOKEN);
    }

    getRefreshToken(): string | null {
      return localStorage.getItem(STORAGE_KEYS.REFRESH_TOKEN);
    }

    clearTokens(): void {
      localStorage.removeItem(STORAGE_KEYS.ACCESS_TOKEN);
      localStorage.removeItem(STORAGE_KEYS.REFRESH_TOKEN);
      localStorage.removeItem(STORAGE_KEYS.USER_ROLE);
    }

    // ---------------------------------------------------------------------------
    // Validation & Decoding
    // ---------------------------------------------------------------------------

    isLoggedIn(): boolean {
      const token = this.getAccessToken();
      return !!token && !this.isTokenExpired(token);
    }

    isTokenExpired(token: string | null): boolean {
      if (!token) return true;

      const payload = this.decodeToken(token);
      if (!payload?.exp) return true;

      // JWT exp is in seconds, Date.now() is in milliseconds
      return Date.now() >= payload.exp * 1000;
    }

    decodeToken(token: string): JwtPayload | null {
      try {
        const base64Payload = token.split('.')[1];
        const decoded = atob(
          base64Payload.replaceAll('-', '+').replaceAll('_', '/')
        );
        return JSON.parse(decoded) as JwtPayload;
      } catch {
        return null;
      }
    }

    // ---------------------------------------------------------------------------
    // Claim Extraction (Defaults to current Access Token if none is provided)
    // ---------------------------------------------------------------------------

    getRoleFromToken(token: string | null = this.getAccessToken()): Role | null {
      if (!token) return null;
      const payload = this.decodeToken(token);
      if (!payload) return null;

      const roleStr = (payload[this.ROLE_CLAIM_URI] as string) ?? (payload['role'] as string) ?? null;
      return roleStr as Role | null;
    }

    getEmailFromToken(token: string | null = this.getAccessToken()): string | null {
      if (!token) return null;
      const payload = this.decodeToken(token);
      return payload ? (payload['email'] as string ?? null) : null;
    }

    getUserIdFromToken(token: string | null = this.getAccessToken()): string | null {
      if (!token) return null;
      const payload = this.decodeToken(token);
      if (!payload) return null;

      return (payload['sub'] as string) ?? (payload[this.NAME_ID_CLAIM_URI] as string) ?? null;
    }

    getPatientIdFromToken(token: string | null = this.getAccessToken()): number | null {
      if (!token) return null;
      const payload = this.decodeToken(token);
      
      // Check both standard casing and PascalCasing just in case C# serializes it differently
      const claimValue = payload ? (payload['patientId'] ?? payload['PatientId']) : null;
      if (claimValue == null) return null;

      const id = Number(claimValue);
      return Number.isNaN(id) ? null : id;
    }

    getDoctorIdFromToken(token: string | null = this.getAccessToken()): number | null {
      if (!token) return null;
      const payload = this.decodeToken(token);
      
      const claimValue = payload ? (payload['doctorId'] ?? payload['DoctorId']) : null;
      if (claimValue == null) return null;

      const id = Number(claimValue);
      return Number.isNaN(id) ? null : id;
    }

    getFullNameFromToken(token: string | null = this.getAccessToken()): string | null {
      if (!token) return null;
      const payload = this.decodeToken(token);
      return payload ? ((payload['fullName'] ?? payload['FullName']) as string ?? null) : null;
    }

    getUserInitialFromToken(token: string | null = this.getAccessToken()): string {
      const fullName = this.getFullNameFromToken(token);
      if (!fullName || fullName.trim().length === 0) {
        return '?';
      }
      return fullName.trim().charAt(0).toUpperCase();
    }
  }