// src/app/features/patient/pages/doctors/doctors.component.ts

import { CommonModule } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

import { PatientService } from '../../../../core/services/patient.service';
import {
  DoctorDto,
  Specialisation
} from '../../../../core/interfaces/patient-domain.types';

import {
  SPECIALISATION_OPTIONS,
  SpecialisationLabel
} from '../../../../core/enums/specialisation.enum';

@Component({
  selector: 'app-doctors',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule
  ],
  templateUrl: './doctors.component.html',
  styleUrl: './doctors.component.css'
})
export class DoctorsComponent implements OnInit {

  private readonly patientService = inject(PatientService);
  private readonly router = inject(Router);

  readonly loading = signal(true);
  readonly doctors = signal<DoctorDto[]>([]);

  // ===============================
  // Pagination
  // ===============================

  readonly pageNumber = signal(1);
  readonly pageSize = signal(10);
  readonly totalCount = signal(0);

  // ===============================

  search = '';
  selectedSpecialisation: Specialisation | null = null;

  readonly specialisationOptions = SPECIALISATION_OPTIONS;

  ngOnInit(): void {
    this.loadDoctors();
  }

  loadDoctors(): void {

    this.loading.set(true);

    this.patientService
      .getAvailableDoctors(
        this.selectedSpecialisation,
        this.pageNumber(),
        this.pageSize(),
        this.search
      )
      .subscribe({
        next: result => {

          this.doctors.set(result.items);
          this.totalCount.set(result.totalCount);

          this.loading.set(false);
        },
        error: () => {
          this.loading.set(false);
        }
      });
  }

  private refreshDoctors(): void {
    this.pageNumber.set(1);
    this.loadDoctors();
  }

  onSearchChange(): void {
    this.refreshDoctors();
  }

  onSpecialisationChange(): void {
    this.refreshDoctors();
  }

  previousPage(): void {

    if (this.pageNumber() > 1) {

      this.pageNumber.update(p => p - 1);

      this.loadDoctors();
    }
  }

  nextPage(): void {

    const totalPages = Math.ceil(
      this.totalCount() / this.pageSize()
    );

    if (this.pageNumber() < totalPages) {

      this.pageNumber.update(p => p + 1);

      this.loadDoctors();
    }
  }

  totalPages(): number {

    return Math.ceil(
      this.totalCount() / this.pageSize()
    );

  }

  specialisationLabel(
    value: Specialisation
  ): string {

    return SpecialisationLabel[value] ?? 'Unknown';

  }

  bookAppointment(
    doctor: DoctorDto
  ): void {

    this.router.navigate(
      ['/patient/appointments/book'],
      {
        queryParams: {
          doctorId: doctor.doctorId,
          specialisation: doctor.specialisation
        }
      }
    );
  }

  resetFilters(): void {

    this.search = '';
    this.selectedSpecialisation = null;

    this.pageNumber.set(1);

    this.loadDoctors();
  }
}