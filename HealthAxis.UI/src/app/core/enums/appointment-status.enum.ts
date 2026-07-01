// src/app/core/enums/appointment-status.enum.ts
export enum AppointmentStatus {
  Pending = 1,
  Confirmed = 2,
  Cancelled = 3,
  Completed = 4
}

export const AppointmentStatusLabel: Record<AppointmentStatus, string> = {
  [AppointmentStatus.Pending]: 'Pending',
  [AppointmentStatus.Confirmed]: 'Confirmed',
  [AppointmentStatus.Cancelled]: 'Cancelled',
  [AppointmentStatus.Completed]: 'Completed'
};