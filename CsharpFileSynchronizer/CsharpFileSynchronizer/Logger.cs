using System;
using Microsoft.Extensions.Logging;
using Serilog;

namespace CsharpFileSynchronizer
{
    public class Logger
    {
        private readonly ILogger<Logger> _logger;

        // Expose the ILogger<Logger> instance via a public property
        public ILogger<Logger> LoggerInstance => _logger;

        public Logger(string fileName)
        {
            // Configure Serilog to log to both console and file
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information() // Set minimum log level
                .WriteTo.Console()           // Log to console
                .WriteTo.File(fileName)     // Log to file
                .CreateLogger();

            // Integrate Serilog with Microsoft.Extensions.Logging
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddSerilog();        // Use Serilog as the logging provider
            });

            // Create a logger for the current class
            _logger = loggerFactory.CreateLogger<Logger>();
        }

        public void LogExampleMessages()
        {
            _logger.LogInformation("This is an informational message.");
            _logger.LogWarning("This is a warning message.");
            _logger.LogError("This is an error message.");
        }
    }
}
