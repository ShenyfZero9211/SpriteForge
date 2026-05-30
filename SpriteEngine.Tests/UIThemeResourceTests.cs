using SkiaSharp;
using SpriteEngine.Resource;
using SpriteEngine.UI;
using Xunit;

namespace SpriteEngine.Tests;

public class UIThemeResourceTests
{
    [Fact]
    public void Implements_IResource()
    {
        var theme = new UIThemeResource("themes/dark.json", UIStyle.DefaultDark());
        Assert.IsAssignableFrom<IResource>(theme);
    }

    [Fact]
    public void Path_IsSet()
    {
        var theme = new UIThemeResource("themes/dark.json", UIStyle.DefaultDark());
        Assert.Equal("themes/dark.json", theme.Path);
    }

    [Fact]
    public void Style_CanBeChanged()
    {
        var style = UIStyle.DefaultDark();
        var theme = new UIThemeResource("", style);

        theme.Style = new UIStyle { BackgroundColor = new SKColor(255, 0, 0) };

        Assert.Equal(new SKColor(255, 0, 0), theme.Style.BackgroundColor);
    }

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        var theme = new UIThemeResource("", UIStyle.DefaultDark());
        theme.Dispose();
    }

    [Fact]
    public void UITheme_Default_ReturnsStyle()
    {
        var style = UITheme.Default;
        // UIStyle.DefaultDark 可能返回 FontSize = 0，但 BackgroundColor 一定有值
        Assert.NotEqual(default(UIStyle), style);
    }

    [Fact]
    public void UITheme_Apply_SetsDefault()
    {
        var original = UITheme.Default;
        var theme = new UIThemeResource("", new UIStyle { BackgroundColor = new SKColor(10, 20, 30) });

        UITheme.Apply(theme);

        Assert.Equal(new SKColor(10, 20, 30), UITheme.Default.BackgroundColor);

        // 恢复
        UITheme.Default = original;
    }

    [Fact]
    public void UITheme_RegisterAndLoad()
    {
        var manager = new ResourceManager();
        var theme = new UIThemeResource("themes/test.json", new UIStyle { BackgroundColor = new SKColor(50, 60, 70) });

        UITheme.Register(manager, "themes/test.json", theme);
        var loaded = UITheme.Get(manager, "themes/test.json");

        Assert.NotNull(loaded);
        Assert.Equal(new SKColor(50, 60, 70), UITheme.Default.BackgroundColor);

        manager.DisposeAll();
    }
}
