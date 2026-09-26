using XPAB;

/// <summary>A scriptable object data class to hold configurations on how the XPAB files are written.</summary>
[SuppressMessage("Design", "CA1050:Declare types in namespaces", Justification = "Unity convention.")]
[SuppressMessage("Major Bug", "S3903:Types should be defined in named namespaces", Justification = "Read above.")]
[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Read above.")]
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:Accessible fields should begin with upper-case letter", Justification = "Read above.")]
public sealed class EditorConfig : ScriptableObject
{
    /// <summary>An array of configurations for each XPAB file.</summary>
    public XPABConfig[] configs = [new()];

    /// <summary>Creates all of the XPAB files based on the set configs in the project.</summary>
    [MenuItem("XPAB/Build Bundles")]
    public static void BuildXPABBundles()
    {
        var configs = AssetDatabase.FindAssets("t:EditorConfig")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Distinct()
            .ToArray();

        if (configs.Length == 0)
        {
            Debug.LogError("There are no configs made to build the bundles.");
            return;
        }

        if (configs.Length > 1)
            Debug.LogWarning("More than one config was found. Using the first config found at: " + configs[0]);

        var editorConfig = AssetDatabase.LoadAssetAtPath<EditorConfig>(configs[0]);

        if (editorConfig.configs.Length == 0)
        {
            Debug.LogError("No configuration data was set for an XPAB file.");
            return;
        }

        foreach (var config in editorConfig.configs)
            XPABWriter.CreateBundle(config);
    }
}
