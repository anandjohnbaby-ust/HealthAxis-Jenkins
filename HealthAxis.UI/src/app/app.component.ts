// src/app/app.component.ts
import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NgIf } from '@angular/common';
import { AuthService } from './core/services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrls: ['./app.css']
})
export class AppComponent {
  private readonly authService = inject(AuthService);

  protected isLoggedIn(): boolean {
    return this.authService.isLoggedIn();
  }

  protected logout(): void {
    this.authService.logout();
  }
}