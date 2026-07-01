import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';

import { AppointmentStatus } from '../../../../core/enums/appointment-status.enum';
import { DoctorDashboard } from '../../../../core/interfaces/doctor-domain.types';
import { DoctorService } from '../../../../core/services/doctor.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit {

  private readonly doctorService = inject(DoctorService);

  readonly dashboard = signal<DoctorDashboard | null>(null);

  readonly loading = signal(true);

  readonly AppointmentStatus = AppointmentStatus;

  ngOnInit(): void {
    this.loadDashboard();
  }

  private loadDashboard(): void {

    this.loading.set(true);

    this.doctorService.getDashboard().subscribe({

      next: (dashboard) => {

        this.dashboard.set(dashboard);

        this.loading.set(false);

      },

      error: (error) => {

        console.error('Failed to load doctor dashboard.', error);

        this.loading.set(false);

      }

    });

  }

  refresh(): void {
    this.loadDashboard();
  }

}