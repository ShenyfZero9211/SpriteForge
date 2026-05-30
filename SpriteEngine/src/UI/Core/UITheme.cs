using SpriteEngine.Resource;

namespace SpriteEngine.UI;

/// <summary>
/// UI 主题静态访问点。提供默认样式，支持从 <see cref="ResourceManager"/> 加载主题资源。
/// </summary>
public static class UITheme
{
    private static UIStyle _default = UIStyle.DefaultDark();
    private static UIThemeResource? _activeResource;

    /// <summary>全局默认样式</summary>
    public static UIStyle Default
    {
        get => _default;
        set => _default = value;
    }

    /// <summary>当前激活的主题资源（可为 null）</summary>
    public static UIThemeResource? ActiveResource => _activeResource;

    /// <summary>
    /// 从 <see cref="ResourceManager"/> 获取指定路径的主题资源。
    /// 如果资源不存在，返回 null。
    /// </summary>
    public static UIThemeResource? Get(ResourceManager manager, string path)
    {
        var resource = manager.Get<UIThemeResource>(path);
        if (resource != null)
        {
            _activeResource = resource;
            _default = resource.Style;
        }
        return resource;
    }

    /// <summary>
    /// 注册一个主题资源到 <see cref="ResourceManager"/>。
    /// 注册后可通过 <see cref="Get"/> 热切换。
    /// </summary>
    public static void Register(ResourceManager manager, string path, UIThemeResource resource)
    {
        manager.Register(path, resource);
    }

    /// <summary>应用已加载的主题资源样式到默认样式</summary>
    public static void Apply(UIThemeResource resource)
    {
        _activeResource = resource;
        _default = resource.Style;
    }
}
