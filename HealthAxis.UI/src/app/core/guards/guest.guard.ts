// src/app/core/guards/guest.guard.ts
import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const guestGuard: CanActivateFn = () => {
  const authService = inject(AuthService);

  if (!authService.isLoggedIn()) {
    return true;
  }

  // Already logged in? Let the auth service handle the specific routing
  authService.redirectAfterLogin();
  return false; 
};