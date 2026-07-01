import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import {
  CreateHealthRecordRequest,
  DoctorAppointmentDto,
  HealthRecordDto,
  UpdateAppointmentStatus
} from '../../../../core/interfaces/doctor-domain.types';

import { AppointmentStatus } from '../../../../core/enums/appointment-status.enum';
import { DoctorService } from '../../../../core/services/doctor.service';

type DialogAction =
  | 'confirm'
  | 'complete'
  | 'cancel'
  | 'healthRecord'
  | 'viewHealthRecord'
  | null;

@Component({
  selector: 'app-weekly-schedule',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule
  ],
  templateUrl: './weekly-schedule.component.html',
  styleUrl: './weekly-schedule.component.css'
})
export class WeeklyScheduleComponent implements OnInit {

  private readonly doctorService = inject(DoctorService);

  readonly AppointmentStatus = AppointmentStatus;

  readonly loading = signal(true);
  readonly updating = signal(false);

  readonly appointments = signal<DoctorAppointmentDto[]>([]);

  readonly successMessage = signal<string | null>(null);
  readonly errorMessage = signal<string |null>(null);

  // ===========================
  // Dialog State
  // ===========================

  readonly dialogOpen = signal(false);

  readonly dialogTitle = signal('');
  readonly dialogMessage = signal('');

  readonly dialogAction =
    signal<DialogAction>(null);

  readonly cancellationReason =
    signal('');

  readonly diagnosis =
    signal('');

  readonly prescription =
    signal('');

  readonly notes =
    signal('');

  readonly savingHealthRecord =
    signal(false);

  readonly loadingHealthRecord =
    signal(false);

  readonly selectedHealthRecord =
    signal<HealthRecordDto | null>(null);

  selectedAppointment:
    DoctorAppointmentDto | null = null;

  // ===========================

  ngOnInit(): void {

    this.loadWeeklySchedule();

  }

  loadWeeklySchedule(): void {

    this.loading.set(true);

    this.errorMessage.set(null);

    this.doctorService
      .getWeeklySchedule()
      .subscribe({

        next: appointments => {

          const sorted = [...appointments].sort(

            (a, b) =>

              new Date(a.scheduledDate).getTime() -

              new Date(b.scheduledDate).getTime()

          );

          this.appointments.set(sorted);

          this.loading.set(false);

        },

        error: () => {

          this.errorMessage.set(
            'Unable to load weekly schedule.'
          );

          this.loading.set(false);

        }

      });

  }

  refresh(): void {

    this.loadWeeklySchedule();

  }

  // ===========================
  // Status Helpers
  // ===========================

  getStatus(status: AppointmentStatus): string {

    switch (status) {

      case AppointmentStatus.Pending:
        return 'Pending';

      case AppointmentStatus.Confirmed:
        return 'Confirmed';

      case AppointmentStatus.Cancelled:
        return 'Cancelled';

      case AppointmentStatus.Completed:
        return 'Completed';

      default:
        return 'Unknown';

    }

  }

  canConfirm(appt: DoctorAppointmentDto): boolean {

    return appt.status === AppointmentStatus.Pending;

  }

  canComplete(appt: DoctorAppointmentDto): boolean {

    return appt.status === AppointmentStatus.Confirmed;

  }

  canCancel(appt: DoctorAppointmentDto): boolean {

    return appt.status === AppointmentStatus.Pending ||

           appt.status === AppointmentStatus.Confirmed;

  }

  canAddHealthRecord(appt: DoctorAppointmentDto): boolean {

    return appt.status === AppointmentStatus.Completed;

  }
    // ===========================
  // Dialog
  // ===========================

  openConfirmDialog(appt: DoctorAppointmentDto): void {

    this.selectedAppointment = appt;

    this.dialogTitle.set('Confirm Appointment');

    this.dialogMessage.set(
      'Are you sure you want to confirm this appointment?'
    );

    this.dialogAction.set('confirm');

    this.dialogOpen.set(true);

  }

  openCompleteDialog(appt: DoctorAppointmentDto): void {

    this.selectedAppointment = appt;

    this.dialogTitle.set('Complete Appointment');

    this.dialogMessage.set(
      'Are you sure you want to complete this appointment?'
    );

    this.dialogAction.set('complete');

    this.dialogOpen.set(true);

  }

  openCancelDialog(appt: DoctorAppointmentDto): void {

    this.selectedAppointment = appt;

    this.dialogTitle.set('Cancel Appointment');

    this.dialogMessage.set(
      'Please provide a cancellation reason.'
    );

    this.dialogAction.set('cancel');

    this.cancellationReason.set('');

    this.dialogOpen.set(true);

  }

