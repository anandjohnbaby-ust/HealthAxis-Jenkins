// src/app/features/patient/pages/appointments/appointments-list/appointments-list.component.ts

import { CommonModule } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

import { TokenService } from '../../../../../core/services/token.service';
import { PatientService } from '../../../../../core/services/patient.service';
import {
  Appointment,
  AppointmentStatus,
  HealthRecord
} from '../../../../../core/interfaces/patient-domain.types';
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
  readonly showHealthRecordDialog = signal(false);
  readonly selectedHealthRecord = signal<HealthRecord | null>(null);

  readonly loading = signal(true);
  readonly appointments = signal<Appointment[]>([]);
  readonly loadError = signal<string | null>(null);

  readonly showCancelDialog = signal(false);
  readonly showReasonDialog = signal(false);
  readonly selectedCancellationReason = signal('');

  // ==========================
  // Pagination
  // ==========================

  readonly pageNumber = signal(1);
  readonly pageSize = signal(10);
  readonly totalCount = signal(0);

  // ==========================

  selectedAppointment: Appointment | null = null;
  cancellationReason = '';

  readonly AppointmentStatus = AppointmentStatus;
  // ==========================
  // Filters
  // ==========================

  search = '';
  selectedStatus: AppointmentStatus | null = null;
  selectedDate = '';

  private readonly searchSubject = new Subject<string>();

  ngOnInit(): void {

    this.searchSubject
      .pipe(
        debounceTime(300),
        distinctUntilChanged()
      )
      .subscribe(() => {

        this.pageNumber.set(1);

        this.loadAppointments();

      });

    this.loadAppointments();

  }
  private loadAppointments(): void {

    this.loading.set(true);
    this.loadError.set(null);

    const token = this.tokenService.getAccessToken() ?? '';
    const patientId = this.tokenService.getPatientIdFromToken(token);

    if (patientId === null) {

      this.loading.set(false);

      this.loadError.set(
        'Could not determine your patient ID. Please log in again.'
      );

      return;
    }

    this.patientService
      .getMyAppointments(
        patientId,
        this.pageNumber(),
        this.pageSize(),
        this.search,
        this.selectedStatus,
        this.selectedDate || null
      )
      .subscribe({

        next: result => {

          this.appointments.set(result.items);

          this.totalCount.set(result.totalCount);

          this.loading.set(false);

        },

        error: () => {

          this.loading.set(false);

          this.loadError.set(
            'Could not load your appointments. Please try again.'
          );

        }

      });
  }

  previousPage(): void {

    if (this.pageNumber() > 1) {

      this.pageNumber.update(p => p - 1);

      this.loadAppointments();

    }

  }

  nextPage(): void {

    if (this.pageNumber() < this.totalPages()) {

      this.pageNumber.update(p => p + 1);

      this.loadAppointments();

    }

  }

  totalPages(): number {

    return Math.ceil(
      this.totalCount() / this.pageSize()
    );

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

  openReasonDialog(appointment: Appointment): void {

    this.selectedCancellationReason.set(
      appointment.cancellationReason?.trim() ||
      'No cancellation reason was provided.'
    );

    this.showReasonDialog.set(true);

  }

  closeReasonDialog(): void {

    this.selectedCancellationReason.set('');

    this.showReasonDialog.set(false);

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
      .cancelAppointment(
        patientId,
        this.selectedAppointment.appointmentId,
        {
          cancellationReason:
            this.cancellationReason.trim() || null
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
openHealthRecordDialog(recordId: number): void {

  const token = this.tokenService.getAccessToken() ?? '';
  const patientId = this.tokenService.getPatientIdFromToken(token);

  if (patientId === null) {
    return;
  }

  this.patientService
    .getHealthRecordsByPatientId(
      patientId,
      1,
      1000
    )
    .subscribe({

      next: result => {

        const record = result.items.find(
          r => r.recordId === recordId
        );

        if (!record) {

          alert('Health record not found.');

          return;

        }

        this.selectedHealthRecord.set(record);

        this.showHealthRecordDialog.set(true);

      },

      error: () => {

        alert('Unable to load health record.');

      }

    });

}

closeHealthRecordDialog(): void {

  this.selectedHealthRecord.set(null);

  this.showHealthRecordDialog.set(false);

}

  statusLabel(status: AppointmentStatus): string {

    return AppointmentStatusLabel[status] ?? 'Unknown';

  }

  badgeClass(status: AppointmentStatus): string {

    const known = [
      AppointmentStatus.Pending,
      AppointmentStatus.Confirmed,
      AppointmentStatus.Cancelled,
      AppointmentStatus.Completed
    ];

    return known.includes(status)
      ? `badge--${status}`
      : 'badge--unknown';

  }

  formatDate(isoDate: string): string {

    if (!isoDate) {
      return '';
    }

    return isoDate.split('T')[0];

  }
  onSearchChange(): void {

  this.searchSubject.next(this.search);

}

  // Shared by onStatusChange/onDateChange: reset to page 1 and reload
  // with the current filters.
  private applyFilters(): void {

    this.pageNumber.set(1);

    this.loadAppointments();

  }

onStatusChange(): void {

  this.applyFilters();

}
onDateChange(): void {

  this.applyFilters();

}
clearFilters(): void {

  this.search = '';
  this.selectedStatus = null;
  this.selectedDate = '';

  this.pageNumber.set(1);

  this.loadAppointments();

}
}