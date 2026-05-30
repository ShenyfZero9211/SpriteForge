namespace SpriteEngine.UI;

/// <summary>
/// UI 主题静态访问点。提供默认样式。
/// 未来可扩展为 ResourceManager 管理的主题资源。
/// </summary>
public static class UITheme
{
    private static UIStyle _default = UIStyle.DefaultDark();

    /// <summary>全局默认样式</summary>
    public static UIStyle Default
    {
        get => _default;
        set => _default = value;
    }
}
