import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import {
  CreateHealthRecordRequest,
  DoctorAppointmentDto,
  HealthRecordDto,
  UpdateAppointmentStatus
} from '../../../../core/interfaces/doctor-domain.types';
import { DoctorService } from '../../../../core/services/doctor.service';

type DialogAction =
  | 'confirm'
  | 'complete'
  | 'cancel'
  | 'healthRecord'
  | 'viewHealthRecord'
  | null;

@Component({
  selector: 'app-appointments',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule
  ],
  templateUrl: './appointments.component.html',
  styleUrl: './appointments.component.css'
})
export class AppointmentsComponent implements OnInit {

  private readonly doctorService = inject(DoctorService);

  readonly loading = signal(true);
  readonly updating = signal(false);

  readonly appointments = signal<DoctorAppointmentDto[]>([]);

  readonly successMessage = signal<string | null>(null);
  readonly errorMessage = signal<string | null>(null);

  // ===========================
  // Dialog State
  // ===========================

  readonly dialogOpen = signal(false);

  readonly dialogTitle = signal('');
  readonly dialogMessage = signal('');

  readonly dialogAction = signal<DialogAction>(null);

  readonly cancellationReason = signal('');

  readonly diagnosis = signal('');

  readonly prescription = signal('');

  readonly notes = signal('');

  readonly savingHealthRecord = signal(false);

  readonly loadingHealthRecord = signal(false);

  readonly selectedHealthRecord = signal<HealthRecordDto | null>(null);

  selectedAppointment: DoctorAppointmentDto | null = null;

  // ===========================

  ngOnInit(): void {
    this.loadAppointments();
  }

  loadAppointments(): void {

    this.loading.set(true);
    this.errorMessage.set(null);

    this.doctorService.getAllAppointments().subscribe({

next: appointments => {

    console.log('Appointments from API:', appointments);

    const sorted = [...appointments].sort(
      (a, b) =>
        new Date(b.scheduledDate).getTime() -
        new Date(a.scheduledDate).getTime()
    );

    this.appointments.set(sorted);

    this.loading.set(false);

},

      error: () => {

        this.errorMessage.set(
          'Unable to load appointments.'
        );

        this.loading.set(false);

      }

    });

  }

  refresh(): void {
    this.loadAppointments();
  }

  // ===========================
  // Status Helpers
  // ===========================

  getStatus(status: number): string {

    switch (status) {

      case 1:
        return 'Pending';

      case 2:
        return 'Confirmed';

      case 3:
        return 'Cancelled';

      case 4:
        return 'Completed';

      default:
        return 'Unknown';

    }

  }

  canConfirm(appt: DoctorAppointmentDto): boolean {
    return appt.status === 1;
  }

  canComplete(appt: DoctorAppointmentDto): boolean {
    return appt.status === 2;
  }

  canCancel(appt: DoctorAppointmentDto): boolean {
    return appt.status === 1 || appt.status === 2;
  }

  canAddHealthRecord(appt: DoctorAppointmentDto): boolean {
    return appt.status === 4;
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

closeDialog(): void {

  this.dialogOpen.set(false);

  this.dialogAction.set(null);

  this.selectedAppointment = null;

  this.selectedHealthRecord.set(null);   // <-- Add here

  this.cancellationReason.set('');

  this.diagnosis.set('');

  this.prescription.set('');

  this.notes.set('');

}

  openHealthRecordDialog(appt: DoctorAppointmentDto): void {

  this.selectedAppointment = appt;

  this.dialogTitle.set('Add Health Record');

  this.dialogMessage.set('Enter the patients diagnosis and prescription.');

  this.dialogAction.set('healthRecord');

  this.diagnosis.set('');

  this.prescription.set('');

  this.notes.set('');

  this.dialogOpen.set(true);

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
            status: 2
          }
        );

        break;

      case 'complete':

        this.updateStatus(
          this.selectedAppointment.appointmentId,
          {
            status: 4
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
            status: 3,
            cancellationReason:
              this.cancellationReason()
          }
        );

        break;

      case 'healthRecord':

        if (!this.diagnosis().trim()) {

            this.errorMessage.set('Diagnosis is required.');

            return;

        }
        if (!this.prescription().trim()) {

            this.errorMessage.set('Prescription is required.');

            return;
        }
        this.addHealthRecord();
        return;

    }

    this.closeDialog();

  }

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

    this.doctorService.addHealthRecord(dto)
        .subscribe({

            next: () => {

                this.successMessage.set(
                    'Health record added successfully.'
                );

                this.savingHealthRecord.set(false);

                this.closeDialog();

                this.loadAppointments();

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

          this.loadAppointments();

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