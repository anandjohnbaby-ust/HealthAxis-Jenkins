// // src/app/core/constants/api-endpoints.constants.ts

// export const API_ENDPOINTS = {
//   AUTH: {
//     REGISTER: '/Auth/register',
//     LOGIN: '/Auth/login',
//     REFRESH_TOKEN: '/Auth/refresh-token',
//     CHANGE_PASSWORD: '/Auth/change-password'
//   },

// DOCTORS: {
//   BASE: '/doctors',

//   BY_ID: (id: number | string) =>
//     `/doctors/${id}`,

//   UPDATE: (id: number | string) =>
//     `/doctors/${id}`,

//   // ==========================
//   // Doctor Profile
//   // ==========================

//   ME: '/doctors/me',

//   UPDATE_ME: '/doctors/me',

//   AVAILABILITY: (id: number | string) =>
//     `/doctors/${id}/availability`,

//   // ==========================
//   // Doctor Appointments
//   // ==========================

//   APPOINTMENTS: '/doctors/appointments',

//   TODAY_SCHEDULE: '/doctors/schedule/today',

//   WEEKLY_SCHEDULE: '/doctors/schedule/week',

//   APPOINTMENT_DETAILS: (id: number | string) =>
//     `/doctors/appointments/${id}`,

//   UPDATE_APPOINTMENT_STATUS: (id: number | string) =>
//     `/doctors/appointments/${id}/status`,

//   // ==========================
//   // Doctor Health Records
//   // ==========================

//   HEALTH_RECORDS: '/doctors/health-records',

//   GET_HEALTH_RECORD: (id: number | string) =>
//     `/doctors/health-records/${id}`
// },

// HEALTH_RECORDS: {
//   BASE: '/health-records',

//   BY_ID: (id: number | string) =>
//     `/health-records/${id}`,

//   BY_PATIENT: (patientId: number | string) =>
//     `/patients/${patientId}/health-records`
// },

// PATIENTS: {
//   GET_APPOINTMENTS: (patientId: number | string) =>
//     `/patients/${patientId}/appointments`,

//   BOOK_APPOINTMENT: (patientId: number | string) =>
//     `/patients/${patientId}/book-appointments`,

//   CANCEL_APPOINTMENT: (
//     patientId: number | string,
//     appointmentId: number | string
//   ) =>
//     `/patients/${patientId}/appointments/${appointmentId}/cancel`,

//   AVAILABLE_DOCTORS: '/patients/available-doctors',

//   DASHBOARD: '/patients/dashboard',

//   ME: '/patients/me',

//   UPDATE_ME: '/patients/me'
// },

//   APPOINTMENTS: {
//     BASE: '/appointments',

//     UPDATE_STATUS: (id: number | string) =>
//       `/appointments/${id}/status`,

//     DELETE: (id: number | string) =>
//       `/appointments/${id}`
//   },

//   ADMIN: {
//     DOCTORS: '/admin/doctors',

//     DOCTOR_BY_ID: (id: number | string) =>
//       `/admin/doctors/${id}`,

//     REPORTS_APPOINTMENTS: '/admin/reports/appointments',

//     USERS: '/admin/users',

//     DASHBOARD: '/admin/dashboard'
//   }

// } as const;

// src/app/core/constants/api-endpoints.constants.ts

export const API_ENDPOINTS = {
  AUTH: {
    REGISTER: '/Auth/register',
    LOGIN: '/Auth/login',
    REFRESH_TOKEN: '/Auth/refresh-token',
    CHANGE_PASSWORD: '/Auth/change-password'
  },

  DOCTORS: {
    BASE: '/doctors',

    BY_ID: (id: number | string) =>
      `/doctors/${id}`,

    UPDATE: (id: number | string) =>
      `/doctors/${id}`,

    // ==========================
    // Doctor Profile
    // ==========================

    ME: '/doctors/me',

    UPDATE_ME: '/doctors/me',

    DASHBOARD: '/doctors/dashboard',

    AVAILABILITY: (id: number | string) =>
      `/doctors/${id}/availability`,

    // ==========================
    // Doctor Appointments
    // ==========================

    APPOINTMENTS: '/doctors/appointments',

    TODAY_SCHEDULE: '/doctors/schedule/today',

    WEEKLY_SCHEDULE: '/doctors/schedule/week',

    APPOINTMENT_DETAILS: (id: number | string) =>
      `/doctors/appointments/${id}`,

    UPDATE_APPOINTMENT_STATUS: (id: number | string) =>
      `/doctors/appointments/${id}/status`,

    // ==========================
    // Doctor Health Records
    // ==========================

    HEALTH_RECORDS: '/doctors/health-records',

    GET_HEALTH_RECORD: (id: number | string) =>
      `/doctors/health-records/${id}`
  },

  HEALTH_RECORDS: {
    BASE: '/health-records',

    BY_ID: (id: number | string) =>
      `/health-records/${id}`,

    BY_PATIENT: (patientId: number | string) =>
      `/patients/${patientId}/health-records`
  },

  PATIENTS: {
    GET_APPOINTMENTS: (patientId: number | string) =>
      `/patients/${patientId}/appointments`,

    BOOK_APPOINTMENT: (patientId: number | string) =>
      `/patients/${patientId}/book-appointments`,

    CANCEL_APPOINTMENT: (
      patientId: number | string,
      appointmentId: number | string
    ) =>
      `/patients/${patientId}/appointments/${appointmentId}/cancel`,

    AVAILABLE_DOCTORS: '/patients/available-doctors',

    DASHBOARD: '/patients/dashboard',

    ME: '/patients/me',

    UPDATE_ME: '/patients/me'
  },

  APPOINTMENTS: {
    BASE: '/appointments',

    UPDATE_STATUS: (id: number | string) =>
      `/appointments/${id}/status`,

    DELETE: (id: number | string) =>
      `/appointments/${id}`
  },

  ADMIN: {
    DOCTORS: '/admin/doctors',

    DOCTOR_BY_ID: (id: number | string) =>
      `/admin/doctors/${id}`,

    REPORTS_APPOINTMENTS: '/admin/reports/appointments',

    USERS: '/admin/users',

    DASHBOARD: '/admin/dashboard'
  }

} as const;