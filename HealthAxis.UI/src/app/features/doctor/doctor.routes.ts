// src/app/features/doctor/doctor.routes.ts
import { Routes } from '@angular/router';

import { DoctorLayoutComponent } from './layout/doctor-layout.component';

// Security imports
import { authGuard } from '../../core/guards/auth.guard';
import { roleGuard } from '../../core/guards/role.guard';
import { Role } from '../../core/enums/role.enum';
import { AppointmentsComponent } from './pages/appointments/appointments.component';
import { TodayScheduleComponent } from './pages/today-schedule/today-schedule.component';
import { WeeklyScheduleComponent } from './pages/weekly-schedule/weekly-schedule.component';
import { ProfileComponent } from './pages/profile/profile.component';
import { ChangePasswordComponent } from './pages/change-password/change-password.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';

export const DOCTOR_ROUTES: Routes = [
  {
    path: '',
    component: DoctorLayoutComponent,
    canActivate: [authGuard, roleGuard([Role.Doctor])],
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      },
      {
        path: 'dashboard',
        component: DashboardComponent
      },
      {
        path: 'today',
        component: TodayScheduleComponent
      },
      {
        path: 'week',
        component: WeeklyScheduleComponent
      },
      {
        path: 'appointments',
        component: AppointmentsComponent
      },
      {
        path: 'profile',
        component: ProfileComponent
      },
      {
        path: 'profile/change-password',
        component: ChangePasswordComponent
      }
    ]
  }
];