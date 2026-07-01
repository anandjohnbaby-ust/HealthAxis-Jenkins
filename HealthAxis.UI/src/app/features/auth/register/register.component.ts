// src/app/features/auth/register/register.component.ts
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { VALIDATION_MESSAGES } from '../../../core/constants/validation-messages.constants';
import { REGEX_PATTERNS } from '../../../core/validators/regex-patterns.constants';
import { CustomValidators } from '../../../core/validators/custom-validators';
import { Gender, GenderLabel } from '../../../core/enums/gender.enum';
import { InputErrorComponent } from '../../../shared/components/input-error/input-error.component';
import { ButtonLoaderComponent } from '../../../shared/components/button-loader/button-loader.component';
import { ApiErrorResponse } from '../../../core/interfaces/api-response.interface';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, InputErrorComponent, ButtonLoaderComponent],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly VALIDATION_MESSAGES = VALIDATION_MESSAGES;
  readonly emailErrorMessage = `${VALIDATION_MESSAGES.REQUIRED('Email')} ${VALIDATION_MESSAGES.EMAIL_INVALID}`;

  readonly loading = signal(false);
  readonly serverError = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);

  readonly genderOptions = [
    { value: Gender.Male, label: GenderLabel[Gender.Male] },
    { value: Gender.Female, label: GenderLabel[Gender.Female] },
    { value: Gender.Other, label: GenderLabel[Gender.Other] }
  ];

  readonly registerForm = this.fb.nonNullable.group(
    {
      fullName: ['', [Validators.required, Validators.pattern(REGEX_PATTERNS.NAME)]],
      dateOfBirth: ['', [Validators.required, CustomValidators.notFutureDate]],
      gender: [null as Gender | null, [Validators.required]],
      phoneNumber: ['', [Validators.required, Validators.pattern(REGEX_PATTERNS.PHONE)]],
      email: ['', [Validators.required, Validators.pattern(REGEX_PATTERNS.EMAIL)]],
      password: ['', [Validators.required, Validators.pattern(REGEX_PATTERNS.PASSWORD)]],
      confirmPassword: ['', [Validators.required]]
    },
    { validators: CustomValidators.passwordMatch('password', 'confirmPassword') }
  );

  dobErrorMessage(): string {
    const control = this.registerForm.get('dateOfBirth');
    if (control?.hasError('futureDate')) {
      return VALIDATION_MESSAGES.DOB_FUTURE;
    }
    return VALIDATION_MESSAGES.REQUIRED('Date of birth');
  }

  onSubmit(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.serverError.set(null);
    this.loading.set(true);

    const rawValue = this.registerForm.getRawValue();
    const payload = {
      ...rawValue,
      gender: rawValue.gender!
    };

    this.authService.register(payload).subscribe({
      next: () => {
        this.loading.set(false);
        this.successMessage.set('Account created successfully! Redirecting to login...');
        setTimeout(() => this.router.navigate(['/login']), 1500);
      },
      error: (err: HttpErrorResponse) => {
        this.loading.set(false);
        const apiError = err.error as ApiErrorResponse;
        this.serverError.set(apiError?.message ?? VALIDATION_MESSAGES.GENERIC_ERROR);
      }
    });
  }
}