  openHealthRecordDialog(appt: DoctorAppointmentDto): void {

    this.selectedAppointment = appt;

    this.dialogTitle.set('Add Health Record');

    this.dialogMessage.set(
      'Enter the patient diagnosis and prescription.'
    );

    this.dialogAction.set('healthRecord');

    this.diagnosis.set('');

    this.prescription.set('');

    this.notes.set('');

    this.dialogOpen.set(true);

  }

  closeDialog(): void {

    this.dialogOpen.set(false);

    this.dialogAction.set(null);

    this.selectedAppointment = null;

    this.selectedHealthRecord.set(null);

    this.cancellationReason.set('');

    this.diagnosis.set('');

    this.prescription.set('');

    this.notes.set('');

  }

  // ===========================
  // Confirm Action
  // ===========================

  confirmAction(): void {

    if (!this.selectedAppointment) {
      return;
    }

    switch (this.dialogAction()) {

      case 'confirm':

        this.updateStatus(
          this.selectedAppointment.appointmentId,
          {
            status: AppointmentStatus.Confirmed
          }
        );

        break;

      case 'complete':

        this.updateStatus(
          this.selectedAppointment.appointmentId,
          {
            status: AppointmentStatus.Completed
          }
        );

        break;

      case 'cancel':

        if (!this.cancellationReason().trim()) {

          this.errorMessage.set(
            'Cancellation reason is required.'
          );

          return;

        }

        this.updateStatus(
          this.selectedAppointment.appointmentId,
          {
            status: AppointmentStatus.Cancelled,
            cancellationReason:
              this.cancellationReason()
          }
        );

        break;

      case 'healthRecord':

        if (!this.diagnosis().trim()) {

          this.errorMessage.set(
            'Diagnosis is required.'
          );

          return;

        }

        if (!this.prescription().trim()) {

          this.errorMessage.set(
            'Prescription is required.'
          );

          return;

        }

        this.addHealthRecord();

        return;

    }

    this.closeDialog();

  }
    // ===========================
  // Add Health Record
  // ===========================

  private addHealthRecord(): void {

    if (!this.selectedAppointment) {
      return;
    }

    this.savingHealthRecord.set(true);

    const dto: CreateHealthRecordRequest = {

      appointmentId:
        this.selectedAppointment.appointmentId,

      diagnosis:
        this.diagnosis(),

      prescription:
        this.prescription(),

      notes:
        this.notes()

    };

    this.doctorService
      .addHealthRecord(dto)
      .subscribe({

        next: () => {

          this.successMessage.set(
            'Health record added successfully.'
          );

          this.savingHealthRecord.set(false);

          this.closeDialog();

          // Refresh weekly schedule
          this.loadWeeklySchedule();

        },

        error: err => {

          this.errorMessage.set(

            err?.error?.message ??

            'Unable to add health record.'

          );

          this.savingHealthRecord.set(false);

        }

      });

  }

  // ===========================
  // Update Appointment Status
  // ===========================

  private updateStatus(
    appointmentId: number,
    dto: UpdateAppointmentStatus
  ): void {

    this.updating.set(true);

    this.errorMessage.set(null);

    this.successMessage.set(null);

    this.doctorService
      .updateAppointmentStatus(
        appointmentId,
        dto
      )
      .subscribe({

        next: () => {

          this.successMessage.set(
            'Appointment updated successfully.'
          );

          this.updating.set(false);

          // Refresh weekly schedule
          this.loadWeeklySchedule();

        },

        error: err => {

          this.errorMessage.set(

            err?.error?.message ??

            'Unable to update appointment.'

          );

          this.updating.set(false);

        }

      });

  }

  // ===========================
  // View Health Record
  // ===========================

  viewHealthRecord(recordId: number): void {

    this.loadingHealthRecord.set(true);

    this.doctorService
      .getHealthRecordById(recordId)
      .subscribe({

        next: record => {

          this.selectedHealthRecord.set(record);

          this.dialogTitle.set('Health Record');

          this.dialogMessage.set('');

          this.dialogAction.set('viewHealthRecord');

          this.dialogOpen.set(true);

          this.loadingHealthRecord.set(false);

        },

        error: () => {

          this.errorMessage.set(
            'Unable to load health record.'
          );

          this.loadingHealthRecord.set(false);

        }

      });

  }

}