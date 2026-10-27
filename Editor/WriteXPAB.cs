using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using XPAB.Internal;
using XPAB.Settings;
using XPAB.Writer;

namespace XPAB;

/// <summary>A utility class that writes an XPAB file.</summary>
public static class WriteXPAB
{
    private const BuildAssetBundleOptions ABConfigs = BuildAssetBundleOptions.DisableWriteTypeTree |
                                                      BuildAssetBundleOptions.DisableLoadAssetByFileName |
                                                      BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension |
                                                      BuildAssetBundleOptions.ForceRebuildAssetBundle |
                                                      BuildAssetBundleOptions.AssetBundleStripUnityVersion;

    private static readonly string OutputPath = Path.Combine("Assets", "StreamingAssets", "XPAB");
    private static readonly string TempPath = Path.Combine("Temp", "XPAB");

    /// <summary>Creates all of the XPAB files based on the set configs in the project.</summary>
    [MenuItem("XPAB/Build Bundles")]
    public static void BuildXPABBundles()
    {
        XPABLogger.SetAll(Debug.Log, Debug.LogWarning, Debug.LogError);

        if (!Directory.Exists(OutputPath))
            Directory.CreateDirectory(OutputPath);

        var configs = AssetDatabase.FindAssets("t:XPABConfig")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<XPABConfig>)
            .ToArray();

        if (configs.Length == 0)
        {
            XPABLogger.Error("There are no configs made to build the bundles.");
            return;
        }

        try
        {
            foreach (var config in configs)
                CreateBundle(config);
        }
        finally
        {
            AssetDatabase.Refresh();
        }
    }

    /// <summary>Creates an XPAB bundle it in the output path.</summary>
    /// <param name="config">The configuration that dictates the serialisation behaviour.</param>
    public static void CreateBundle(XPABConfig config)
    {
        var dirPath = config.filePath?.Length is > 0 ? Path.Combine(config.filePath) : OutputPath;

        try
        {
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);
        }
        catch
        {
            dirPath = OutputPath;
        }

        var sanitisedName = string.Join("_", config.fileName.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));

        if (string.IsNullOrEmpty(sanitisedName))
            throw new ArgumentException("Received a config file name that was either empty or contained only invalid file path characters.");

        var temp = Path.Combine(TempPath, $"temp_{sanitisedName}");
        var filePath = Path.Combine(dirPath, $"{sanitisedName}.xpab");

        try
        {
            PrepDirectory(temp);

            var activeTargets = config.targets?.Distinct()?.ToArray() ?? [];

            config.GetAllAssetPaths();

            IAssetEntry[] bundleAssets = [.. config.shaders];

            if (config.imageSettings.encoding == Texture2DEncoding.Native)
                bundleAssets = [.. bundleAssets, .. config.textures];

            var bundlePaths = bundleAssets
                .Select(x => x.AssetPath)
                .Where(path => !string.IsNullOrEmpty(path))
                .ToArray();

            var hasBundles = bundlePaths.Length > 0 && activeTargets.Length > 0;

            if (bundlePaths.Length > 0 && activeTargets.Length == 0)
                XPABLogger.Warning($"Bundle assets were assigned for {sanitisedName}, but no target platforms were configured. Asset bundle building will be skipped.");

            if (bundlePaths.Length == 0 && activeTargets.Length > 0)
                XPABLogger.Warning($"Bundle platform targets for {sanitisedName} were defined, but no platform-dependent assets were assigned. Asset bundle building will be skipped.");

            var bundles = new Dictionary<TargetPlatform, string>();
            var bundleOptions = ABConfigs | config.bundleCompression.GetCompression();

            if (hasBundles)
            {
                foreach (var target in activeTargets)
                    BuildTempAssetBundle(temp, sanitisedName, bundlePaths, target, bundles, bundleOptions);
            }

            using var file = File.Create(filePath);
            using var sha256 = SHA256.Create();
            using var stream = new CryptoStream(file, sha256, CryptoStreamMode.Write, true);
            using var writer = new XPABWriter(stream, true);

            writer.Write(Encoding.ASCII.GetBytes(Constants.Header));
            writer.Write(sanitisedName);

            if (hasBundles)
            {
                writer.WritePacked((uint)activeTargets.Length);

                foreach (var (target, path) in bundles)
                {
                    writer.WritePacked((uint)target);
                    writer.WriteFile(path);
                }
            }
            else
            {
                writer.WritePacked(0u);
            }

            foreach (var settings in config.GetAllSettings())
                settings.WriteTo(writer);

            // TODO: String Pool, Assets and Master TOC

            writer.Write(Encoding.ASCII.GetBytes(Constants.Footer));

            writer.Flush();
            stream.FlushFinalBlock();

            file.Write(sha256.Hash);
        }
        finally
        {
            if (Directory.Exists(temp))
                Directory.Delete(temp, true);
        }
    }

    private static void BuildTempAssetBundle(string outputDir, string name, string[] assetPaths, TargetPlatform target, Dictionary<TargetPlatform, string> bundles, BuildAssetBundleOptions options)
    {
        var sf = target.GetShortForm();
        var dir = Path.Combine(outputDir, sf);
        var buildMap = new AssetBundleBuild
        {
            assetBundleName = $"{name}.bundle_{sf}",
            assetNames = assetPaths,
        };
        PrepDirectory(dir);

        var manifest = BuildPipeline.BuildAssetBundles(dir, [buildMap], options, target.GetBuildTarget());
        var filePath = Path.Combine(dir, buildMap.assetBundleName);

        if (manifest == null || !File.Exists(filePath))
            throw new SerializationException($"Failed to build platform bundle for {target}.");

        bundles[target] = filePath;
    }

    private static void PrepDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            var dirInfo = new DirectoryInfo(path);

            foreach (var file in dirInfo.GetFiles())
                file.Delete();

            foreach (var dir in dirInfo.GetDirectories())
                dir.Delete(true);
        }
        else
        {
            Directory.CreateDirectory(path);
        }
    }
}
