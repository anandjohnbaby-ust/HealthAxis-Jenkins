// src/app/features/patient/pages/health-records/health-records.component.ts

import { CommonModule } from '@angular/common';
import {
  Component,
  ElementRef,
  inject,
  OnInit,
  signal,
  effect,
  ViewChild
} from '@angular/core';

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
  // Modal (native <dialog>)
  // ===============================

  @ViewChild('haModal') haModal?: ElementRef<HTMLDialogElement>;

  constructor() {

    // Keeps the native <dialog> element's open state in sync with the
    // selectedRecord signal. showModal() gives us focus trapping,
    // Escape-to-close, and a ::backdrop for free.
    effect(() => {

      const record = this.selectedRecord();
      const dialogEl = this.haModal?.nativeElement;

      if (!dialogEl) {
        return;
      }

      if (record && !dialogEl.open) {
        dialogEl.showModal();
      } else if (!record && dialogEl.open) {
        dialogEl.close();
      }

    });

  }

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

  onDialogClick(event: MouseEvent): void {

    // <dialog> fills the viewport when open, so a click that lands directly
    // on the dialog element itself (not on any child content) means the
    // user clicked the backdrop area — close it, mimicking the old
    // click-outside-to-close behavior.
    if (event.target === event.currentTarget) {
      this.closeModal();
    }

  }

  closeModal(): void {

    this.selectedRecord.set(null);

  }

}