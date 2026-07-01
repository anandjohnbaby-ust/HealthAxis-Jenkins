// src/app/features/doctor/pages/profile/profile.component.ts

import { CommonModule } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { Router } from '@angular/router';

import { DoctorService } from '../../../../core/services/doctor.service';

import {
  Specialisation,
  SpecialisationLabel
} from '../../../../core/enums/specialisation.enum';

import {
  DoctorProfile,
  UpdateDoctorProfileRequest
} from '../../../../core/interfaces/doctor-domain.types';

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
  private readonly doctorService = inject(DoctorService);
  private readonly router = inject(Router);

  readonly loading = signal(true);
  readonly savingProfile = signal(false);

  readonly editing = signal(false);

  readonly successMessage = signal<string | null>(null);
  readonly errorMessage = signal<string | null>(null);

  readonly specialisationOptions =
    Object.entries(SpecialisationLabel).map(
      ([value, label]) => ({
        value: Number(value) as Specialisation,
        label
      })
    );

  doctor!: DoctorProfile;

  profileForm = this.fb.group({

    fullName: [
      '',
      [
        Validators.required,
        Validators.maxLength(100)
      ]
    ],

    email: [
      '',
      [
        Validators.required,
        Validators.email
      ]
    ],

    specialisation: [
      null as Specialisation | null,
      Validators.required
    ],

    yearsOfExperience: [
      0,
      [
        Validators.required,
        Validators.min(0)
      ]
    ],

    consultationFee: [
      0,
      [
        Validators.required,
        Validators.min(0)
      ]
    ]

  });

  ngOnInit(): void {

    this.profileForm
      .get('specialisation')
      ?.disable();

    this.loadProfile();

  }

  // ============================
  // Load Profile
  // ============================

  private loadProfile(): void {

    this.loading.set(true);

    this.successMessage.set(null);

    this.errorMessage.set(null);

    this.doctorService
      .getMyProfile()
      .subscribe({

        next: doctor => {

          this.doctor = doctor;

          this.profileForm.patchValue({

            fullName: doctor.fullName,

            email: doctor.email,

            specialisation: doctor.specialisation,

            yearsOfExperience:
              doctor.yearsOfExperience,

            consultationFee:
              doctor.consultationFee

          });

          if (!this.editing()) {

            this.profileForm
              .get('specialisation')
              ?.disable();

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

    this.profileForm
      .get('specialisation')
      ?.enable();

  }

  cancelEditing(): void {

    this.editing.set(false);

    this.profileForm
      .get('specialisation')
      ?.disable();

    this.loadProfile();

  }

  // ============================
  // Save Profile
  // ============================

  saveProfile(): void {
    
    // if (this.profileForm.invalid) {

    //   this.profileForm.markAllAsTouched();

    //   return;

    // }

    this.savingProfile.set(true);

    this.successMessage.set(null);

    this.errorMessage.set(null);

const dto = this.profileForm.getRawValue() as UpdateDoctorProfileRequest;

    this.doctorService
      .updateMyProfile(dto)
      .subscribe({

        next: doctor => {

          this.doctor = doctor;

          this.successMessage.set(
            'Profile updated successfully.'
          );

          this.savingProfile.set(false);

          this.editing.set(false);

          this.profileForm
            .get('specialisation')
            ?.disable();

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
      '/doctor/profile/change-password'
    ]);

  }

}