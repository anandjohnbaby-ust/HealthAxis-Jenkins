// src/app/core/interceptors/auth.interceptor.ts
import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { TokenService } from '../services/token.service';
import { environment } from '../../../environments/environment';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const tokenService = inject(TokenService);
  const accessToken = tokenService.getAccessToken();

  // Only attach the token to requests going to YOUR API
  const isApiUrl = req.url.startsWith(environment.apiBaseUrl);

  if (accessToken && isApiUrl) {
    const cloned = req.clone({
      setHeaders: { Authorization: `Bearer ${accessToken}` }
    });
    return next(cloned);
  }

  return next(req);
};