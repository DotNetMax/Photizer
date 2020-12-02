using System;

namespace Photizer.Domain.Interfaces
{
    public interface IPhotizerLogger
    {
        void LogInformation(string message, params object[] values);

        void LogError(string message, Exception exception, params object[] values);
    }
}