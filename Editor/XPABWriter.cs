using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using XPAB.Writer;

namespace XPAB;

/// <summary>A utility class that writes an XPAB file.</summary>
public static class XPABWriter
{
    private const BuildAssetBundleOptions ABConfigs = BuildAssetBundleOptions.UncompressedAssetBundle |
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
        var temp = Path.Combine(TempPath, $"temp_{config.fileName}");
        var filePath = Path.Combine(OutputPath, $"{config.fileName}.xpab");

        try
        {
            PrepDirectory(temp);

            config.targets = [.. config.targets.Distinct()];

            var bundleAssets = AssetDatabase.FindAssets($"l:xpab_{config.fileName}_bundle")
                .Select(AssetDatabase.GUIDToAssetPath)
                .ToArray();
            var hasBundles = bundleAssets.Length > 0 && config.targets.Length > 0;

            if (bundleAssets.Length > 0 && config.targets.Length == 0)
                Debug.LogWarning($"Assets were tagged for {config.fileName}, but no target platforms were configured in XPABConfig. Asset bundle building will be skipped.");

            if (bundleAssets.Length == 0 && config.targets.Length > 0)
                Debug.LogWarning($"Bundle platform targets for {config.fileName} were defined, but no assets were tagged for it. Asset bundle building will be skipped.");

            if (hasBundles)
            {
                foreach (var target in config.targets)
                    BuildTempAssetBundle(temp, config.fileName, bundleAssets, target);
            }

            using var fs = File.Create(filePath);

            fs.Write(Encoding.ASCII.GetBytes(Constants.Header));

            using var sha256 = SHA256.Create();
            using var cryptoStream = new CryptoStream(fs, sha256, CryptoStreamMode.Write, true);
            using var writer = new AssetWriter(cryptoStream, true);

            writer.WritePacked(Constants.FileVersion);

            if (hasBundles)
            {
                writer.WritePacked((uint)config.targets.Length);
                writer.WritePacked(Constants.AssetBundleVersion);

                foreach (var target in config.targets)
                {
                    writer.Write((byte)target);

                    var sf = target.GetShortForm();
                    var bundleFilePath = Path.Combine(temp, sf, $"{config.fileName}.bundle_{sf}");

                    using var fs2 = File.OpenRead(bundleFilePath);
                    writer.WritePacked((ulong)fs2.Length);

                    fs2.CopyTo(cryptoStream);
                }
            }
            else
            {
                writer.WritePacked(0u);
            }

            // String Pool, Assets and Master TOC to be written

            writer.Flush();
            cryptoStream.FlushFinalBlock();

            fs.Write(sha256.Hash);
            fs.Write(Encoding.ASCII.GetBytes(Constants.Footer));
        }
        finally
        {
            if (Directory.Exists(temp))
                Directory.Delete(temp, true);
        }
    }

    private static void BuildTempAssetBundle(string outputDir, string name, string[] assetPaths, TargetPlatform target)
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

        if (manifest == null || !File.Exists(Path.Combine(dir, buildMap.assetBundleName)))
            throw new SerializationException($"Failed to build platform bundle for {target}.");
    }

    private static void PrepDirectory(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, true);

        Directory.CreateDirectory(path);
    }
}
