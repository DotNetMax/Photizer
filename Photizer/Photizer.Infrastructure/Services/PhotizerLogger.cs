using Photizer.Domain.Interfaces;
using Serilog;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.EntityFrameworkCore.Destructurers;
using System;
using System.Diagnostics;
using System.IO;

namespace Photizer.Infrastructure.Services
{
    public class PhotizerLogger : IPhotizerLogger
    {
        public PhotizerLogger()
        {
            string logDbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData
                , Environment.SpecialFolderOption.DoNotVerify), "PhotizerData", "Logs.db");
            string logFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData
                , Environment.SpecialFolderOption.DoNotVerify), "PhotizerData", "Log.txt");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.WithThreadId()
                .Enrich.WithExceptionDetails(new DestructuringOptionsBuilder()
                    .WithDefaultDestructurers()
                    .WithDestructurers(new[] { new DbUpdateExceptionDestructurer() }))
                .WriteTo.SQLite(logDbPath)
                .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        public void LogError(string message, Exception exception, params object[] values)
        {
            Debug.WriteLine($"{message}, {exception.Message}");
            Log.Logger.Error(exception, message, values);
        }

        public void LogInformation(string message, params object[] values)
        {
            Debug.WriteLine($"{message}");
            Log.Logger.Information(message, values);
        }
    }
}