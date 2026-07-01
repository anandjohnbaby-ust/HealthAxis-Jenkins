// src/app/core/validators/custom-validators.ts
import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export class CustomValidators {
  /** Ensures confirmPassword matches password */
  static passwordMatch(passwordKey: string, confirmKey: string): ValidatorFn {
    return (group: AbstractControl): ValidationErrors | null => {
      const password = group.get(passwordKey)?.value;
      const confirmPassword = group.get(confirmKey)?.value;

      if (!password || !confirmPassword) {
        return null;
      }

      if (password !== confirmPassword) {
        group.get(confirmKey)?.setErrors({ passwordMismatch: true });
        return { passwordMismatch: true };
      }

      // clear error if previously set and now matches
      const confirmControl = group.get(confirmKey);
      if (confirmControl?.hasError('passwordMismatch')) {
        confirmControl.setErrors(null);
      }
      return null;
    };
  }

  /** Ensures date of birth is not in the future */
  static notFutureDate(control: AbstractControl): ValidationErrors | null {
    if (!control.value) {
      return null;
    }
    const inputDate = new Date(control.value);
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    return inputDate > today ? { futureDate: true } : null;
  }

  /** Ensures appointment date is not in the past */
  static notPastDate(control: AbstractControl): ValidationErrors | null {
    if (!control.value) {
      return null;
    }
    const inputDate = new Date(control.value);
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    return inputDate < today ? { pastDate: true } : null;
  }
}