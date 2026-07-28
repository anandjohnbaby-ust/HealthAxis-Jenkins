// src/app/features/patient/pages/change-password/change-password.component.ts

import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import {FormBuilder, ReactiveFormsModule, Validators} from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
import { ChangePasswordRequest } from '../../../../core/interfaces/change-password.interface';


@Component({
  selector: 'app-change-password',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
  ],
  templateUrl: './change-password.component.html',
  styleUrl: './change-password.component.css'
})

export class ChangePasswordComponent {

  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly changing = signal(false);
  readonly successMessage = signal<string | null>(null);
  readonly errorMessage = signal<string | null>(null);

  form = this.fb.group({

    currentPassword: [
      '',
      Validators.required
    ],

    newPassword: [
      '',
      [
        Validators.required,
        Validators.minLength(6)
      ]
    ],

    confirmPassword: [
      '',
      Validators.required
    ]

  });

  changePassword(): void {

    this.successMessage.set(null);
    this.errorMessage.set(null);

    if (this.form.invalid) {

      this.form.markAllAsTouched();
      return;

    }

    const value =
      this.form.getRawValue() as ChangePasswordRequest;

    if (value.newPassword !== value.confirmPassword) {

      this.errorMessage.set(
        'New password and confirm password do not match.'
      );

      return;

    }

    this.changing.set(true);

    this.authService
      .changePassword(value)
      .subscribe({

        next: () => {

          this.successMessage.set(
            'Password changed successfully.'
          );

          this.form.reset();

          this.changing.set(false);

        },

        error: () => {

          this.errorMessage.set(
            'Unable to change password.'
          );

          this.changing.set(false);

        }

      });

  }

  goBack(): void {
    this.router.navigate(['/patient/profile']);
  }

}