namespace HealthAxis.API.Utilities
{
    public class ValidationLimits
    {
        public const int FullNameLength = 100;

        public const int EmailLength = 100;

        public const int PhoneNumberLength = 20;

        public const int PolicyNumberLength = 50;

        public const int ProviderNameLength = 100;

        public const int TimeSlotLength = 20;

        public const int CancellationReasonLength = 500;

        public const int DiagnosisLength = 500;

        public const int PrescriptionLength = 500;

        public const int NotesLength = 1000;

        public const int MinExperience = 0;

        public const int MaxExperience = 50;

        public const string MinCoverageAmount = "0";

        public const string MaxCoverageAmount = "999999999";

        public const string MinConsultationFee = "0.01";

        public const string MaxConsultationFee = "10000";
    }
}
