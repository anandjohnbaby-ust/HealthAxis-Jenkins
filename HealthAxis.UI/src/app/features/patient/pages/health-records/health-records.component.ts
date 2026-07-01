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

  ngOnInit(): void {
    const token = this.tokenService.getAccessToken() ?? '';
    const patientId = this.tokenService.getPatientIdFromToken(token);

    if (patientId === null) {
      this.loading.set(false);
      return;
    }

    this.patientService.getHealthRecordsByPatientId(patientId).subscribe({
      next: (records: HealthRecord[]) => {
        this.records.set(records);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Failed to load health records:', error);
        this.loading.set(false);
      }
    });
  }

  viewRecord(record: HealthRecord): void {
    this.selectedRecord.set(record);
  }

  closeModal(): void {
    this.selectedRecord.set(null);
  }
}