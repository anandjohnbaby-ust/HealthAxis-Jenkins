// src/app/core/interfaces/doctor-domain.types.ts

// The primary object returned by your API
// Rename 'Appointment' to 'DoctorAppointmentDto'
// export interface DoctorAppointmentDto {
//   appointmentId: number;
//   patientId: number;
//   patientName: string;
//   doctorId: number;
//   doctorName: string;
//   scheduledDate: string;
//   timeSlot: string;
//   status: number;
//   cancellationReason?: string | null;
// }
import { AppointmentStatus } from '../enums/appointment-status.enum';


export interface DoctorAppointmentDto {
  appointmentId: number;
  patientId: number;
  patientName: string;
  doctorId: number;
  doctorName: string;
  scheduledDate: string;
  timeSlot: string;
  status: number;
  cancellationReason?: string | null;

  healthRecordId?: number | null;
}

// The object required to modify an appointment
export interface UpdateAppointmentStatus {
  status: number;           // Mapped to AppointmentStatus enum
  cancellationReason?: string | null;
}

export interface DoctorProfile {
  doctorId: number;
  fullName: string;
  email: string;
  specialisation: number;
  yearsOfExperience: number;
  consultationFee: number;
}

export interface UpdateDoctorProfileRequest {
  fullName: string;
  specialisation: number;
  yearsOfExperience: number;
  consultationFee: number;
}

// ===============================
// Health Record
// ===============================

export interface CreateHealthRecordRequest {
  appointmentId: number;
  diagnosis: string;
  prescription: string;
  notes?: string | null;
}

export interface HealthRecordDto {
  recordId: number;
  appointmentId: number;
  patientId: number;
  doctorId: number;
  visitDate: string;
  diagnosis: string;
  prescription: string;
  notes?: string | null;
}

export interface TodayAppointment {
  appointmentId: number;
  patientName: string;
  scheduledDate: string;
  timeSlot: string;
  status: AppointmentStatus;
}

export interface DoctorDashboard {
  fullName: string;
  todayAppointments: number;
  weeklyAppointments: number;
  totalAppointments: number;
  todaySchedule: TodayAppointment[];
}