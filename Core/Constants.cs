namespace XPAB;

/// <summary>The constants for each asset type's serialisation version.</summary>
public static class Constants
{
    /// <summary>The default file extension used for native Unity asset files.</summary>
    public const string AssetFileExtension = "asset";

    /// <summary>The magic ASCII string used as a header to mark the start of a valid XPAB file.</summary>
    public const string Header = "XPAB";

    /// <summary>The magic ASCII string used as a footer to mark the end of a valid XPAB file.</summary>
    public const string Footer = "BAPX";

    /// <summary>Gets a dictionary mapping Unity asset types to arrays of their supported file extensions.</summary>
    public static Dictionary<Type, string[]> FileExtensions { get; } = new()
    {
        // TODO: Fill this
    };
}
