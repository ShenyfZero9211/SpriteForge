using SkiaSharp;

namespace SpriteCore.Graphics;

/// <summary>
/// Processing 风格的纹理对象。包装 SkiaSharp SKBitmap，支持文件加载和基础缓存。
/// 对应 Processing 的 PImage。
/// </summary>
public class SPTexture : IDisposable
{
    private static readonly Dictionary<string, WeakReference<SPTexture>> _cache = new();

    /// <summary>底层 SkiaSharp 位图</summary>
    public SKBitmap Bitmap { get; }

    public int Width => Bitmap.Width;
    public int Height => Bitmap.Height;

    private SPTexture(SKBitmap bitmap)
    {
        Bitmap = bitmap;
    }

    /// <summary>
    /// 从文件路径加载纹理。支持 PNG/JPG/BMP/WebP。
    /// 重复加载同一路径时，优先返回缓存实例（弱引用）。
    /// </summary>
    public static SPTexture Load(string path)
    {
        var fullPath = Path.GetFullPath(path);

        lock (_cache)
        {
            if (_cache.TryGetValue(fullPath, out var weakRef) && weakRef.TryGetTarget(out var existing))
                return existing;
        }

        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"Image not found: {fullPath}", fullPath);

        var bitmap = SKBitmap.Decode(fullPath);
        if (bitmap == null)
            throw new InvalidOperationException($"Failed to decode image: {fullPath}");

        var texture = new SPTexture(bitmap);

        lock (_cache)
        {
            _cache[fullPath] = new WeakReference<SPTexture>(texture);
        }

        return texture;
    }

    public void Dispose()
    {
        lock (_cache)
        {
            // 从缓存中移除已释放的实例
            var toRemove = new List<string>();
            foreach (var kv in _cache)
            {
                if (kv.Value.TryGetTarget(out var target) && target == this)
                    toRemove.Add(kv.Key);
            }
            foreach (var key in toRemove)
                _cache.Remove(key);
        }
        Bitmap?.Dispose();
        GC.SuppressFinalize(this);
    }
}
