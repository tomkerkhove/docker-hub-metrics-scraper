using GuardNet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DockerHub.Metrics.Runtime.AzureFunction.Extensions
{
    public static class ILoggingBuilderExtensions
    {
        public static ILoggingBuilder ClearProvidersExceptFunctionProviders(this ILoggingBuilder loggingBuilder)
        {
            Guard.NotNull(loggingBuilder, nameof(loggingBuilder));

            // Kudos to katrash: https://stackoverflow.com/questions/45986517/remove-console-and-debug-loggers-in-asp-net-core-2-0-when-in-production-mode
            foreach (ServiceDescriptor serviceDescriptor in loggingBuilder.Services)
            {
                if (serviceDescriptor.ServiceType == typeof(ILoggerProvider))
                {
                    if (serviceDescriptor.ImplementationType.FullName != "Microsoft.Azure.WebJobs.Script.Diagnostics.HostFileLoggerProvider"
                        && serviceDescriptor.ImplementationType.FullName != "Microsoft.Azure.WebJobs.Script.Diagnostics.FunctionFileLoggerProvider")
                    {
                        loggingBuilder.Services.Remove(serviceDescriptor);
                        break;
                    }
                }
            }

            return loggingBuilder;
        }
    }
}
