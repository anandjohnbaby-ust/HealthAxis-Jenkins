// src/app/core/services/patient.service.ts
import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';

import { environment } from '../../../environments/environment';
import { API_ENDPOINTS } from '../constants/api-endpoints.constants';

// Patient Interfaces
import { Specialisation } from '../enums/specialisation.enum';
import { Appointment, BookAppointmentRequest, CancelAppointment, DoctorDto, HealthRecord, PagedResult, Patient, PatientDashboard, TimeSlot, UpdatePatientRequest } from '../interfaces/patient-domain.types';

// Health Record Interfaces

@Injectable({
  providedIn: 'root'
})
export class PatientService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl;

  /**
   * Holds the currently logged-in patient's profile.
   * Components can subscribe to this signal for live updates.
   */
  readonly currentPatient = signal<Patient | null>(null);

  // =========================================================================
  // 1. PROFILE MANAGEMENT
  // =========================================================================

  /** GET /api/patients/me */
  getMyProfile(): Observable<Patient> {
    return this.http
      .get<Patient>(`${this.baseUrl}${API_ENDPOINTS.PATIENTS.ME}`)
      .pipe(
        tap((patient) => {
          this.currentPatient.set(patient);
        })
      );
  }

  /** PUT /api/patients/me */
  updateMyProfile(payload: UpdatePatientRequest): Observable<Patient> {
    return this.http
      .put<Patient>(`${this.baseUrl}${API_ENDPOINTS.PATIENTS.UPDATE_ME}`, payload)
      .pipe(
        tap((patient) => {
          this.currentPatient.set(patient);
        })
      );
  }

  // =========================================================================
  // 2. APPOINTMENT MANAGEMENT
  // =========================================================================

getMyAppointments(
  patientId: number,
  pageNumber: number,
  pageSize: number,
  search?: string,
  status?: number | null,
  date?: string | null
): Observable<PagedResult<Appointment>> {

  let params = new HttpParams()
    .set('pageNumber', pageNumber)
    .set('pageSize', pageSize);

  if (search?.trim()) {
    params = params.set('search', search.trim());
  }

  if (status !== null && status !== undefined) {
    params = params.set('status', status);
  }

  if (date) {
    params = params.set('date', date);
  }

  return this.http.get<PagedResult<Appointment>>(
    `${this.baseUrl}${API_ENDPOINTS.PATIENTS.GET_APPOINTMENTS(patientId)}`,
    { params }
  );
}
  /** POST /api/patients/{patientId}/book-appointments */
  bookAppointment(patientId: number, payload: BookAppointmentRequest): Observable<Appointment> {
    return this.http.post<Appointment>(
      `${this.baseUrl}${API_ENDPOINTS.PATIENTS.BOOK_APPOINTMENT(patientId)}`,
      payload
    );
  }

  /** PUT /api/patients/{patientId}/appointments/{appointmentId}/cancel */
  cancelAppointment(patientId: number, appointmentId: number, payload: CancelAppointment): Observable<Appointment> {
    return this.http.put<Appointment>(
      `${this.baseUrl}${API_ENDPOINTS.PATIENTS.CANCEL_APPOINTMENT(patientId, appointmentId)}`,
      payload
    );
  }

  // =========================================================================
  // 3. DOCTOR DISCOVERY
  // =========================================================================

  /** GET /api/patients/available-doctors */
  getAvailableDoctors(
    specialisation: Specialisation | null,
    pageNumber: number,
    pageSize: number,
    search?: string
  ): Observable<PagedResult<DoctorDto>> {

    let params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);

    if (specialisation !== null) {
      params = params.set('specialisation', specialisation);
    }

    if (search && search.trim().length > 0) {
      params = params.set('search', search.trim());
    }

    return this.http.get<PagedResult<DoctorDto>>(
      `${this.baseUrl}${API_ENDPOINTS.PATIENTS.AVAILABLE_DOCTORS}`,
      { params }
    );
  }

  // =========================================================================
  // 4. HEALTH RECORDS
  // =========================================================================

/** GET /api/patients/{patientId}/health-records */
getHealthRecordsByPatientId(
  patientId: number,
  pageNumber: number,
  pageSize: number
): Observable<PagedResult<HealthRecord>> {

  const params = new HttpParams()
    .set('pageNumber', pageNumber)
    .set('pageSize', pageSize);

  return this.http.get<PagedResult<HealthRecord>>(
    `${this.baseUrl}${API_ENDPOINTS.HEALTH_RECORDS.BY_PATIENT(patientId)}`,
    { params }
  );

}

  // =========================================================================
  // 5. DASHBOARD
  // =========================================================================

  /** GET /api/patients/dashboard */
  getDashboard(): Observable<PatientDashboard> {
    return this.http.get<PatientDashboard>(
      `${this.baseUrl}${API_ENDPOINTS.PATIENTS.DASHBOARD}`
    );
  }

  getAvailableTimeSlots(
    doctorId: number,
    date: string
  ): Observable<TimeSlot[]> {

    const params = new HttpParams()
      .set('doctorId', doctorId)
      .set('date', date);

    return this.http.get<TimeSlot[]>(
      `${this.baseUrl}${API_ENDPOINTS.APPOINTMENTS.AVAILABLE_TIME_SLOTS}`,
      { params }
    );
  }
}