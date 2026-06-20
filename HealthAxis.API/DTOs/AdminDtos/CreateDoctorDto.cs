using HealthAxis.API.Enums;

public class CreateDoctorDto
{
    public string FullName { get; set; }

    public Specialisation Specialisation { get; set; }

    public int YearsOfExperience { get; set; }

    public decimal ConsultationFee { get; set; }

    public string Email { get; set; }

    public string Password { get; set; }

    public string ConfirmPassword { get; set; }
}