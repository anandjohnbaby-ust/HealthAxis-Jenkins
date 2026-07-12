// src/app/features/patient/pages/health-records/health-records.component.ts

import { CommonModule } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';

import { TokenService } from '../../../../core/services/token.service';
import { PatientService } from '../../../../core/services/patient.service';
import { HealthRecord } from '../../../../core/interfaces/patient-domain.types';

@Component({
  selector: 'app-health-records',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './health-records.component.html',
  styleUrl: './health-records.component.css'
})
export class HealthRecordsComponent implements OnInit {

  private readonly patientService = inject(PatientService);
  private readonly tokenService = inject(TokenService);

  readonly loading = signal(true);
  readonly records = signal<HealthRecord[]>([]);
  readonly selectedRecord = signal<HealthRecord | null>(null);

  // ===============================
  // Pagination
  // ===============================

  readonly pageNumber = signal(1);
  readonly pageSize = signal(10);
  readonly totalCount = signal(0);

  ngOnInit(): void {
    this.loadHealthRecords();
  }

  private loadHealthRecords(): void {

    this.loading.set(true);

    const token = this.tokenService.getAccessToken() ?? '';
    const patientId = this.tokenService.getPatientIdFromToken(token);

    if (patientId === null) {
      this.loading.set(false);
      return;
    }

    this.patientService
      .getHealthRecordsByPatientId(
        patientId,
        this.pageNumber(),
        this.pageSize()
      )
      .subscribe({
        next: result => {

          this.records.set(result.items);
          this.totalCount.set(result.totalCount);

          this.loading.set(false);

        },
        error: error => {

          console.error('Failed to load health records:', error);

          this.loading.set(false);

        }
      });

  }

  previousPage(): void {

    if (this.pageNumber() > 1) {

      this.pageNumber.update(page => page - 1);

      this.loadHealthRecords();

    }

  }

  nextPage(): void {

    if (this.pageNumber() < this.totalPages()) {

      this.pageNumber.update(page => page + 1);

      this.loadHealthRecords();

    }

  }

  totalPages(): number {

    return Math.ceil(
      this.totalCount() / this.pageSize()
    );

  }

  viewRecord(record: HealthRecord): void {

    this.selectedRecord.set(record);

  }

  closeModal(): void {

    this.selectedRecord.set(null);

  }

}