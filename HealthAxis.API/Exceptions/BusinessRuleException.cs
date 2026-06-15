namespace HealthAxis.API.Exceptions
{
    public class BusinessRuleException : AppException
    {
        public BusinessRuleException(string message)
            : base(message)
        {
        }
    }
}