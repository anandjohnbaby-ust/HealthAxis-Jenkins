// src/app/core/interceptors/error.interceptor.ts
import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { APP_ROUTES } from '../constants/app-routes.constants';
import { AuthService } from '../services/auth.service';
import { API_ENDPOINTS } from '../constants/api-endpoints.constants';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const authService = inject(AuthService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      
      // Handle 401 Unauthorized
      if (error.status === 401) {
        
        // Prevent infinite loops if the refresh token request itself fails with a 401
        if (req.url.includes(API_ENDPOINTS.AUTH.REFRESH_TOKEN)) {
          authService.logout();
          return throwError(() => error);
        }

        // Attempt silent refresh
        return authService.refreshToken().pipe(
          switchMap((response) => {
            // Refresh succeeded! Clone the original request with the new token
            const clonedReq = req.clone({
              setHeaders: { Authorization: `Bearer ${response.accessToken}` }
            });
            // Retry the request
            return next(clonedReq);
          }),
          catchError((refreshErr) => {
            // Refresh failed (likely expired). Log them out.
            authService.logout();
            return throwError(() => refreshErr);
          })
        );
      } 
      
      // Handle 403 Forbidden
      else if (error.status === 403) {
        router.navigate(['/' + APP_ROUTES.UNAUTHORIZED]);
      }

      // Propagate the error for component-level handling
      return throwError(() => error);
    })
  );
};