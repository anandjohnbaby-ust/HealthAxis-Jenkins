import { Component, inject } from '@angular/core';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-logout',
  standalone: true,
  template: ''
})
export class LogoutComponent {

  private authService = inject(AuthService);

  constructor() {
    this.authService.logout();
  }
}