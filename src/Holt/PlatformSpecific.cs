using Microsoft.Extensions.Logging;
using System;
using System.Runtime.Versioning;

namespace Holt
{
    internal static partial class PlatformSpecific
    {
        public static void ConfigureLogging(ILoggingBuilder loggingBuilder)
        {
            if(OperatingSystem.IsWindows())
                ConfigureLoggingWindows( loggingBuilder );
            else if(OperatingSystem.IsLinux())
                ConfigureLoggingLinux( loggingBuilder );
        }

        [SupportedOSPlatform("windows")]
        private static void ConfigureLoggingWindows( ILoggingBuilder loggingBuilder )
        {
            _ = loggingBuilder.AddEventLog( config =>
            {
                config.SourceName = "Holt";
            } );
        }

        [SupportedOSPlatform("linux")]
        private static void ConfigureLoggingLinux(ILoggingBuilder loggingBuilder)
        {
            loggingBuilder.AddEventSourceLogger();
        }
    }
}
