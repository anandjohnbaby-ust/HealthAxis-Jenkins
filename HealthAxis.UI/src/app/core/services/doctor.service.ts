// src/app/core/services/doctor.service.ts

import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { API_ENDPOINTS } from '../constants/api-endpoints.constants';

import {
  CreateHealthRecordRequest,
  DoctorAppointmentDto,
  DoctorProfile,
  DoctorDashboard,
  HealthRecordDto,
  UpdateAppointmentStatus,
  UpdateDoctorProfileRequest,
  PagedResult,
  PaginationRequest
} from '../interfaces/doctor-domain.types';
import { HealthRecord } from '../interfaces/patient-domain.types';

@Injectable({
  providedIn: 'root'
})
export class DoctorService {

  private readonly http = inject(HttpClient);
  private readonly apiUrl = environment.apiBaseUrl;

  // ============================
  // Doctors
  // ============================

  getAllDoctors(): Observable<DoctorProfile[]> {
    return this.http.get<DoctorProfile[]>(
      `${this.apiUrl}${API_ENDPOINTS.DOCTORS.BASE}`
    );
  }

// ============================
// Doctor Profile
// ============================

getMyProfile(): Observable<DoctorProfile> {

  return this.http.get<DoctorProfile>(
    `${this.apiUrl}${API_ENDPOINTS.DOCTORS.ME}`
  );
}

updateMyProfile(
  dto: UpdateDoctorProfileRequest
): Observable<DoctorProfile> {

  return this.http.put<DoctorProfile>(
    `${this.apiUrl}${API_ENDPOINTS.DOCTORS.UPDATE_ME}`,
    dto
  );

}

  // ============================
  // Appointments
  // ============================

getAllAppointments(
  request: PaginationRequest
): Observable<PagedResult<DoctorAppointmentDto>> {

  let params = new HttpParams()
    .set('pageNumber', request.pageNumber)
    .set('pageSize', request.pageSize);

  if (request.search?.trim()) {
    params = params.set('search', request.search.trim());
  }

  if (request.status !== null && request.status !== undefined) {
    params = params.set('status', request.status);
  }

  if (request.date) {
    params = params.set('date', request.date);
  }

  return this.http.get<PagedResult<DoctorAppointmentDto>>(
    `${this.apiUrl}${API_ENDPOINTS.DOCTORS.APPOINTMENTS}`,
    { params }
  );
}

getTodaySchedule(
  request: PaginationRequest
): Observable<PagedResult<DoctorAppointmentDto>> {

  let params = new HttpParams()
    .set('pageNumber', request.pageNumber)
    .set('pageSize', request.pageSize);

  if (request.search?.trim()) {
    params = params.set('search', request.search.trim());
  }

  if (request.status !== null && request.status !== undefined) {
    params = params.set('status', request.status);
  }

  return this.http.get<PagedResult<DoctorAppointmentDto>>(
    `${this.apiUrl}${API_ENDPOINTS.DOCTORS.TODAY_SCHEDULE}`,
    { params }
  );
}

getWeeklySchedule(
  request: PaginationRequest
): Observable<PagedResult<DoctorAppointmentDto>> {

  let params = new HttpParams()
    .set('pageNumber', request.pageNumber)
    .set('pageSize', request.pageSize);

  if (request.search?.trim()) {
    params = params.set('search', request.search.trim());
  }

  if (request.status !== null && request.status !== undefined) {
    params = params.set('status', request.status);
  }

  if (request.date) {
    params = params.set('date', request.date);
  }

  return this.http.get<PagedResult<DoctorAppointmentDto>>(
    `${this.apiUrl}${API_ENDPOINTS.DOCTORS.WEEKLY_SCHEDULE}`,
    { params }
  );
}
  getAppointmentDetails(
    appointmentId: number
  ): Observable<DoctorAppointmentDto> {

    return this.http.get<DoctorAppointmentDto>(
      `${this.apiUrl}${API_ENDPOINTS.DOCTORS.APPOINTMENT_DETAILS(appointmentId)}`
    );

  }

  updateAppointmentStatus(
    appointmentId: number,
    payload: UpdateAppointmentStatus
  ): Observable<DoctorAppointmentDto> {

    return this.http.put<DoctorAppointmentDto>(
      `${this.apiUrl}${API_ENDPOINTS.DOCTORS.UPDATE_APPOINTMENT_STATUS(appointmentId)}`,
      payload
    );

  }

  // ============================
  // Health Records
  // ============================

  addHealthRecord(
    dto: CreateHealthRecordRequest
  ): Observable<HealthRecordDto> {

    return this.http.post<HealthRecordDto>(
      `${this.apiUrl}${API_ENDPOINTS.DOCTORS.HEALTH_RECORDS}`,
      dto
    );

  }

  // GET /api/doctors/health-records/{id}
  getHealthRecordById(
    id: number
  ): Observable<HealthRecordDto> {

    return this.http.get<HealthRecordDto>(
      `${this.apiUrl}${API_ENDPOINTS.DOCTORS.GET_HEALTH_RECORD(id)}`
    );

  }
  hasHealthRecord(appt: DoctorAppointmentDto): boolean {

    return appt.healthRecordId != null;

  }

  getPatientHealthHistory(
    patientId: number,
    appointmentId: number
  ): Observable<HealthRecordDto[]> {

    return this.http.get<HealthRecordDto[]>(
      `${this.apiUrl}${API_ENDPOINTS.DOCTORS.PATIENT_HEALTH_HISTORY(
        patientId,
        appointmentId
      )}`
    );

  }

// ============================
// Dashboard
// ============================

  getDashboard(): Observable<DoctorDashboard> {

    return this.http.get<DoctorDashboard>(
      `${this.apiUrl}${API_ENDPOINTS.DOCTORS.DASHBOARD}`
    );

  }
  }