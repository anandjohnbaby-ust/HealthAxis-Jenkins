// src/app/features/patient/layout/patient-layout.component.ts
import { Component, HostListener, OnInit, computed, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

import { AuthService } from '../../../core/services/auth.service';
import { PatientService } from '../../../core/services/patient.service';

const SIDEBAR_COLLAPSE_KEY = 'patient-sidebar-collapsed';

@Component({
  selector: 'app-patient-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './patient-layout.component.html',
  styleUrl: './patient-layout.component.css'
})
export class PatientLayoutComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly patientService = inject(PatientService);
  private readonly router = inject(Router);

  readonly menuOpen = signal(false);

  /** Desktop: collapsed (icon-only) vs full-width sidebar */
  readonly sidebarCollapsed = signal(false);

  /** Mobile: off-canvas drawer open/closed */
  readonly mobileSidebarOpen = signal(false);

  /** Reactive patient signal */
  readonly patient = this.patientService.currentPatient;

  /** Full name shown in the top-right corner */
  readonly fullName = computed(() => this.patient()?.fullName ?? '');

  /** Avatar initial */
  readonly initial = computed(() => {
    const name = this.fullName().trim();
    return name.length > 0 ? name.charAt(0).toUpperCase() : '?';
  });

  ngOnInit(): void {
    // Load the profile only if it hasn't already been loaded.
    if (!this.patient()) {
      this.patientService.getMyProfile().subscribe();
    }

    // Restore the user's last collapse preference on desktop.
    const saved = localStorage.getItem(SIDEBAR_COLLAPSE_KEY);
    if (saved === 'true') {
      this.sidebarCollapsed.set(true);
    }
  }

  toggleMenu(event: MouseEvent): void {
    event.stopPropagation();
    this.menuOpen.update((open) => !open);
  }

  goToProfile(): void {
    this.menuOpen.set(false);
    this.router.navigate(['/patient/profile']);
  }

  logout(): void {
    this.menuOpen.set(false);
    this.authService.logout();
  }

  /** Desktop minimize/maximize toggle */
  toggleSidebarCollapse(): void {
    this.sidebarCollapsed.update((collapsed) => {
      const next = !collapsed;
      localStorage.setItem(SIDEBAR_COLLAPSE_KEY, String(next));
      return next;
    });
  }

  /** Mobile hamburger toggle */
  toggleMobileSidebar(event: MouseEvent): void {
    event.stopPropagation();
    this.mobileSidebarOpen.update((open) => !open);
  }

  closeMobileSidebar(): void {
    this.mobileSidebarOpen.set(false);
  }

  @HostListener('document:click')
  closeMenu(): void {
    this.menuOpen.set(false);
  }
}