namespace Simple.Auth.Interfaces
{
    public interface ICorrelationService : IHttpContextSwitchable
    {
        string GetCorrelationId();

        string GenerateCorrelationId();

        void SetCorrelationId(string correlationId);
    }
}