// src/app/core/enums/specialisation.enum.ts
export enum Specialisation {
  Cardiology = 1,
  Neurology,
  Dermatology,
  Orthopedics,
  Pediatrics,
  Gynecology,
  Oncology,
  Psychiatry,
  Ophthalmology,
  ENT,
  Pulmonology,
  Gastroenterology,
  Nephrology,
  Urology,
  Endocrinology,
  Radiology,
  GeneralSurgery,
  Anesthesiology,
  EmergencyMedicine,
  GeneralMedicine
}

export const SpecialisationLabel: Record<Specialisation, string> = {
  [Specialisation.Cardiology]: 'Cardiology',
  [Specialisation.Neurology]: 'Neurology',
  [Specialisation.Dermatology]: 'Dermatology',
  [Specialisation.Orthopedics]: 'Orthopedics',
  [Specialisation.Pediatrics]: 'Pediatrics',
  [Specialisation.Gynecology]: 'Gynecology',
  [Specialisation.Oncology]: 'Oncology',
  [Specialisation.Psychiatry]: 'Psychiatry',
  [Specialisation.Ophthalmology]: 'Ophthalmology',
  [Specialisation.ENT]: 'ENT',
  [Specialisation.Pulmonology]: 'Pulmonology',
  [Specialisation.Gastroenterology]: 'Gastroenterology',
  [Specialisation.Nephrology]: 'Nephrology',
  [Specialisation.Urology]: 'Urology',
  [Specialisation.Endocrinology]: 'Endocrinology',
  [Specialisation.Radiology]: 'Radiology',
  [Specialisation.GeneralSurgery]: 'General Surgery',
  [Specialisation.Anesthesiology]: 'Anesthesiology',
  [Specialisation.EmergencyMedicine]: 'Emergency Medicine',
  [Specialisation.GeneralMedicine]: 'General Medicine'
};

/** Flattened list for populating a <select> dropdown */
export const SPECIALISATION_OPTIONS = Object.entries(SpecialisationLabel).map(
  ([value, label]) => ({
    value: Number(value) as Specialisation,
    label
  })
);