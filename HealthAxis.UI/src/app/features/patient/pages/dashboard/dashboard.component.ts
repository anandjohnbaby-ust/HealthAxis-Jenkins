import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';

import { PatientService } from '../../../../core/services/patient.service';
import {
  AppointmentStatus,
  PatientDashboard
} from '../../../../core/interfaces/patient-domain.types';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit {

  private readonly patientService = inject(PatientService);

  readonly dashboard = signal<PatientDashboard | null>(null);

  readonly loading = signal(true);

  readonly AppointmentStatus = AppointmentStatus;

  ngOnInit(): void {
    this.loadDashboard();
  }

  private loadDashboard(): void {
    this.loading.set(true);

    this.patientService.getDashboard().subscribe({
      next: (dashboard) => {
        this.dashboard.set(dashboard);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Failed to load dashboard', error);
        this.loading.set(false);
      }
    });
  }

  refresh(): void {
    this.loadDashboard();
  }
}