// src/app/features/patient/pages/appointments/appointments-list/appointments-list.component.ts

import { CommonModule } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

import { TokenService } from '../../../../../core/services/token.service';
import { PatientService } from '../../../../../core/services/patient.service';
import { Appointment, AppointmentStatus } from '../../../../../core/interfaces/patient-domain.types';
import { AppointmentStatusLabel } from '../../../../../core/enums/appointment-status.enum';

@Component({
  selector: 'app-appointments-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    FormsModule
  ],
  templateUrl: './appointments-list.component.html',
  styleUrl: './appointments-list.component.css'
})
export class AppointmentsListComponent implements OnInit {

  private readonly patientService = inject(PatientService);
  private readonly tokenService = inject(TokenService);

  readonly loading = signal(true);
  readonly appointments = signal<Appointment[]>([]);
  readonly loadError = signal<string | null>(null);
  readonly showCancelDialog = signal(false);

  selectedAppointment: Appointment | null = null;
  cancellationReason = '';

  // Expose the enum to the template so we can use it in @if statements
  readonly AppointmentStatus = AppointmentStatus;

  ngOnInit(): void {
    this.loadAppointments();
  }

  private loadAppointments(): void {
    this.loading.set(true);
    this.loadError.set(null);

    const token = this.tokenService.getAccessToken() ?? '';
    const patientId = this.tokenService.getPatientIdFromToken(token);

    if (patientId === null) {
      this.loading.set(false);
      this.loadError.set('Could not determine your patient ID. Please log in again.');
      return;
    }

    this.patientService.getMyAppointments(patientId).subscribe({
      next: (data) => {
        this.appointments.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.loadError.set('Could not load your appointments. Please try again.');
      }
    });
  }

  openCancelDialog(appointment: Appointment): void {
    this.selectedAppointment = appointment;
    this.cancellationReason = '';
    this.showCancelDialog.set(true);
  }

  closeCancelDialog(): void {
    this.selectedAppointment = null;
    this.cancellationReason = '';
    this.showCancelDialog.set(false);
  }

  confirmCancel(): void {
    if (!this.selectedAppointment) {
      return;
    }

    const token = this.tokenService.getAccessToken() ?? '';
    const patientId = this.tokenService.getPatientIdFromToken(token);

    if (patientId === null) {
      return;
    }

    this.patientService
      .cancelAppointment(patientId, this.selectedAppointment.appointmentId, {
        cancellationReason: this.cancellationReason.trim() || null
      })
      .subscribe({
        next: () => {
          this.closeCancelDialog();
          this.loadAppointments();
        },
        error: () => {
          alert('Unable to cancel appointment.');
        }
      });
  }

  statusLabel(status: AppointmentStatus): string {
    // Maps the number (1, 2, 3) back to the readable label ('Pending', 'Confirmed')
    return AppointmentStatusLabel[status as keyof typeof AppointmentStatusLabel] ?? 'Unknown';
  }

  badgeClass(status: AppointmentStatus): string {
    // Restored your original logic: maps to CSS classes like badge--1, badge--2
    const known = [
      AppointmentStatus.Pending, 
      AppointmentStatus.Confirmed, 
      AppointmentStatus.Cancelled, 
      AppointmentStatus.Completed
    ];
    
    return known.includes(status) ? `badge--${status}` : 'badge--unknown';
  }

  formatDate(isoDate: string): string {
    if (!isoDate) return '';
    return isoDate.split('T')[0];
  }
}