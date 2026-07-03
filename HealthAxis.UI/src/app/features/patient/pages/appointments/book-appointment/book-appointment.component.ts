// src/app/features/patient/pages/appointments/book-appointment/book-appointment.component.ts
import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';

import { TokenService } from '../../../../../core/services/token.service';
import { PatientService } from '../../../../../core/services/patient.service';
import { VALIDATION_MESSAGES } from '../../../../../core/constants/validation-messages.constants';
import { CustomValidators } from '../../../../../core/validators/custom-validators';
import { InputErrorComponent } from '../../../../../shared/components/input-error/input-error.component';
import { ButtonLoaderComponent } from '../../../../../shared/components/button-loader/button-loader.component';
import { ApiErrorResponse } from '../../../../../core/interfaces/api-response.interface';
import { DoctorDto, Specialisation } from '../../../../../core/interfaces/patient-domain.types';
import { SPECIALISATION_OPTIONS } from '../../../../../core/enums/specialisation.enum';

@Component({
  selector: 'app-book-appointment',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, InputErrorComponent, ButtonLoaderComponent],
  templateUrl: './book-appointment.component.html',
  styleUrl: './book-appointment.component.css'
})
export class BookAppointmentComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly patientService = inject(PatientService);
  private readonly tokenService = inject(TokenService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly VALIDATION_MESSAGES = VALIDATION_MESSAGES;
  readonly specialisationOptions = SPECIALISATION_OPTIONS;
  readonly minDate = new Date().toISOString().split('T')[0];

  readonly doctors = signal<DoctorDto[]>([]);
  readonly doctorsLoading = signal(false);
  readonly submitting = signal(false);
  readonly serverError = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);

  readonly bookingForm = this.fb.nonNullable.group({
    specialisation: [null as Specialisation | null, [Validators.required]],
    doctorId: [null as number | null, [Validators.required]],
    scheduledDate: ['', [Validators.required, CustomValidators.notPastDate]],
    timeSlot: ['', [Validators.required]]
  });
  
  timeSlots = [
    { value: '09:00:00', label: '09:00 AM - 10:00 AM' },
    { value: '10:00:00', label: '10:00 AM - 11:00 AM' },
    { value: '11:00:00', label: '11:00 AM - 12:00 PM' },
    { value: '12:00:00', label: '12:00 PM - 01:00 PM' },
    { value: '13:00:00', label: '01:00 PM - 02:00 PM' },
    { value: '14:00:00', label: '02:00 PM - 03:00 PM' },
    { value: '15:00:00', label: '03:00 PM - 04:00 PM' },
    { value: '16:00:00', label: '04:00 PM - 05:00 PM' },
    { value: '17:00:00', label: '05:00 PM - 06:00 PM' }
  ];

  ngOnInit(): void {
    // 1. Listen for specialisation changes to reload doctors
    this.bookingForm.get('specialisation')!.valueChanges.subscribe(specialisation => {
      // Capture the current doctorId before we reset the doctors array
      const currentDoctorId = this.bookingForm.get('doctorId')?.value;
      
      this.doctors.set([]);

      if (specialisation === null) {
        this.bookingForm.patchValue({ doctorId: null }, { emitEvent: false });
        return;
      }

      // Pass the current doctorId so loadAvailableDoctors can retain it if it's valid
      this.loadAvailableDoctors(specialisation, currentDoctorId ?? undefined);
    });

    // 2. Handle incoming Query Parameters from the Doctors List
    this.route.queryParams.subscribe(params => {
      const doctorId = Number(params['doctorId']);
      
      // FIX: Explicitly cast the URL string parameter to a Number
      const specialisation = Number(params['specialisation']) as Specialisation; 

      if (!Number.isNaN(doctorId) && !Number.isNaN(specialisation) && specialisation) {
        // Set the doctorId silently first
        this.bookingForm.patchValue({ doctorId: doctorId }, { emitEvent: false });
        // Set the specialisation (this triggers the valueChanges listener above!)
        this.bookingForm.patchValue({ specialisation: specialisation });
      }
    });
  }

  private loadAvailableDoctors(specialisation: Specialisation, selectedDoctorId?: number): void {
    this.doctorsLoading.set(true);

    this.patientService.getAvailableDoctors(specialisation).subscribe({
      next: data => {
        const doctors = data.filter(doc => doc.isActive);
        this.doctors.set(doctors);

        // If a doctor was selected (or passed via query params), keep them selected
        if (selectedDoctorId !== undefined && doctors.some(d => d.doctorId === selectedDoctorId)) {
          this.bookingForm.patchValue({ doctorId: selectedDoctorId }, { emitEvent: false });
        } else {
          this.bookingForm.patchValue({ doctorId: null }, { emitEvent: false });
        }

        this.doctorsLoading.set(false);
      },
      error: () => {
        this.doctorsLoading.set(false);
      }
    });
  }

  dateErrorMessage(): string {
    const control = this.bookingForm.get('scheduledDate');
    if (control?.hasError('pastDate')) {
      return 'Date cannot be in the past.';
    }
    return VALIDATION_MESSAGES.REQUIRED('Date');
  }

  onSubmit(): void {
    if (this.bookingForm.invalid) {
      this.bookingForm.markAllAsTouched();
      return;
    }

    const token = this.tokenService.getAccessToken() ?? '';
    const patientId = this.tokenService.getPatientIdFromToken(token);

    if (patientId === null) {
      this.serverError.set('Could not determine your patient ID. Please log in again.');
      return;
    }

    const { doctorId, scheduledDate, timeSlot } = this.bookingForm.getRawValue();

    this.serverError.set(null);
    this.submitting.set(true);

    this.patientService
      .bookAppointment(patientId, {
        patientId,
        doctorId: doctorId!,
        scheduledDate,
        timeSlot
      })
      .subscribe({
        next: () => {
          this.submitting.set(false);
          this.successMessage.set('Appointment booked successfully! Redirecting...');
          setTimeout(() => this.router.navigate(['/patient/appointments']), 1200);
        },
        error: (err: HttpErrorResponse) => {
          this.submitting.set(false);
          const apiError = err.error as ApiErrorResponse;
          this.serverError.set(apiError?.message ?? VALIDATION_MESSAGES.GENERIC_ERROR);
        }
      });
  }
}