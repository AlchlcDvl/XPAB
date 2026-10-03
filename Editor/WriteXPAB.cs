using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using XPAB.Writer;

namespace XPAB;

/// <summary>A utility class that writes an XPAB file.</summary>
public static class WriteXPAB
{
    private const BuildAssetBundleOptions ABConfigs = BuildAssetBundleOptions.ChunkBasedCompression |
                                                      BuildAssetBundleOptions.DisableWriteTypeTree |
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
        if (!Directory.Exists(OutputPath))
            Directory.CreateDirectory(OutputPath);

        var configs = AssetDatabase.FindAssets("t:XPABConfig")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<XPABConfig>)
            .ToArray();

        if (configs.Length == 0)
        {
            Debug.LogError("There are no configs made to build the bundles.");
            return;
        }

        foreach (var config in configs)
            CreateBundle(config);

        AssetDatabase.Refresh();
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

        var sanitizedName = string.Join("_", config.fileName.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
        var temp = Path.Combine(TempPath, $"temp_{sanitizedName}");
        var filePath = Path.Combine(dirPath, $"{sanitizedName}.xpab");

        try
        {
            PrepDirectory(temp);

            var activeTargets = config.targets?.Distinct()?.ToArray() ?? [];

            var assets = (config.assetsToBundle ?? [])
                .Where(obj => obj != null)
                .Select(AssetDatabase.GetAssetPath)
                .Where(path => !string.IsNullOrEmpty(path))
                .Distinct()
                .ToLookup(OtherExtensions.ShouldBundleAsset);

            var bundleAssets = assets[true].ToArray();
            var genericAssets = assets[false].ToArray();

            var hasBundles = bundleAssets.Length > 0 && activeTargets.Length > 0;

            if (bundleAssets.Length > 0 && activeTargets.Length == 0)
                Debug.LogWarning($"Bundle assets were assigned for {sanitizedName}, but no target platforms were configured. Asset bundle building will be skipped.");

            if (bundleAssets.Length == 0 && activeTargets.Length > 0)
                Debug.LogWarning($"Bundle platform targets for {sanitizedName} were defined, but no platform-dependent assets were assigned. Asset bundle building will be skipped.");

            var bundles = new Dictionary<TargetPlatform, string>();

            if (hasBundles)
            {
                foreach (var target in activeTargets)
                    BuildTempAssetBundle(temp, sanitizedName, bundleAssets, target, bundles);
            }

            using var file = File.Create(filePath);
            using var sha256 = SHA256.Create();
            using var stream = new CryptoStream(file, sha256, CryptoStreamMode.Write, true);
            using var writer = new XPABWriter(stream, true);

            writer.Write(Encoding.ASCII.GetBytes(Constants.Header));
            writer.Write(sanitizedName);

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

    private static void BuildTempAssetBundle(string outputDir, string name, string[] assetPaths, TargetPlatform target, Dictionary<TargetPlatform, string> bundles)
    {
        var sf = target.GetShortForm();
        var dir = Path.Combine(outputDir, sf);
        var buildMap = new AssetBundleBuild
        {
            assetBundleName = $"{name}.bundle_{sf}",
            assetNames = assetPaths,
        };
        PrepDirectory(dir);

        var manifest = BuildPipeline.BuildAssetBundles(dir, [buildMap], ABConfigs, target.GetBuildTarget());
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
