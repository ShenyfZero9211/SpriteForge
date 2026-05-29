using SpriteCore.Graphics;

namespace SpriteEngine.Resource;

/// <summary>
/// 引擎层纹理资源，包装 SpriteCore 的 SPTexture。
/// 由 ResourceManager 统一加载和释放。
/// </summary>
public class Texture2D : IResource
{
    public string Path { get; }
    public SPTexture Texture { get; }

    public int Width => Texture.Width;
    public int Height => Texture.Height;

    internal Texture2D(string path, SPTexture texture)
    {
        Path = path;
        Texture = texture;
    }

    public void Dispose()
    {
        Texture.Dispose();
        GC.SuppressFinalize(this);
    }
}
