using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using XPAB.Internal;
using XPAB.Reader;

#if IL2CPP
using Il2CppInterop.Runtime;
#endif

namespace XPAB;

/// <summary>An asset bundle made to be cross-platform.</summary>
public sealed class XPAssetBundle : IDisposable
{
    private readonly Dictionary<(Type Type, string Path), UObject> loadedAssets = [];

    private bool shouldUnloadAssets;
    private AssetBundle bundle;
    private XPABReader reader;
    private bool bundleExists;
    private string name;

    private static readonly HashSet<Type> BundledTypes = [typeof(Shader)];

    private XPAssetBundle()
    {
    }

    /// <summary>Finalizes an instance of the <see cref="XPAssetBundle"/> class.</summary>
    ~XPAssetBundle() => Dispose(false);

    /// <summary>Loads an asset with the specified path.</summary>
    /// <typeparam name="T">The type of the asset.</typeparam>
    /// <param name="name">The name, or path, of the asset. Extension can be excluded.</param>
    /// <returns>The loaded asset.</returns>
    public T LoadAsset<T>(string name)
        where T : UObject
        => (T)loadedAssets.GetOrAdd((typeof(T), name), LoadInternal);

    private UObject LoadInternal((Type Type, string Name) assetParams)
    {
        if (bundleExists && BundledTypes.Contains(assetParams.Type))
        {
#if IL2CPP
            return bundle.LoadAsset(assetParams.Name, Il2CppType.From(assetParams.Type));
#else
            return bundle.LoadAsset(assetParams.Name, assetParams.Type);
#endif
        }

        return null!; // TODO: Actual asset loading
    }

