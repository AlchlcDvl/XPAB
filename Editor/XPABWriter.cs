using System.Security.Cryptography;
using System.Text;

namespace XPAB;

/// <summary>A utility class that writes an XPAB file.</summary>
public static class XPABWriter
{
    private const BuildAssetBundleOptions ABConfigs = BuildAssetBundleOptions.UncompressedAssetBundle |
                                                      BuildAssetBundleOptions.ForceRebuildAssetBundle |
                                                      BuildAssetBundleOptions.AssetBundleStripUnityVersion;

    /// <summary>Creates an XPAB bundle it in the output path.</summary>
    /// <param name="config">The configuration that dictates the serialisation behaviour.</param>
    public static void CreateBundle(XPABConfig config)
    {
        var path = Path.Combine(config.path);
        var temp = Path.Combine(path, $"temp_{config.name}");
        var filePath = Path.Combine(path, $"{config.name}.xpab");

        try
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            PrepDirectory(temp);

            var bundleAssets = AssetDatabase.FindAssets($"l:xpab_{config.name}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Distinct()
                .ToArray();

            foreach (var target in config.targets)
                BuildTempAssetBundle(temp, config.name, bundleAssets, target);

            using var fs = File.Create(filePath);
            using var fileWriter = new BinaryWriter(fs);

            fileWriter.Write(Encoding.ASCII.GetBytes(Constants.Header));

            using var assets = new FileStream(filePath + "_assets", FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose);
            using var writer = new BinaryWriter(assets, Encoding.UTF8, true);

            writer.WritePacked(Constants.FileVersion);

            writer.WritePacked(Constants.AssetBundleVersion);
            writer.WritePacked((uint)config.targets.Length);

            foreach (var target in config.targets)
            {
                writer.Write((byte)target);

                var sf = target.GetShortForm();
                var bundleFilePath = Path.Combine(temp, sf, $"{config.name}.bundle_{sf}");

                writer.WriteBytesAndSize(File.ReadAllBytes(bundleFilePath));
            }

            // Assets to be written

            assets.Position = 0;
            using var sha256 = SHA256.Create();
            var checksumBytes = sha256.ComputeHash(assets);

            assets.Position = 0;
            assets.CopyTo(fs);

            fileWriter.Write(checksumBytes);
            fileWriter.Write(Encoding.ASCII.GetBytes(Constants.Footer));
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
        BuildPipeline.BuildAssetBundles(dir, [buildMap], ABConfigs, target.GetBuildTarget());
    }

    private static void PrepDirectory(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, true);

        Directory.CreateDirectory(path);
    }
}
