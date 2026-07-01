// src/app/shared/components/input-error/input-error.component.ts
import { Component, input } from '@angular/core';
import { AbstractControl } from '@angular/forms';

@Component({
  selector: 'app-input-error',
  standalone: true,
  template: `
    @if (control() && control()!.invalid && (control()!.dirty || control()!.touched)) {
      <small class="input-error">{{ message() }}</small>
    }
  `,
  styles: [`
    .input-error {
      color: #dc2626;
      font-size: 0.8rem;
      margin-top: 4px;
      display: block;
    }
  `]
})
export class InputErrorComponent {
  control = input<AbstractControl | null>(null);
  message = input<string>('');
}