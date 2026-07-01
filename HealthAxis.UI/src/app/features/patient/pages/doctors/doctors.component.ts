// src/app/features/patient/pages/doctors/doctors.component.ts

import { CommonModule } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

import { PatientService } from '../../../../core/services/patient.service';
import { DoctorDto, Specialisation } from '../../../../core/interfaces/patient-domain.types';
import { SPECIALISATION_OPTIONS, SpecialisationLabel } from '../../../../core/enums/specialisation.enum';

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

  search = '';
  selectedSpecialisation: Specialisation | null = null;

  // Use the pre-built options array from your enum file
  readonly specialisationOptions = SPECIALISATION_OPTIONS;

  ngOnInit(): void {
    this.loadDoctors();
  }

  loadDoctors(): void {
    this.loading.set(true);

    this.patientService
      .getAvailableDoctors(this.selectedSpecialisation, this.search)
      .subscribe({
        next: doctors => {
          this.doctors.set(doctors);
          this.loading.set(false);
        },
        error: () => {
          this.loading.set(false);
        }
      });
  }

  onSearchChange(): void {
    this.loadDoctors();
  }

  onSpecialisationChange(): void {
    this.loadDoctors();
  }

  specialisationLabel(value: Specialisation): string {
    // Map the numeric enum to the readable label
    return SpecialisationLabel[value] ?? 'Unknown';
  }

  bookAppointment(doctor: DoctorDto): void {
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
    this.loadDoctors();
  }
}