// src/app/core/interfaces/patient-domain.types.ts

import { AppointmentStatus } from '../enums/appointment-status.enum';
import { Gender } from '../enums/gender.enum';
import { Specialisation } from '../enums/specialisation.enum';

// Re-exporting them so your components only need to import from THIS file
export { AppointmentStatus, Gender, Specialisation };

// =========================================================================
// 1. PATIENT PROFILE
// =========================================================================

export interface Patient {
  patientId: number;
  fullName: string;
  dateOfBirth: string;
  age: number;
  gender: Gender; // Now strongly typed to your Gender enum!
  phoneNumber: string;
  email: string;
  createdDate: string;
}

export interface UpdatePatientRequest {
  fullName?: string;
  dateOfBirth?: string;
  gender?: Gender;
  phoneNumber?: string;
  email?: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

// src/app/core/interfaces/paged-result.interface.ts

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}

// =========================================================================
// 2. DOCTORS
// =========================================================================

/** Full doctor profile — used on the doctor's own dashboard */
export interface DoctorProfile {
  doctorId: number;
  fullName: string;
  specialisation: Specialisation;
  yearsOfExperience: number;
  consultationFee: number;
  isActive: boolean;
  upcomingAppointmentCount: number;
}

/** Matches backend DoctorDto returned by GET /api/patients/available-doctors */
export interface DoctorDto {
  doctorId: number;
  fullName: string;
  specialisation: Specialisation;
  yearsOfExperience: number;
  consultationFee: number;
  isActive: boolean;
  email: string; // <-- add this
}

// =========================================================================
// 3. APPOINTMENTS
// =========================================================================
/** Shape returned by GET /api/appointments */
export interface Appointment {
  appointmentId: number;
  patientId: number;
  doctorId: number;
  doctorName?: string;
  scheduledDate: string; // yyyy-MM-dd
  timeSlot: string;      // HH:mm:ss (TimeOnly from .NET)
  status: AppointmentStatus;
  cancellationReason?: string | null
  healthRecordId?: number | null;
}

/** Exact shape confirmed from POST /api/patients/{id}/appointments */
export interface BookAppointmentRequest {
  patientId: number;
  doctorId: number;
  scheduledDate: string; // yyyy-MM-dd
  timeSlot: string;      // HH:mm:ss (TimeOnly)
}

export interface CancelAppointment {
  cancellationReason: string | null;
}

// =========================================================================
// 4. HEALTH RECORDS
// =========================================================================
export interface HealthRecord {
  recordId: number;       
  appointmentId: number;  
  doctorId: number;       
  doctorName: string;     
  patientId: number;      
  visitDate: string;      
  diagnosis: string;      
  prescription: string;   
  notes: string;          
}

// =========================================================================
// 5. DASHBOARD
// =========================================================================

export interface DashboardAppointment {
  appointmentId: number;
  doctorName: string;
  scheduledDate: string;
  timeSlot: string; // HH:mm:ss (TimeOnly)
  status: AppointmentStatus;
}

export interface PatientDashboard {
  fullName: string;
  totalAppointments: number;
  totalHealthRecords: number;
  nextAppointment: DashboardAppointment | null;
}

export interface TimeSlot {
  value: string;
  label: string;
}