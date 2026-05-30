using SkiaSharp;
using SpriteEngine.UI;
using Xunit;

namespace SpriteEngine.Tests;

public class UIElementTests
{
    private class TestElement : UIElement { }

    [Fact]
    public void DefaultProperties()
    {
        var el = new TestElement();
        Assert.Equal(0, el.LocalX);
        Assert.Equal(0, el.LocalY);
        Assert.Equal(0, el.Width);
        Assert.Equal(0, el.Height);
        Assert.True(el.Visible);
        Assert.True(el.Enabled);
        Assert.False(el.Focusable);
        Assert.Equal(0, el.ZOrder);
        Assert.Equal(1.0f, el.Alpha);
        Assert.Null(el.Parent);
    }

    [Fact]
    public void AbsolutePosition_NoParent_ReturnsLocal()
    {
        var el = new TestElement { LocalX = 10, LocalY = 20 };
        Assert.Equal(10, el.AbsoluteX);
        Assert.Equal(20, el.AbsoluteY);
    }

    [Fact]
    public void AbsolutePosition_WithParent_AccumulatesOffset()
    {
        var parent = new UIContainer { LocalX = 5, LocalY = 5, Padding = new Thickness(2, 3, 4, 5) };
        var child = new TestElement { LocalX = 10, LocalY = 20 };
        parent.AddChild(child);

        // parent.AbsoluteX = 5, ContentOffsetX = Padding.Left = 2
        // child.AbsoluteX = 5 + 2 + 10 = 17
        Assert.Equal(17, child.AbsoluteX);
        Assert.Equal(28, child.AbsoluteY);
    }

    [Fact]
    public void EffectiveAlpha_NoParent_ReturnsAlpha()
    {
        var el = new TestElement { Alpha = 0.5f };
        Assert.Equal(0.5f, el.EffectiveAlpha);
    }

    [Fact]
    public void EffectiveAlpha_WithParent_Multiplies()
    {
        var parent = new UIContainer { Alpha = 0.5f };
        var child = new TestElement { Alpha = 0.5f };
        parent.AddChild(child);

        Assert.Equal(0.25f, child.EffectiveAlpha);
    }

    [Fact]
    public void ResolvedStyle_UsesOverride()
    {
        var el = new TestElement
        {
            OverrideStyle = new UIStyle { FontSize = 99 }
        };
        Assert.Equal(99, el.ResolvedStyle.FontSize);
    }

    [Fact]
    public void ResolvedStyle_FallsBackToParent()
    {
        var parent = new UIContainer
        {
            OverrideStyle = new UIStyle { FontSize = 77 }
        };
        var child = new TestElement();
        parent.AddChild(child);

        Assert.Equal(77, child.ResolvedStyle.FontSize);
    }

    [Fact]
    public void ResolvedStyle_FallsBackToThemeDefault()
    {
        var el = new TestElement();
        Assert.Equal(UITheme.Default.FontSize, el.ResolvedStyle.FontSize);
    }

    [Fact]
    public void ContainsPoint_Inside_ReturnsTrue()
    {
        var el = new TestElement { LocalX = 10, LocalY = 10, Width = 50, Height = 30 };
        Assert.True(el.ContainsPoint(20, 20));
    }

    [Fact]
    public void ContainsPoint_Outside_ReturnsFalse()
    {
        var el = new TestElement { LocalX = 10, LocalY = 10, Width = 50, Height = 30 };
        Assert.False(el.ContainsPoint(5, 5));
    }

    [Fact]
    public void ContainsPoint_Invisible_ReturnsFalse()
    {
        var el = new TestElement { LocalX = 10, LocalY = 10, Width = 50, Height = 30, Visible = false };
        Assert.False(el.ContainsPoint(20, 20));
    }

    [Fact]
    public void HitTest_ReturnsSelfWhenHit()
    {
        var el = new TestElement { LocalX = 10, LocalY = 10, Width = 50, Height = 30 };
        Assert.Equal(el, el.HitTest(20, 20));
    }

    [Fact]
    public void HitTest_ReturnsNullWhenMiss()
    {
        var el = new TestElement { LocalX = 10, LocalY = 10, Width = 50, Height = 30 };
        Assert.Null(el.HitTest(5, 5));
    }

    [Fact]
    public void HitTest_ReturnsNullWhenInvisible()
    {
        var el = new TestElement { LocalX = 10, LocalY = 10, Width = 50, Height = 30, Visible = false };
        Assert.Null(el.HitTest(20, 20));
    }

    [Fact]
    public void Measure_SetsDesiredSizeFromExplicitSize()
    {
        var el = new TestElement { Width = 100, Height = 50 };
        el.Measure();
        Assert.Equal(100, el.DesiredWidth);
        Assert.Equal(50, el.DesiredHeight);
    }

    [Fact]
    public void OnEvent_DefaultReturnsFalse()
    {
        var el = new TestElement();
        Assert.False(el.OnEvent(UIEvent.MousePressed(0, 0, 0)));
    }
}
