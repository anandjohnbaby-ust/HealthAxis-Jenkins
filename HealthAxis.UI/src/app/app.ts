import { Component, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AuthService } from './core/services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrls: ['./app.css']
})
export class App {
  protected readonly title = signal('HealthAxis.UI');
  private readonly authService = inject(AuthService);

  protected isLoggedIn(): boolean {
    return this.authService.isLoggedIn();
  }

  protected logout(): void {
    this.authService.logout();
  }
}
