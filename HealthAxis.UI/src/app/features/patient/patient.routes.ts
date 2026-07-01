// src/app/features/patient/patient.routes.ts
import { Routes } from '@angular/router';

import { PatientLayoutComponent } from './layout/patient-layout.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { HealthRecordsComponent } from './pages/health-records/health-records.component';
import { AppointmentsListComponent } from './pages/appointments/appointments-list/appointments-list.component';
import { BookAppointmentComponent } from './pages/appointments/book-appointment/book-appointment.component';
import { ProfileComponent } from './pages/profile/profile.component';
import { ChangePasswordComponent } from './pages/change-password/change-password.component';
import { DoctorsComponent } from './pages/doctors/doctors.component';

export const PATIENT_ROUTES: Routes = [
  {
    path: '',
    component: PatientLayoutComponent,
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
        path: 'health-records',
        component: HealthRecordsComponent
      },
      {
        path: 'appointments',
        component: AppointmentsListComponent
      },
      {
        path: 'appointments/book',
        component: BookAppointmentComponent
      },
      {
        path: 'doctors',
        component: DoctorsComponent
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