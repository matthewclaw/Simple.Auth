using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Auth.Interfaces
{
    public interface ICorrelationService: IHttpContextSwitchable
    {
        string GetCorrelationId();
        string GenerateCorrelationId();
        string SetCorrelationId(string correlationId);
    }
}
