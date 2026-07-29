import { Component, computed, HostListener, inject, signal, OnInit } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

import { AuthService } from '../../../core/services/auth.service';
import { TokenService } from '../../../core/services/token.service';

const SIDEBAR_COLLAPSE_KEY = 'doctor-sidebar-collapsed';

@Component({
  selector: 'app-doctor-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './doctor-layout.component.html',
  styleUrl: './doctor-layout.component.css'
})
export class DoctorLayoutComponent implements OnInit {

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

  /** Desktop: collapsed (icon-only) vs full-width sidebar */
  readonly sidebarCollapsed = signal(false);

  /** Mobile: off-canvas drawer open/closed */
  readonly mobileSidebarOpen = signal(false);

  ngOnInit(): void {
    const saved = localStorage.getItem(SIDEBAR_COLLAPSE_KEY);
    if (saved === 'true') {
      this.sidebarCollapsed.set(true);
    }
  }

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

  toggleSidebarCollapse(): void {
    this.sidebarCollapsed.update(collapsed => {
      const next = !collapsed;
      localStorage.setItem(SIDEBAR_COLLAPSE_KEY, String(next));
      return next;
    });
  }

  toggleMobileSidebar(event: MouseEvent): void {
    event.stopPropagation();
    this.mobileSidebarOpen.update(open => !open);
  }

  closeMobileSidebar(): void {
    this.mobileSidebarOpen.set(false);
  }
}