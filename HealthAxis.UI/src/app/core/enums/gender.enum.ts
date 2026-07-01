// src/app/core/enums/gender.enum.ts
export enum Gender {
  Male = 1,
  Female = 2,
  Other = 3
}

export const GenderLabel: Record<Gender, string> = {
  [Gender.Male]: 'Male',
  [Gender.Female]: 'Female',
  [Gender.Other]: 'Other'
};