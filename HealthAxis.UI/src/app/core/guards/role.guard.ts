// src/app/core/guards/role.guard.ts
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { APP_ROUTES } from '../constants/app-routes.constants';
import { AuthService } from '../services/auth.service';

export const roleGuard = (allowedRoles: string[]): CanActivateFn => {
  return () => {
    const authService = inject(AuthService);
    const router = inject(Router);

    const role = authService.getRole();

    if (role && allowedRoles.includes(role)) {
      return true;
    }

    return router.createUrlTree(['/' + APP_ROUTES.UNAUTHORIZED]);
  };
};