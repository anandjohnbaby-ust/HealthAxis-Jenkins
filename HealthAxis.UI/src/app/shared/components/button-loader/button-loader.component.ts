// src/app/shared/components/button-loader/button-loader.component.ts
import { Component, input } from '@angular/core';

@Component({
  selector: 'app-button-loader',
  standalone: true,
  template: `<span class="spinner" [class.show]="loading()"></span>`,
  styles: [`
    .spinner {
      display: none;
      width: 16px;
      height: 16px;
      border: 2px solid #fff;
      border-top-color: transparent;
      border-radius: 50%;
      animation: spin 0.6s linear infinite;
      margin-left: 8px;
    }
    .spinner.show {
      display: inline-block;
    }
    @keyframes spin {
      to { transform: rotate(360deg); }
    }
  `]
})
export class ButtonLoaderComponent {
  loading = input<boolean>(false);
}