// src/app/core/services/navigation.service.ts
import { inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { APP_ROUTES } from '../constants/app-routes.constants';

/** Small helper to avoid scattering route strings across components */
@Injectable({ providedIn: 'root' })
export class NavigationService {
  private readonly router = inject(Router);

  toLogin(): void {
    this.router.navigate(['/' + APP_ROUTES.LOGIN]);
  }

  toRegister(): void {
    this.router.navigate(['/' + APP_ROUTES.REGISTER]);
  }

  toLanding(): void {
    this.router.navigate(['/']);
  }
}