    private void Initialise()
    {
        var length = reader.Length;

        if (length < 40)
            throw new InvalidDataException("File is too small to be a valid XPAB bundle.");

        reader.Position = 0;
        var header = Encoding.ASCII.GetString(reader.ReadBytes(4));

        if (header != Constants.Header)
            throw new InvalidDataException($"Invalid header signature: expected '{Constants.Header}', got '{header}'.");

        reader.Position = length - 36;
        var footer = Encoding.ASCII.GetString(reader.ReadBytes(4));

        if (footer != Constants.Footer)
            throw new InvalidDataException($"Invalid footer signature: expected '{Constants.Footer}', got '{footer}'.");

        var expectedHash = reader.ReadBytes(32);

        using var sha256 = SHA256.Create();
        reader.Position = 0;

        var hashableLength = length - 32;
        var buffer = new byte[8192];
        var remaining = hashableLength;

        while (remaining > 0)
        {
            var bytesToRead = (int)Math.Min(buffer.Length, remaining);
            var bytesRead = reader.Read(buffer, 0, bytesToRead);

            if (bytesRead == 0)
                throw new EndOfStreamException("Unexpected end of stream during hash calculation.");

            if (remaining == bytesRead)
                sha256.TransformFinalBlock(buffer, 0, bytesRead);
            else
                sha256.TransformBlock(buffer, 0, bytesRead, buffer, 0);

            remaining -= bytesRead;
        }

        var actualHash = sha256.Hash;

        if (actualHash == null || !expectedHash.SequenceEqual(actualHash))
            throw new InvalidDataException("File checksum verification failed. The bundle may be corrupted or tampered with.");

        reader.Position = 4;

        name = reader.ReadString();

        var bundleCount = reader.ReadPackedUInt32();

        if (bundleCount > 0)
        {
            var target = OtherExtensions.GetTargetPlatform();
            var i = 0;

            for (; i < bundleCount; i++)
            {
                var currentTarget = reader.ReadPackedEnum<TargetPlatform>();

                if (currentTarget == target)
                {
                    bundleExists = true;
                    break;
                }

                reader.Position += (long)reader.ReadPackedUInt64();
            }

            if (bundleExists)
            {
                var hashString = BitConverter.ToString(actualHash).Replace("-", string.Empty).ToLowerInvariant();
                var sf = target.GetShortForm();
                var bundlePath = Path.Combine(XPABCore.CachePath, $"{name}_{hashString}.bundle_{sf}");
                var embeddedLength = (long)reader.ReadPackedUInt64();
                var requiresExtraction = true;

                if (File.Exists(bundlePath))
                {
                    var cachedSize = new FileInfo(bundlePath).Length;

                    if (cachedSize == embeddedLength)
                        requiresExtraction = false;
                }

                if (requiresExtraction)
                {
                    var expectedLength = name.Length + 1 + hashString.Length + 8 + sf.Length;

                    foreach (var file in Directory.GetFiles(XPABCore.CachePath, $"{name}_*.bundle_{sf}"))
                    {
                        if (Path.GetFileName(file).Length == expectedLength)
                            File.Delete(file);
                    }

                    using var fs = File.Create(bundlePath);
                    reader.ReadToStream(fs, embeddedLength);
                }
                else
                {
                    reader.Position += embeddedLength;
                }

                bundle = AssetBundle.LoadFromFile(bundlePath);
            }
            else
            {
                throw new InvalidDataException($"Got indication that the {name} XPAB file contained an asset bundle, but couldn't find the appropriate bundle for platform: {target}");
            }

            for (i++; i < bundleCount; i++)
            {
                _ = reader.ReadPackedEnum<TargetPlatform>();
                reader.Position += (long)reader.ReadPackedUInt64();
            }
        }

        // TODO: String Pool, Master TOC and Asset Groups
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            reader?.Dispose();

            if (bundleExists && bundle)
                bundle.Unload(shouldUnloadAssets);

            // TODO: Unload non-bundled assets
        }
        else
        {
            XPABLogger.Warning($"XPAB file {name} was garbage collected without being disposed! Native Unity assets cannot be unloaded from the finalizer.");
        }
    }

    /// <summary>Unloads the bundle.</summary>
    public void Unload() => Dispose();

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Loads an XPAB file from a file path.</summary>
    /// <param name="path">The file path to the XPAB bundle.</param>
    /// <param name="unloadAssetsOnDispose">An optional flag to decide whether the already loaded assets need to be unloaded when the <see cref="XPAssetBundle"/> instance is disposed.</param>
    /// <returns>The loaded XPAB file.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the provided path is null.</exception>
    /// <exception cref="FileNotFoundException">Thrown if the file cannot be found on disk.</exception>
    /// <exception cref="InvalidDataException">Thrown if the stream contained malformed data.</exception>
    public static XPAssetBundle LoadFromFile(string path, bool unloadAssetsOnDispose = false)
        => LoadFromStream(File.OpenRead(path), unloadAssetsOnDispose);

    /// <summary>Loads an XPAB file from an embedded resource in the calling assembly.</summary>
    /// <param name="path">The exact manifest resource name of the embedded XPAB file.</param>
    /// <param name="unloadAssetsOnDispose">An optional flag to decide whether the already loaded assets need to be unloaded when the <see cref="XPAssetBundle"/> instance is disposed.</param>
    /// <returns>The loaded XPAB file.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the provided path is null, or if the embedded resource cannot be found.</exception>
    /// <exception cref="InvalidDataException">Thrown if the stream contained malformed data.</exception>
    public static XPAssetBundle LoadFromAssembly(string path, bool unloadAssetsOnDispose = false)
        => LoadFromAssembly(path, Assembly.GetCallingAssembly(), unloadAssetsOnDispose);

    /// <summary>Loads an XPAB file from an embedded resource in a specific assembly.</summary>
    /// <param name="path">The exact manifest resource name of the embedded XPAB file.</param>
    /// <param name="assembly">The assembly containing the embedded resource.</param>
    /// <param name="unloadAssetsOnDispose">An optional flag to decide whether the already loaded assets need to be unloaded when the <see cref="XPAssetBundle"/> instance is disposed.</param>
    /// <returns>The loaded XPAB file.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the provided path or assembly is null, or if the embedded resource cannot be found.</exception>
    /// <exception cref="InvalidDataException">Thrown if the stream contained malformed data.</exception>
    public static XPAssetBundle LoadFromAssembly(string path, Assembly? assembly, bool unloadAssetsOnDispose = false)
        => LoadFromStream(assembly?.GetManifestResourceStream(path), unloadAssetsOnDispose);

    /// <summary>Loads an XPAB file from a byte array.</summary>
    /// <param name="bytes">The byte array containing the XPAB file data.</param>
    /// <param name="unloadAssetsOnDispose">An optional flag to decide whether the already loaded assets need to be unloaded when the <see cref="XPAssetBundle"/> instance is disposed.</param>
    /// <returns>The loaded XPAB file.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the provided byte array is null.</exception>
    /// <exception cref="InvalidDataException">Thrown if the stream contained malformed data.</exception>
    public static XPAssetBundle LoadFromMemory(byte[] bytes, bool unloadAssetsOnDispose = false)
        => LoadFromStream(new MemoryStream(bytes), unloadAssetsOnDispose);

    /// <summary>Loads an XPAB file from a provided stream.</summary>
    /// <param name="stream">The stream that contains the file data.</param>
    /// <param name="unloadAssetsOnDispose">An optional flag to decide whether the already loaded assets need to be unloaded when the <see cref="XPAssetBundle"/> instance is disposed.</param>
    /// <returns>The loaded XPAB file.</returns>
    /// <exception cref="ArgumentNullException">Throw if the provided stream is null.</exception>
    /// <exception cref="InvalidDataException">Thrown if the stream contained malformed data.</exception>
    public static XPAssetBundle LoadFromStream(Stream? stream, bool unloadAssetsOnDispose = false)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        var bundle = new XPAssetBundle
        {
            shouldUnloadAssets = unloadAssetsOnDispose,
            reader = new XPABReader(stream),
        };

        try
        {
            bundle.Initialise();
            return bundle;
        }
        catch
        {
            bundle.reader?.Dispose();
            throw;
        }
    }
}
