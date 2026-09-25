namespace XPAB.Internal;

/// <summary>Logging channel for the XPAB handler.</summary>
public static class XPABLogger
{
    private static Action<object?> debugA = Console.WriteLine;
    private static Action<object?> infoA = Console.WriteLine;
    private static Action<object?> warningA = Console.WriteLine;
    private static Action<object?> errorA = Console.WriteLine;

    /// <summary>Sets the console debug action.</summary>
    /// <param name="debug">The new action.</param>
    public static void SetDebug(Action<object?>? debug) => debugA = debug ?? Console.WriteLine;

    /// <summary>Sets the console info action.</summary>
    /// <param name="info">The new action.</param>
    public static void SetInfo(Action<object?>? info) => infoA = info ?? Console.WriteLine;

    /// <summary>Sets the console warning action.</summary>
    /// <param name="warning">The new action.</param>
    public static void SetWarning(Action<object?>? warning) => warningA = warning ?? Console.WriteLine;

    /// <summary>Sets the console error action.</summary>
    /// <param name="error">The new action.</param>
    public static void SetError(Action<object?>? error) => errorA = error ?? Console.WriteLine;

    /// <summary>Sets the console actions.</summary>
    /// <param name="info">The new info action.</param>
    /// <param name="warning">The new warning action.</param>
    /// <param name="error">The new error action.</param>
    public static void SetAll(Action<object?>? info, Action<object?>? warning, Action<object?>? error) => SetAll(info, info, warning, error);

    /// <summary>Sets the console actions.</summary>
    /// <param name="debug">The new debug action.</param>
    /// <param name="info">The new info action.</param>
    /// <param name="warning">The new warning action.</param>
    /// <param name="error">The new error action.</param>
    public static void SetAll(Action<object?>? debug, Action<object?>? info, Action<object?>? warning, Action<object?>? error)
    {
        SetDebug(debug);
        SetInfo(info);
        SetWarning(warning);
        SetError(error);
    }

    internal static void Debug(object? message) => Log(debugA, message);

    internal static void Info(object? message) => Log(infoA, message);

    internal static void Warning(object? message) => Log(warningA, message);

    internal static void Error(object? message) => Log(errorA, message);

    private static void Log(Action<object?> logger, object? message) => logger(message?.ToString() ?? "message was null");
}
