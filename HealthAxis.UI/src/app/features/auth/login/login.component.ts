// src/app/features/auth/login/login.component.ts
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { VALIDATION_MESSAGES } from '../../../core/constants/validation-messages.constants';
import { REGEX_PATTERNS } from '../../../core/validators/regex-patterns.constants';
import { InputErrorComponent } from '../../../shared/components/input-error/input-error.component';
import { ButtonLoaderComponent } from '../../../shared/components/button-loader/button-loader.component';
import { ApiErrorResponse } from '../../../core/interfaces/api-response.interface';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, InputErrorComponent, ButtonLoaderComponent],
  templateUrl: 'login.component.html',
  styleUrl: 'login.component.css'
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);

  readonly VALIDATION_MESSAGES = VALIDATION_MESSAGES;
  readonly emailErrorMessage = `${VALIDATION_MESSAGES.REQUIRED('Email')} ${VALIDATION_MESSAGES.EMAIL_INVALID}`;

  readonly loading = signal(false);
  readonly serverError = signal<string | null>(null);

  readonly loginForm = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.pattern(REGEX_PATTERNS.EMAIL)]],
    password: ['', [Validators.required]]
  });

  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.serverError.set(null);
    this.loading.set(true);

    this.authService.login(this.loginForm.getRawValue()).subscribe({
      next: () => {
        this.loading.set(false);
        this.authService.redirectAfterLogin();
      },
      error: (err: HttpErrorResponse) => {
        this.loading.set(false);
        const apiError = err.error as ApiErrorResponse;
        this.serverError.set(apiError?.message ?? VALIDATION_MESSAGES.INVALID_CREDENTIALS);
      }
    });
  }
}