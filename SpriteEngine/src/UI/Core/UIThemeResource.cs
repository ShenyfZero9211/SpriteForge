using SpriteEngine.Resource;

namespace SpriteEngine.UI;

/// <summary>
/// 可被 <see cref="ResourceManager"/> 管理的主题资源。
/// 包装一个 <see cref="UIStyle"/>，支持热加载和运行时切换。
/// </summary>
public class UIThemeResource : IResource
{
    public string Path { get; }

    /// <summary>当前主题样式</summary>
    public UIStyle Style { get; set; }

    public UIThemeResource(string path, UIStyle style)
    {
        Path = path;
        Style = style;
    }

    public void Dispose()
    {
        // UIStyle 是 struct，无托管资源需要释放
    }
}
