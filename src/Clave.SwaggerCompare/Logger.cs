using System;

namespace Clave.SwaggerCompare
{
    internal static class Logger
    {
        public static void LogWarning(string message) => Log(message, ConsoleColor.DarkYellow);
        public static void LogError(string message) => Log(message, ConsoleColor.DarkRed);
        public static void LogResponseDiff(string message) => Log(message, ConsoleColor.Red);
        public static void LogSuccess(string message) => Log(message, ConsoleColor.DarkGreen);
        public static void LogInfo(string message) => Log(message, ConsoleColor.DarkCyan);

        static void Log(string message, ConsoleColor foregroundColor)
        {
            Console.ForegroundColor = foregroundColor;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}