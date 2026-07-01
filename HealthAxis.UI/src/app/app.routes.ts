// src/app/app.routes.ts
import { Routes } from '@angular/router';

import { APP_ROUTES } from './core/constants/app-routes.constants';
import { Role } from './core/enums/role.enum';
import { authGuard } from './core/guards/auth.guard';
import { guestGuard } from './core/guards/guest.guard';
import { roleGuard } from './core/guards/role.guard';
import { LogoutComponent } from './features/auth/logout/logout.component';

export const routes: Routes = [
  {
    path: APP_ROUTES.LANDING,
    loadComponent: () =>
      import('./features/landing/landing.component').then((m) => m.LandingComponent)
  },
  {
    path: APP_ROUTES.LOGIN,
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./features/auth/login/login.component').then((m) => m.LoginComponent)
  },
  {
    path: APP_ROUTES.REGISTER,
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./features/auth/register/register.component').then((m) => m.RegisterComponent)
  },

  // Patient Module
  {
    path: APP_ROUTES.PATIENT.ROOT,
    canActivate: [authGuard, roleGuard([Role.Patient])],
    loadChildren: () =>
      import('./features/patient/patient.routes').then((m) => m.PATIENT_ROUTES)
  },

  // Doctor Module (Excluded from current refactor scope)
  {
    path: APP_ROUTES.DOCTOR.ROOT,
    canActivate: [authGuard, roleGuard([Role.Doctor])],
    loadChildren: () =>
      import('./features/doctor/doctor.routes').then((m) => m.DOCTOR_ROUTES)
  },

  {
    path: APP_ROUTES.UNAUTHORIZED,
    loadComponent: () =>
      import('./features/landing/landing.component').then((m) => m.LandingComponent)
  },
    {
    path: 'logout',
    component: LogoutComponent
  },
  
  {
    path: '**',
    redirectTo: APP_ROUTES.LANDING
  }

];