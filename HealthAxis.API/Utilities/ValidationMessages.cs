namespace HealthAxis.API.Utilities
{
    public static class ValidationMessages
    {

        public const string FullNameRequired = "Full Name is required.";

        public const string InvalidFullNameFormat = "Full Name must contain only alphabets and spaces.";

        public const string DateOfBirthRequired = "Date Of Birth is required.";

        public const string DateOfBirthCannotBeFuture = "Date Of Birth cannot be in the future.";

        public const string DateOfBirthYearMustBe1900OrLater = "Date of birth year must be 1900 or later.";

        public const string GenderRequired = "Gender is required.";

        public const string PhoneNumberRequired = "Phone Number is required.";

        public const string InvalidPhoneNumberFormat = "Phone Number must contain exactly 10 digits.";

        public const string EmailRequired = "Email is required.";

        public const string InvalidEmailFormat = "Please enter a valid email address.";

        public const string SpecialisationRequired = "Specialisation is required.";

        public const string InvalidExperienceRange = "Years Of Experience must be between 0 and 50.";

        public const string InvalidConsultationFee = "Consultation Fee must be between 0 and 10000.";

        public const string PatientRequired = "Patient details are required.";

        public const string DoctorRequired = "Doctor details are required.";

        public const string ScheduledDateRequired = "Scheduled Date is required.";

        public const string TimeSlotRequired = "Time Slot is required.";

        public const string AppointmentStatusRequired = "Appointment Status is required.";

        public const string VisitDateRequired = "Visit Date is required.";

        public const string DiagnosisRequired = "Diagnosis is required.";

        public const string PrescriptionRequired = "Prescription is required.";

        public const string ProviderNameRequired = "Provider Name is required.";

        public const string PolicyNumberRequired = "Policy Number is required.";

        public const string ExpiryDateRequired = "Expiry Date is required.";

        public const string InsuranceStatusRequired = "Insurance Status is required.";

        public const string PasswordRequired = "Password is required.";

        public const string UserRoleRequired = "User Role is required.";

        public const string ReferenceIdRequired = "Reference Id is required.";

        public const string ScheduledDateCannotBePast = "Sheduled Date cannot be in the past.";

        public const string AppointmentDateRequired = "Appointment Date is required.";

        public const string InvalidTimeSlot = "Invalid Time Slot format. Expected format: HH:mm-HH:mm.";

        public const string AppointmentRequired = "Appointment details are required.";

    }
}
