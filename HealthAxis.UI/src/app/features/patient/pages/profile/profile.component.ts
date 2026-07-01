// src/app/features/patient/pages/profile/profile.component.ts

import { CommonModule } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { Router } from '@angular/router';

import { PatientService } from '../../../../core/services/patient.service';
import {
  Patient,
  UpdatePatientRequest,
  Gender
} from '../../../../core/interfaces/patient-domain.types';
import { GenderLabel } from '../../../../core/enums/gender.enum';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule
  ],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit {

  private readonly fb = inject(FormBuilder);
  private readonly patientService = inject(PatientService);
  private readonly router = inject(Router);

  readonly loading = signal(true);
  readonly savingProfile = signal(false);

  readonly editing = signal(false);

  readonly successMessage = signal<string | null>(null);
  readonly errorMessage = signal<string | null>(null);

  readonly genderOptions = Object.entries(GenderLabel).map(
    ([value, label]) => ({
      value: Number(value) as Gender,
      label
    })
  );

  patient!: Patient;

  profileForm = this.fb.group({
    fullName: ['', [Validators.required, Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email]],
    phoneNumber: ['', Validators.required],
    dateOfBirth: ['', Validators.required],
    gender: [null as Gender | null, Validators.required]
  });

  ngOnInit(): void {

    // Disable gender initially
    this.profileForm.get('gender')?.disable();

    this.loadProfile();

  }

  // ============================
  // Load Profile
  // ============================

  private loadProfile(): void {

    this.loading.set(true);

    this.successMessage.set(null);

    this.errorMessage.set(null);

    this.patientService.getMyProfile().subscribe({

      next: patient => {

        this.patient = patient;

        this.profileForm.patchValue({

          fullName: patient.fullName,

          email: patient.email,

          phoneNumber: patient.phoneNumber,

          dateOfBirth: patient.dateOfBirth.split('T')[0],

          gender: patient.gender

        });

        // Keep gender disabled while not editing
        if (!this.editing()) {

          this.profileForm.get('gender')?.disable();

        }

        this.loading.set(false);

      },

      error: () => {

        this.errorMessage.set(
          'Unable to load your profile.'
        );

        this.loading.set(false);

      }

    });

  }

  // ============================
  // Edit Mode
  // ============================

  startEditing(): void {

    this.successMessage.set(null);

    this.errorMessage.set(null);

    this.editing.set(true);

    // Enable gender dropdown
    this.profileForm.get('gender')?.enable();

  }

  cancelEditing(): void {

    this.editing.set(false);

    // Disable again
    this.profileForm.get('gender')?.disable();

    this.loadProfile();

  }

  // ============================
  // Save Profile
  // ============================

  saveProfile(): void {

    if (this.profileForm.invalid) {

      this.profileForm.markAllAsTouched();

      return;

    }

    this.savingProfile.set(true);

    this.successMessage.set(null);

    this.errorMessage.set(null);

    const dto =
      this.profileForm.getRawValue() as UpdatePatientRequest;

    this.patientService
      .updateMyProfile(dto)
      .subscribe({

        next: patient => {

          this.patient = patient;

          this.successMessage.set(
            'Profile updated successfully.'
          );

          this.savingProfile.set(false);

          // Back to read-only mode
          this.editing.set(false);

          // Disable gender again
          this.profileForm.get('gender')?.disable();

          // Refresh form values
          this.loadProfile();

        },

        error: () => {

          this.errorMessage.set(
            'Unable to update profile.'
          );

          this.savingProfile.set(false);

        }

      });

  }

  // ============================
  // Change Password
  // ============================

  goToChangePassword(): void {

    this.router.navigate([
      '/patient/profile/change-password'
    ]);

  }

}