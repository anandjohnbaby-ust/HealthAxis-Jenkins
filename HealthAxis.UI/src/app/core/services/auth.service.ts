// src/app/core/services/auth.service.ts
import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { API_ENDPOINTS } from '../constants/api-endpoints.constants';
import { APP_ROUTES } from '../constants/app-routes.constants';
import { STORAGE_KEYS } from '../constants/storage-keys.constants';
import { Role } from '../enums/role.enum';
import { TokenService } from './token.service';
import { ChangePasswordRequest } from '../interfaces/change-password.interface';
import { LoginRequest, LoginResponse, RegisterRequest } from '../interfaces/auth.interface';

@Injectable({
  providedIn: 'root'
})

export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly tokenService = inject(TokenService);

  // Note: Double check if your environment file uses 'apiBaseUrl' or 'apiUrl'
  private readonly baseUrl = environment.apiBaseUrl;

  /** Reactive signal strictly typed to your Role enum */
  readonly currentRole = signal<Role | null>(
    this.tokenService.getRoleFromToken()
  );

  login(payload: LoginRequest): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${this.baseUrl}${API_ENDPOINTS.AUTH.LOGIN}`, payload)
      .pipe(
        tap((res) => {
          this.tokenService.setTokens(res.accessToken, res.refreshToken);
          this.currentRole.set(this.tokenService.getRoleFromToken());
        })
      );
  }

  register(payload: RegisterRequest): Observable<unknown> {
    return this.http.post(`${this.baseUrl}${API_ENDPOINTS.AUTH.REGISTER}`, payload);
  }

  logout(): void {
    this.tokenService.clearTokens();
    localStorage.removeItem(STORAGE_KEYS.USER_ROLE);
    this.currentRole.set(null);
    this.router.navigate(['/' + APP_ROUTES.LOGIN]);
  }

  refreshToken(): Observable<LoginResponse> {
    const refreshToken = this.tokenService.getRefreshToken();
    return this.http
      .post<LoginResponse>(`${this.baseUrl}${API_ENDPOINTS.AUTH.REFRESH_TOKEN}`, {
        refreshToken
      })
      .pipe(
        tap((res) => {
          this.tokenService.setTokens(res.accessToken, res.refreshToken);
        })
      );
  }

  changePassword(payload: ChangePasswordRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}${API_ENDPOINTS.AUTH.CHANGE_PASSWORD}`, payload);
  }

  // ---------------------------------------------------------------------------
  // State & Utility Methods
  // ---------------------------------------------------------------------------

  isLoggedIn(): boolean {
    // Delegating to the fortified logic we built in TokenService
    return this.tokenService.isLoggedIn();
  }

  getRole(): Role | null {
    return this.currentRole();
  }

  redirectAfterLogin(): void {
    const role = this.getRole();

    switch (role) {
      case Role.Admin: {
        const token = this.tokenService.getAccessToken();

        window.location.href =
          `${environment.blazorAdminUrl}/auth?token=${encodeURIComponent(token ?? '')}`;

        break;
      }

      case Role.Doctor:
        this.router.navigate(['/' + APP_ROUTES.DOCTOR.ROOT]);
        break;

      case Role.Patient:
        this.router.navigate(['/' + APP_ROUTES.PATIENT.ROOT]);
        break;

      default:
        this.router.navigate(['/' + APP_ROUTES.UNAUTHORIZED]);
    }
  }
}