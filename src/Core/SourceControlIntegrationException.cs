using System;

namespace Fettle.Core
{
    public class SourceControlIntegrationException : Exception
    {
        public SourceControlIntegrationException(string message) : base(message)
        {
        }
    }
}
