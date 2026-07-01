// src/app/core/constants/app-routes.constants.ts
export const APP_ROUTES = {
  LANDING: '',
  LOGIN: 'login',
  REGISTER: 'register',
  PATIENT: {
    ROOT: 'patient',
    HEALTH_RECORDS: 'health-records'
  },
  DOCTOR: {
    ROOT: 'doctor',
    PROFILE: 'profile'
  },
  UNAUTHORIZED: 'unauthorized',
  NOT_FOUND: 'not-found'
} as const;