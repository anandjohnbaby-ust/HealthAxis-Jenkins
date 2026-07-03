// src/app/features/doctor/doctor.routes.ts
import { Routes } from '@angular/router';

import { DoctorLayoutComponent } from './layout/doctor-layout.component';

// Security imports
import { authGuard } from '../../core/guards/auth.guard';
import { roleGuard } from '../../core/guards/role.guard';
import { Role } from '../../core/enums/role.enum';

import { SharedScheduleComponent } from './pages/shared-schedule/shared-schedule.component';
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
        component: SharedScheduleComponent,
        data: {
          mode: 'today',
          title: "Today's Appointment",
          emptyMessage: 'No appointments found.'
        }
      },
      {
        path: 'week',
        component: SharedScheduleComponent,
        data: {
          mode: 'week',
          title: 'Weekly Schedule',
          emptyMessage: 'No appointments scheduled for this week.'
        }
      },
      {
        path: 'appointments',
        component: SharedScheduleComponent,
        data: {
          mode: 'all',
          title: 'All Appointments',
          emptyMessage: 'No appointments found.'
        }
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