import { Component, computed, HostListener, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

import { AuthService } from '../../../core/services/auth.service';
import { TokenService } from '../../../core/services/token.service';

@Component({
  selector: 'app-doctor-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './doctor-layout.component.html',
  styleUrl: './doctor-layout.component.css'
})
export class DoctorLayoutComponent {

  private readonly authService = inject(AuthService);
  private readonly tokenService = inject(TokenService);
  private readonly router = inject(Router);

  readonly doctorName = computed(() =>
    this.tokenService.getFullNameFromToken() ?? 'Doctor'
  );

  readonly initial = computed(() =>
    this.tokenService.getUserInitialFromToken()
  );

  readonly menuOpen = signal(false);

  toggleMenu(event: Event): void {
    event.stopPropagation();
    this.menuOpen.update(value => !value);
  }

  @HostListener('document:click')
  closeMenu(): void {
    this.menuOpen.set(false);
  }

  goToProfile(): void {
    this.menuOpen.set(false);
    this.router.navigate(['/doctor/profile']);
  }

  logout(): void {
    this.authService.logout();
  }

}